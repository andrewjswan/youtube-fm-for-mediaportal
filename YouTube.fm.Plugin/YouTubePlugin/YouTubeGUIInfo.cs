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
using YouTubePlugin.Class.Artist;
using YouTubePlugin.DataProvider;
using Action = MediaPortal.GUI.Library.Action;

namespace YouTubePlugin
{

  public class YouTubeGUIInfo : YoutubeGUIBase
  {
    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIThumbnailPanel listControl = null;
    [SkinControlAttribute(166)]
    protected GUIListControl listsimilar = null;
    //[SkinControlAttribute(5)]
    //protected GUIButtonControl btnPlay = null;
    [SkinControlAttribute(95)]
    protected GUIImage imgFanArt = null;
    #endregion

    #region variabiles
    List<GUIListItem> relatated = new List<GUIListItem>();
    List<GUIListItem> similar = new List<GUIListItem>();
    public System.Timers.Timer infoTimer = new System.Timers.Timer(2 * 1000);
    private System.Timers.Timer _lastFmTimer = new System.Timers.Timer(60 * 1000);
    BackgroundWorker backgroundWorker = new BackgroundWorker();

    private static readonly object locker = new object();
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
          LoadRelatated();
          LoadSimilarArtists();
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

    void ClearLists()
    {
      relatated.Clear();
      similar.Clear();

      if (GUIWindowManager.ActiveWindow != GetID)
        return;

      lock (locker)
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(498); // menu
      dlg.AddLocalizedString(970);//prev screen
      dlg.Add("Fullscreen");
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      switch (dlg.SelectedLabel)
      { 
        case 0:
          GUIWindowManager.ShowPreviousWindow();
          break;
        case 1:
          g_Player.ShowFullScreenWindow();
          break;
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
            Log.Error("Youtube.Fm fanart NowPlaying not defined");
            if (imgFanArt != null) imgFanArt.Visible = false;
          }
        }
      }
      backgroundWorker.RunWorkerAsync();
    }

    private void LoadRelatated()
    {
      //Youtube2MP.getIDSimple(Youtube2MP.NowPlayingEntry.Id.AbsoluteUri));
      //GUIControl.ClearControl(GetID, listControl.GetID);
      string relatatedUrl = string.Format("http://gdata.youtube.com/feeds/api/videos/{0}/related",
                                         Youtube2MP.GetVideoId(Youtube2MP.NowPlayingEntry));
      relatated.Clear();
      YouTubeQuery query = new YouTubeQuery(relatatedUrl);
      YouTubeFeed vidr = Youtube2MP.service.Query(query);
      if (vidr.Entries.Count > 0)
      {
        addVideos(vidr, query);
      }
      if (listControl != null)
      {
        FillRelatedList();
      }
    }

    private void LoadSimilarArtists()
    {
      //if (listsimilar != null)
      {
        similar.Clear();
        string vidId = Youtube2MP.GetVideoId(Youtube2MP.NowPlayingEntry);
        ArtistItem artistItem = ArtistManager.Instance.SitesCache.GetByVideoId(vidId) != null
                                  ? ArtistManager.Instance.Grabber.GetFromVideoSite(
                                    ArtistManager.Instance.SitesCache.GetByVideoId(vidId).SIte)
                                  : ArtistManager.Instance.Grabber.GetFromVideoId(vidId);

        if (string.IsNullOrEmpty(artistItem.Id) && Youtube2MP.NowPlayingEntry.Title.Text.Contains("-"))
        {
          artistItem =
            ArtistManager.Instance.GetArtistsByName(Youtube2MP.NowPlayingEntry.Title.Text.Split('-')[0].TrimEnd());
        }

        if (!string.IsNullOrEmpty(artistItem.Id))
        {
          List<ArtistItem> items = ArtistManager.Instance.Grabber.GetSimilarArtists(artistItem.Id);
          //GUIControl.ClearControl(GetID, listsimilar.GetID);
          foreach (ArtistItem aitem in items)
          {
            GUIListItem item = new GUIListItem();
            // and add station name & bitrate
            item.Label = aitem.Name;
            item.Label2 = "";
            item.IsFolder = true;

            string imageFile = GetLocalImageFileName(aitem.Img_url);
            if (File.Exists(imageFile))
            {
              item.ThumbnailImage = imageFile;
              item.IconImage = "defaultVideoBig.png";
              item.IconImageBig = imageFile;
            }
            else
            {
              MediaPortal.Util.Utils.SetDefaultIcons(item);
              DownloadImage(aitem.Img_url, item);
              //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
            }
            item.MusicTag = aitem;
            similar.Add(item);
            //listsimilar.Add(item);
          }
          OnDownloadTimedEvent(null, null);
        }
        if (listsimilar != null)
        {
          FillSimilarList();
        }
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
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

    private void FillRelatedList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listControl == null)
          return;
      GUIControl.ClearControl(GetID, listControl.GetID);
      if (relatated == null || relatated.Count < 1) return;
      foreach (GUIListItem item in relatated)
      {
        listControl.Add(item);
      }
      listControl.SelectedListItemIndex = 0;
    }

    private void FillSimilarList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listsimilar == null)
          return;
      GUIControl.ClearControl(GetID, listsimilar.GetID);
      if (similar == null || similar.Count < 1) return;
      foreach (GUIListItem item in similar)
      {
          listsimilar.Add(item);
      }
      listsimilar.SelectedListItemIndex = 0;
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
    
    void addVideos(YouTubeFeed videos, YouTubeQuery qu)
    {
      foreach (YouTubeEntry entry in videos.Entries)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = entry.Title.Text; //ae.Entry.Author.Name + " - " + ae.Entry.Title.Content;
        item.Label2 = "";
        item.IsFolder = false;

        try
        {
          item.Duration = Convert.ToInt32(entry.Duration.Seconds, 10);
          if (entry.Rating != null)
            item.Rating = (float) entry.Rating.Average;
        }
        catch
        {

        }

        string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
        if (File.Exists(imageFile))
        {
          item.ThumbnailImage = imageFile;
          item.IconImage = imageFile; item.IconImageBig = imageFile;
        }
        else
        {
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += item_OnRetrieveArt;
          DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
          //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
        }
        item.MusicTag = entry;
        relatated.Add(item);
      }
      OnDownloadTimedEvent(null, null);
    }

    void item_OnRetrieveArt(GUIListItem item)
    {
      YouTubeEntry entry = item.MusicTag as YouTubeEntry;
      string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
      if (File.Exists(imageFile))
      {
        item.ThumbnailImage = imageFile;
        item.IconImage = "defaultVideoBig.png";
        item.IconImageBig = imageFile;
      }
    }

  }
}
