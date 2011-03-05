using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Timers;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using Google.GData.YouTube;
using YouTubePlugin.DataProvider;
using Action = MediaPortal.GUI.Library.Action;

namespace YouTubePlugin
{

  public class YouTubeGUIInfo : YoutubeGUIBase
  {
    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIThumbnailPanel listControl = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnPlay = null;
    [SkinControlAttribute(95)]
    protected GUIImage imgFanArt = null;
    #endregion

#region variabiles
    List<GUIListItem> relatated = new List<GUIListItem>();
    public System.Timers.Timer infoTimer = new System.Timers.Timer(0.3 * 1000);

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

    public YouTubeGUIInfo()
    {
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = false;
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(OnDownloadTimedEvent);
      Client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadLogoEnd);
      Youtube2MP.player.PlayBegin += new YoutubePlaylistPlayer.EventHandler(player_PlayBegin);
      Youtube2MP.temp_player.PlayBegin += new YoutubePlaylistPlayer.EventHandler(player_PlayBegin);
      Youtube2MP.player.Init();
      Youtube2MP.temp_player.Init();
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
            item.FileName = Youtube2MP.StreamPlaybackUrl(en, info);
            Song song = Youtube2MP.YoutubeEntry2Song(en);
            
            Youtube2MP.NowPlayingEntry = en;
            Youtube2MP.NowPlayingSong = song;
            SetLabels(en, "NowPlaying");
            if (listControl != null)
            {
                GUIControl.ClearControl(GetID, listControl.GetID);
                relatated.Clear();
            }
            infoTimer.Enabled = true;
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
      infoTimer.Interval = 0.5 * 1000;
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
      if (Client.IsBusy)
        return;
      if (imgFanArt != null)
        imgFanArt.Visible = false;
      infoTimer.Enabled = false;
      if (Youtube2MP.NowPlayingSong != null)
      {
        Log.Debug("Youtube.Fm load fanart");
        GUIPropertyManager.SetProperty("#Play.Current.Title", Youtube2MP.NowPlayingSong.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Artist", Youtube2MP.NowPlayingSong.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", GetBestUrl(Youtube2MP.NowPlayingEntry.Media.Thumbnails));

        LoadRelatated();
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

        if (!Youtube2MP._settings.LoadOnlineFanart)
          return;

        file = GetFanArtImage(Youtube2MP.NowPlayingSong.Artist);
        if (!File.Exists(file))
        {
          fanart.Search(Youtube2MP.NowPlayingSong.Artist);
          Log.Debug("Youtube.Fm found {0} online fanarts for {1}", fanart.ImageUrls.Count, Youtube2MP.NowPlayingSong.Artist);
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
        if (listControl != null)
          GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else
      {
        Log.Error("Youtube.Fm fanart NowPlaying not defined");
        if (imgFanArt != null) imgFanArt.Visible = false;
      }
    }

    private void LoadRelatated()
    {
      if (listControl!=null)
      {
        string relatatedUrl = string.Format("http://gdata.youtube.com/feeds/api/videos/{0}/related", Youtube2MP.getIDSimple(Youtube2MP.NowPlayingEntry.Id.AbsoluteUri));
        GUIControl.ClearControl(GetID, listControl.GetID);
        relatated.Clear();
        YouTubeQuery query = new YouTubeQuery(relatatedUrl);
        YouTubeFeed vidr = Youtube2MP.service.Query(query);
        if (vidr.Entries.Count > 0)
        {
          addVideos(vidr, query);
        }
      }
    }

    protected override void OnPageLoad()
    {
      foreach (GUIListItem item in relatated)
      {
        listControl.Add(item);
      }
      //updateStationLogoTimer.Enabled = true;
     
      //if (Youtube2MP.NowPlayingEntry != null)
      //{
      //  if (File.Exists(GetFanArtImage(Youtube2MP.NowPlayingSong.Artist)))
      //  {
      //    GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", GetFanArtImage(Youtube2MP.NowPlayingSong.Artist));
      //    imgFanArt.Visible = true;
      //    updateStationLogoTimer.Enabled = false;
      //  }
      //  else
      //  {
      //    imgFanArt.Visible = false;
      //    updateStationLogoTimer.Enabled = true;
      //  }


      //}
      GUIControl.FocusControl(GetID, listControl.GetID);
      base.OnPageLoad();
    }


    void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      //      throw new Exception("The method or operation is not implemented.");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listControl)
      {
        // execute only for enter keys
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
            DoPlay(listControl.SelectedListItem.MusicTag as YouTubeEntry, false, null);
        }
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
          item.IconImage = "defaultVideoBig.png";
          item.IconImageBig = imageFile;
        }
        else
        {
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += item_OnRetrieveArt;
          DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
          //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
        }
        item.MusicTag = entry;
        listControl.Add(item);
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
