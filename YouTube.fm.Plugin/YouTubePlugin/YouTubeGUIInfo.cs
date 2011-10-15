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
    [SkinControlAttribute(96)]
    protected GUIButtonControl infobutton = null;

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
      updateStationLogoTimer.Elapsed += OnDownloadTimedEvent;
      Client.DownloadFileCompleted += DownloadLogoEnd;
      Youtube2MP.player.PlayBegin += player_PlayBegin;
      Youtube2MP.player.PlayStop += player_PlayStop;
      Youtube2MP.temp_player.PlayBegin += player_PlayBegin;
      Youtube2MP.temp_player.PlayStop += player_PlayStop;
      Youtube2MP.player.Init();
      Youtube2MP.temp_player.Init();
      backgroundWorker.DoWork += backgroundWorker_DoWork;
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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == infobutton)
      {
        YoutubeGuiInfoEx scr = (YoutubeGuiInfoEx)GUIWindowManager.GetWindow(29053);
        scr.YouTubeEntry = Youtube2MP.NowPlayingEntry;
        GUIWindowManager.ActivateWindow(29053);
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
            HTBFanArt fanart = new HTBFanArt();
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

  }
}
