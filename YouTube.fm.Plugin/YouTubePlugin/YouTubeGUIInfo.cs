using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using Google.GData.Client;
using Google.YouTube;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using Google.GData.YouTube;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.DataProvider;
using Action = MediaPortal.GUI.Library.Action;

namespace YouTubePlugin
{

  public class YouTubeGUIInfo : YouTubeGuiInfoBase
  {


    #region variabiles
    public System.Timers.Timer infoTimer = new System.Timers.Timer(2 * 1000);
    private System.Timers.Timer _lastFmTimer = new System.Timers.Timer(60 * 1000);
    BackgroundWorker backgroundWorker = new BackgroundWorker();

    #endregion

    public override int GetID
    {
      get
      {
        return 29052;
      }
      set
      {
      }
    }

    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    public YouTubeGUIInfo()
    {
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = false;
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(OnDownloadTimedEvent);
      Client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadLogoEnd);
      Youtube2MP.player.PlayBegin += new YoutubePlaylistPlayer.EventHandler(player_PlayBegin);
      Youtube2MP.player.PlayStop += new YoutubePlaylistPlayer.StopEventHandler(player_PlayStop);
      Youtube2MP.temp_player.PlayBegin += new YoutubePlaylistPlayer.EventHandler(player_PlayBegin);
      Youtube2MP.temp_player.PlayStop += new YoutubePlaylistPlayer.StopEventHandler(player_PlayStop);
      Youtube2MP.player.Init();
      Youtube2MP.temp_player.Init();
      backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
      _lastFmTimer.Elapsed += _lastFmTimer_Elapsed;
    }

    void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      if (Monitor.TryEnter(locker))
      {
        try
        {
          LoadRelatated(Youtube2MP.NowPlayingEntry);
          LoadSimilarArtists(Youtube2MP.NowPlayingEntry);
        }
        finally
        {
          Monitor.Exit(locker);
        }
      }
    }

    void _lastFmTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (Youtube2MP._settings.LastFmSubmit)
      {
        Youtube2MP.LastFmProfile.Submit(Youtube2MP.NowPlayingEntry);
      } 
      _lastFmTimer.Stop();
    }

    void player_PlayStop()
    {
      infoTimer.Enabled = false;
      _lastFmTimer.Enabled = false;
      ClearLists();
    }


    void player_PlayBegin(PlayListItem item)
    {
      try
      {
        ClearLabels("NowPlaying");
        VideoInfo info = item.MusicTag as VideoInfo;
        if (info == null)
          return;

        Log.Debug("YouTube.fm playback started");
        YouTubeEntry en = info.Entry;

        if (en.Authors.Count == 0 )
        {
          Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(en));
          try
          {
            Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
            en = video.YouTubeEntry;
          }
          catch (Exception)
          {
            //vid = null;
          }
        }

        item.FileName = Youtube2MP.StreamPlaybackUrl(en, info);
        Song song = Youtube2MP.YoutubeEntry2Song(en);
            
        Youtube2MP.NowPlayingEntry = en;
        Youtube2MP.NowPlayingSong = song;
        SetLabels(en, "NowPlaying");
        relatated.Clear();
        similar.Clear();
        if (GUIWindowManager.ActiveWindow == (int)GetID)
        {
          if (listControl != null)
          {
            GUIControl.ClearControl(GetID, listControl.GetID);
          }
          if (listsimilar != null)
          {
            GUIControl.ClearControl(GetID, listsimilar.GetID);
          }
        }
        if(Youtube2MP._settings.LastFmNowPlay)
        {
          Youtube2MP.LastFmProfile.NowPlaying(Youtube2MP.NowPlayingEntry);
        }
        infoTimer.Enabled = true;
        _lastFmTimer.Start();
      }
      catch (Exception exception)
      {
        Log.Error("Youtube play begin exception");
        Log.Error(exception);
      }
    }

    public override bool Init()
    {
      infoTimer.Elapsed += new ElapsedEventHandler(updateStationLogoTimer_Elapsed);
      infoTimer.Enabled = false;
      infoTimer.Interval = 2 * 1000;
      return Load(GUIGraphicsContext.Skin + @"\youtubeinfo.xml");
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem selectedItem = listControl.SelectedListItem;
      YouTubeEntry videoEntry = selectedItem.MusicTag as YouTubeEntry;
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(498); // menu
      dlg.Add(Translation.ShowPreviousWindow);
      dlg.Add(Translation.Fullscreen);
      if (videoEntry != null)
      {
        dlg.Add(Translation.AddPlaylist);
        dlg.Add(Translation.AddAllPlaylist);
        if (Youtube2MP.service.Credentials != null)
        {
          dlg.Add(Translation.AddFavorites);
          dlg.Add(Translation.AddWatchLater);
        }
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      if (dlg.SelectedLabelText == Translation.ShowPreviousWindow)
      {
        GUIWindowManager.ShowPreviousWindow();
      }
      else if (dlg.SelectedLabelText == Translation.Fullscreen)
      {
        g_Player.ShowFullScreenWindow();
      }
      else if (dlg.SelectedLabelText == Translation.AddPlaylist)
      {
        VideoInfo inf = SelectQuality(videoEntry);
        if (inf.Quality != VideoQuality.Unknow)
        {
          AddItemToPlayList(selectedItem, inf);
        }
      }
      else if (dlg.SelectedLabelText == Translation.AddAllPlaylist)
      {

        VideoInfo inf = SelectQuality(videoEntry);
        inf.Items = new Dictionary<string, string>();
        if (inf.Quality != VideoQuality.Unknow)
        {
          GUIDialogProgress dlgProgress =
            (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
          if (dlgProgress != null)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(Translation.AddAllPlaylist);
            dlgProgress.SetLine(1, "");
            dlgProgress.SetLine(2, "");
            dlgProgress.SetPercentage(0);
            dlgProgress.Progress();
            dlgProgress.ShowProgressBar(true);
            dlgProgress.StartModal(GetID);
          }
          int i = 0;
          for (int j = 0; j < listControl.Count; j++)
          {
            GUIListItem item = listControl[j];
            if (dlgProgress != null)
            {
              double pr = ((double) i/(double) listControl.Count)*100;
              dlgProgress.SetLine(1, item.Label);
              dlgProgress.SetLine(2, i.ToString() + "/" + listControl.Count.ToString());
              dlgProgress.SetPercentage((int) pr);
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled)
                break;
            }
            i++;
            AddItemToPlayList(item, new VideoInfo(inf));
          }
          if (dlgProgress != null)
            dlgProgress.Close();
        }
      }else if (dlg.SelectedLabelText == Translation.AddFavorites)
      {
        try
        {
         Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreateFavoritesUri(null)), videoEntry);
        }
        catch (Exception)
        {
          Err_message(Translation.WrongRequestWrongUser);
        }

      }else if (dlg.SelectedLabelText == Translation.AddWatchLater)
      {
        PlayListMember pm = new PlayListMember();
        pm.Id = videoEntry.VideoId;
        Youtube2MP.request.Insert(new Uri("https://gdata.youtube.com/feeds/api/users/default/watch_later"), pm);
      }
    }

    void updateStationLogoTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      infoTimer.Enabled = false;
      lock (locker)
      {
        //LoadRelatated();
        //LoadSimilarArtists();
        //do we really need to focus? skin will set default focus when entering the screen, and not good to mess with focus while user has window open (is navigating in the window)
        //if (listControl != null)
        //  GUIControl.FocusControl(GetID, listControl.GetID););
        if (imgFanArt != null)
          imgFanArt.Visible = false;
        if (Youtube2MP.NowPlayingSong != null)
        {
          Uri videoEntryUrl =
            new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(Youtube2MP.NowPlayingEntry));
          Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);

          Log.Debug("Youtube.Fm load fanart");
          GUIPropertyManager.SetProperty("#Play.Current.Title", Youtube2MP.NowPlayingSong.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", Youtube2MP.NowPlayingSong.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", GetBestUrl(Youtube2MP.NowPlayingEntry.Media.Thumbnails));
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", video.Description);

          if (Youtube2MP.NowPlayingEntry.Rating != null)
            GUIPropertyManager.SetProperty("#Play.Current.Rating",
                                           (Youtube2MP.NowPlayingEntry.Rating.Average*2).ToString());

          try
          {
            Feed<Comment> comments = Youtube2MP.request.GetComments(video);
            string cm = "\n------------------------------------------\n";
            foreach (Comment c in comments.Entries)
            {
              cm +=c.Author+" : "+ c.Content + "\n------------------------------------------\n";
            }
            GUIPropertyManager.SetProperty("#Play.Current.Plot", video.Description + cm);
          }
          catch (Exception ex)
          {
            //Log.Error(ex);
          }

          HTBFanArt fanart = new HTBFanArt();
          string file = Youtube2MP._settings.FanartDir.Replace("%artist%", Youtube2MP.NowPlayingSong.Artist);

          if (File.Exists(file) && imgFanArt != null)
          {
            Log.Debug("Youtube.Fm local fanart {0} loaded ", file);
            imgFanArt.Visible = true;
            imgFanArt.FileName = file;
            imgFanArt.DoUpdate();
            return;
          }

          if (Youtube2MP._settings.LoadOnlineFanart && !Client.IsBusy)
          {
            file = GetFanArtImage(Youtube2MP.NowPlayingSong.Artist);
            if (!File.Exists(file))
            {
              fanart.Search(Youtube2MP.NowPlayingSong.Artist);
              Log.Debug("Youtube.Fm found {0} online fanarts for {1}", fanart.ImageUrls.Count,
                        Youtube2MP.NowPlayingSong.Artist);
              if (fanart.ImageUrls.Count > 0)
              {
                Log.Debug("Youtube.Fm fanart download {0} to {1}  ", fanart.ImageUrls[0].Url, file);
                Client.DownloadFile(fanart.ImageUrls[0].Url, file);
                GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
                Log.Debug("Youtube.Fm fanart {0} loaded ", file);
                if (imgFanArt != null)
                {
                  imgFanArt.Visible = true;
                  imgFanArt.FileName = file;
                  imgFanArt.DoUpdate();
                }
              }
              else
              {
                if (imgFanArt != null) imgFanArt.Visible = false;
              }
            }
            else
            {
              GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
              if (imgFanArt != null)
              {
                imgFanArt.Visible = true;
                imgFanArt.FileName = file;
                imgFanArt.DoUpdate();
              }
            }
          }
          else
          {
            //Log.Error("Youtube.Fm fanart NowPlaying not defined");
            if (imgFanArt != null) imgFanArt.Visible = false;
          }
        }
      }
      backgroundWorker.RunWorkerAsync();
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/Now Playing");
      if (Monitor.TryEnter(locker))
      {
        try
        {
          FillRelatedList();
          FillSimilarList();
        }
        finally
        {
          Monitor.Exit(locker);
        }
      }
      //leave the focus to the skin
      //GUIControl.FocusControl(GetID, listControl.GetID);
    }


    void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      //      throw new Exception("The method or operation is not implemented.");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listControl && actionType == Action.ActionType.ACTION_SELECT_ITEM && listControl.SelectedListItem != null)
      {
        DoPlay(listControl.SelectedListItem.MusicTag as YouTubeEntry, false, null);
      }
      else if (control == listsimilar && actionType == Action.ActionType.ACTION_SELECT_ITEM && listsimilar.SelectedListItem != null)
      {
        ArtistItem artistItem = listsimilar.SelectedListItem.MusicTag as ArtistItem;
        MessageGUI.Item = artistItem;
        GUIWindowManager.ActivateWindow(29050);
      }
      base.OnClicked(controlId, control, actionType);
    }

  }
}
