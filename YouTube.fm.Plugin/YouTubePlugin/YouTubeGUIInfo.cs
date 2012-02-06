using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Net;
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
using YouTubePlugin.Class.Database;
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
    private System.Timers.Timer _labelTimer = new System.Timers.Timer(15 * 1000);
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
      _labelTimer.Elapsed += _labelTimer_Elapsed;
    }

    void _labelTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      GUIPropertyManager.SetProperty("#Youtube.fm.FullScreen.ShowTitle", "false");
      GUIPropertyManager.SetProperty("#Youtube.fm.FullScreen.ShowNextTitle", "false");
      _labelTimer.Stop();
    }

    void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      // try to wait 3 sec to lock the thread 
      if (Monitor.TryEnter(similarlocker,3000))
      {
        try
        {
          LoadRelatated(Youtube2MP.NowPlayingEntry);
          LoadSimilarArtists(Youtube2MP.NowPlayingEntry);
        }
        finally
        {
          Monitor.Exit(similarlocker);
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


    void player_PlayBegin(YoutubePlaylistPlayer playlit, PlayListItem item)
    {
      try
      {
        //ClearLabels("NowPlaying");
        VideoInfo info = item.MusicTag as VideoInfo;
        if (info == null)
          return;

        Log.Debug("YouTube.fm playback started");
        YouTubeEntry en = info.Entry;

        if (en.Authors.Count == 0)
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

        en.Title.Text = item.Description;
        item.FileName = Youtube2MP.StreamPlaybackUrl(en, info);
        ClearLabels("NowPlaying",true);
        ClearLabels("Next", true);
        Youtube2MP.NowPlayingEntry = en;
        Youtube2MP.NextPlayingEntry = null;
        ArtistManager.Instance.SetSkinProperties(Youtube2MP.NextPlayingEntry, "NowPlaying", false, false);
        if (playlit.GetNextItem() != null)
        {
          VideoInfo nextinfo = playlit.GetNextItem().MusicTag as VideoInfo;
          if (nextinfo != null)
          {
            Youtube2MP.NextPlayingEntry = nextinfo.Entry;
            ArtistManager.Instance.SetSkinProperties(Youtube2MP.NextPlayingEntry, "Next", false, false);
          }
        }
        BackgroundWorker playBeginWorker = new BackgroundWorker();
        playBeginWorker.DoWork += playBeginWorker_DoWork;
        playBeginWorker.RunWorkerAsync();
        Youtube2MP.YouTubePlaying = true;
        GUIPropertyManager.SetProperty("#Youtube.fm.FullScreen.ShowTitle", "true");
        GUIPropertyManager.SetProperty("#Youtube.fm.FullScreen.ShowNextTitle",
                                       Youtube2MP.NextPlayingEntry != null ? "true" : "false");
        _labelTimer.Stop();
        _labelTimer.Start();
      }
      catch (Exception exception)
      {
        Log.Error("Youtube play begin exception");
        Log.Error(exception);
      }
    }

    void playBeginWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      GUIPropertyManager.SetProperty("#Play.Current.Artist",
                                     GUIPropertyManager.GetProperty("#Youtube.fm.NowPlaying.Artist.Name"));
      GUIPropertyManager.SetProperty("#Play.Current.Title",
                                     GUIPropertyManager.GetProperty("#Youtube.fm.NowPlaying.Video.Title"));

      if (Youtube2MP.NowPlayingEntry.Media != null)
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", Youtube2MP.GetLocalImageFileName(
          GetBestUrl(Youtube2MP.NowPlayingEntry.Media.Thumbnails)));

      if (Youtube2MP.NowPlayingEntry.Rating != null)
        GUIPropertyManager.SetProperty("#Play.Current.Rating",
                                       (Youtube2MP.NowPlayingEntry.Rating.Average*2).ToString());

      SetLabels(Youtube2MP.NowPlayingEntry, "NowPlaying");
      ArtistManager.Instance.SetSkinProperties(Youtube2MP.NowPlayingEntry, "NowPlaying", true, true);
      DatabaseProvider.InstanInstance.SavePlayData(Youtube2MP.NowPlayingEntry, DateTime.Now);
      relatated.Clear();
      similar.Clear();
      if (GUIWindowManager.ActiveWindow == (int) GetID)
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
      if (Youtube2MP._settings.LastFmNowPlay)
      {
        Youtube2MP.LastFmProfile.NowPlaying(Youtube2MP.NowPlayingEntry);
      }
      infoTimer.Enabled = true;
      _lastFmTimer.Start();


      if (Youtube2MP.NextPlayingEntry != null)
      {
        SetLabels(Youtube2MP.NextPlayingEntry, "Next");
        ArtistManager.Instance.SetSkinProperties(Youtube2MP.NextPlayingEntry, "Next", true, true);
      }
      else
      {
        ClearLabels("Next");
      }
    }

    public override bool Init()
    {
      infoTimer.Elapsed += updateStationLogoTimer_Elapsed;
      infoTimer.Enabled = false;
      infoTimer.Interval = 2 * 1000;
      _setting.Load();
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
      backgroundWorker.RunWorkerAsync();
      WebClient fanartclient = new WebClient();
      try
      {
        infoTimer.Enabled = false;
        lock (locker)
        {
          string file = GetFanArtImage(GUIPropertyManager.GetProperty("#Youtube.fm.NowPlaying.Artist.Name").Trim());

          if (File.Exists(file))
          {
            GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
          }
          else
          {
            if (Youtube2MP._settings.LoadOnlineFanart)
            {
              HTBFanArt fanart = new HTBFanArt();
              if (!File.Exists(file))
              {
                fanart.Search(GUIPropertyManager.GetProperty("#Youtube.fm.NowPlaying.Artist.Name").Trim());
                if (fanart.ImageUrls.Count > 0)
                {
                  Log.Debug("Youtube.Fm fanart download {0} to {1}  ", fanart.ImageUrls[0].Url, file);
                  fanartclient.DownloadFile(fanart.ImageUrls[0].Url, file);
                  GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
                }
              }
              else
              {
                GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
              }
            }
          }

          if (Youtube2MP.NowPlayingEntry != null)
          {
            Uri videoEntryUrl =
              new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(Youtube2MP.NowPlayingEntry));
            Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
            GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", video.Description);
            try
            {
              Feed<Comment> comments = Youtube2MP.request.GetComments(video);
              string cm = "\n------------------------------------------\n";
              foreach (Comment c in comments.Entries)
              {
                cm += c.Author + " : " + c.Content + "\n------------------------------------------\n";
              }
              GUIPropertyManager.SetProperty("#Play.Current.Plot", video.Description + cm);
              GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.Comments", video.Description + cm);
            }
            catch (Exception ex)
            {
              //Log.Error(ex);
            }
          }
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }


    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (Youtube2MP.NowPlayingEntry == null)
        return;
      GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/Now Playing");
      if (Monitor.TryEnter(locker,5000))
      {
        try
        {
          FillRelatedList();
          FillSimilarList();
          string file = GetFanArtImage(GUIPropertyManager.GetProperty("#Youtube.fm.NowPlaying.Artist.Name").Trim());

          if (File.Exists(file))
          {
            GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
          }
          else
          {
            GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", " ");
          }
        }
        finally
        {
          Monitor.Exit(locker);
        }
      }
      OnDownloadTimedEvent(null, null);
    }

  }
}
