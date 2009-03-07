using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.Localisation;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Playlists;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;


namespace YouTubePlugin
{
  public class YoutubeGUIBase : GUIWindow
  {
    public Settings _setting = new Settings();
    protected PlayListPlayer playlistPlayer;
    public System.Timers.Timer updateStationLogoTimer = new System.Timers.Timer(0.3 * 1000);
    public WebClient Client = new WebClient();
    public Queue downloaQueue = new Queue();
    private DownloadFileObject curentDownlodingFile;
    protected YouTubeQuery.UploadTime uploadtime = YouTubeQuery.UploadTime.AllTime;

    public void SetLabels(YouTubeEntry vid, string type)
    {
      ClearLabels(type);
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Rating", vid.Rating.Average.ToString());
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Image", GetLocalImageFileName(GetBestUrl(vid.Media.Thumbnails)));
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Comments",vid.Comments.ToString());
      if (vid.Title.Text.Contains("-"))
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", vid.Title.Text.Split('-')[1]);
        if (type == "NowPlaying")
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", vid.Title.Text.Split('-')[1]);
        }
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", vid.Title.Text.Split('-')[0]);
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FanArt", GetFanArtImage(vid.Title.Text.Split('-')[0]));
      }
      else
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", vid.Title.Text);
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", " ");
      }
      //GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Image", GetLocalImageFileName(provider.GetArtistImage(vid.Artist.Id,400)));
      //if (vid.Albums.Count > 0)
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Album1.Title", vid.Albums[0].Release.Title);
      //if (vid.Albums.Count > 1)
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Album2.Title", vid.Albums[1].Release.Title);
      //if (vid.Categories.Count > 0)
      //{
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Category1.Title", vid.Categories[0].Name);
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Category1.Type", vid.Categories[0].Type.ToString());
      //}
      //if (vid.Categories.Count > 1)
      //{
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Category2.Title", vid.Categories[1].Name);
      //  GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Category2.Type", vid.Categories[1].Type.ToString());
      //}
    }

    public void ClearLabels(string type)
    {
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Image", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Comments", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Rating", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FanArt", " ");
    }

    public string FormatTitle(YouTubeEntry vid)
    {
      //if (string.IsNullOrEmpty(_setting.Format_Title))
      //  return vid.Video.Title;
      //string s = _setting.Format_Title;
      //s = s.Replace("%title%", vid.Video.Title);
      //s = s.Replace("%artist%", vid.Artist.Name);
      //s = s.Replace("%year%", vid.Video.CopyrightYear.ToString());
      //s = s.Replace("%rating%", vid.Video.Rating.ToString());
      return string.Format("{0}", vid.Title.Text);
    }
  
    public string GetBestUrl(ExtensionCollection<MediaThumbnail> th)
    {
      return th[th.Count - 1].Url;
    }

    public string GetLocalImageFileName(string strURL)
    {
      if (strURL == "")
        return string.Empty;
      string url = String.Format("youtubevideos-{0}.jpg", MediaPortal.Util.Utils.EncryptLine(strURL));
      return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url); ;
    }

    public bool filterVideoContens(YouTubeEntry vid)
    {
      if (_setting.MusicFilter && _setting.UseExtremFilter)
      {
        if (vid.Title.Text.Contains("-"))
          return true;
        else
          return false;
      }
      return true;
    }

    public void Err_message(int langid)
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(25660);
        dlgOK.SetLine(1, langid);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GetID);
      }
    }

    public void Err_message(string message)
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(25660);
        dlgOK.SetLine(1, message);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GetID);
      }
    }

    public void DoPlay(YouTubeEntry vid,bool fullscr)
    {
      bool isplaying = false;
      if (vid != null)
      {
        g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);

        if (_setting.UseYouTubePlayer)
        {
          if (vid.Media.Contents.Count > 0)
          {
            if (g_Player.Play(string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri))))
              isplaying = true;
          }
          else
          {
            if (g_Player.Play(vid.AlternateUri.ToString()))
              isplaying = true;
          }
        }
        else
        {
          VideoQuality qa = SelectQuality(vid);
          if (g_Player.PlayVideoStream(Youtube2MP.StreamPlaybackUrl(vid, qa)))
            isplaying = true;
        }

        if (isplaying && fullscr)
        {
          if (_setting.ShowNowPlaying)
          {
            GUIWindowManager.ActivateWindow(29052);
          }
          else
          {
            g_Player.ShowFullScreenWindow();
          }
        }
        if (!isplaying)
        {
          Err_message("Unable to playback the item ! ");
        }
      }
    }

    public VideoQuality SelectQuality(YouTubeEntry vid)
    {
      switch (Youtube2MP._settings.VideoQuality)
      {
        case 0:
          return VideoQuality.Normal;
        case 1:
          return VideoQuality.High;
        case 2:
          return VideoQuality.HD;
        case 3:
          {
            string title = vid.Title.Text;
            if (title.Contains("HQ"))
              return VideoQuality.High;
            if (title.Contains("HD"))
              return VideoQuality.HD;
            break;
          }
        case 4:
          {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) return VideoQuality.Normal;
            dlg.Reset();
            dlg.SetHeading("Select video quality");
            dlg.Add("Normal quality");
            dlg.Add("High quality");
            dlg.Add("HD quality");
            dlg.DoModal(GetID);
            if (dlg.SelectedId == -1) return VideoQuality.Normal;
            switch (dlg.SelectedLabel)
            {
              case 0:
                return VideoQuality.Normal;
              case 1:
                return VideoQuality.High;
              case 2:
                return VideoQuality.HD;
            }
          }
          break;
      }
      return VideoQuality.Normal;
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      g_Player.Release();
      g_Player.PlayBackStopped -= g_Player_PlayBackStopped;
      ClearLabels("NowPlaying");
    }

    public void AddItemToPlayList(GUIListItem pItem, VideoQuality qa)
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;
      PlayList playList;
      if (_setting.UseYouTubePlayer)
        playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
      else
        playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
      AddItemToPlayList(pItem, ref playList, qa);
    }

    public void AddItemToPlayList(GUIListItem pItem, ref PlayList playList,VideoQuality qa)
    {
      if (playList == null || pItem == null)
        return;
      string PlayblackUrl = "";
      
      List<GUIListItem> list = new List<GUIListItem>();
      YouTubeEntry vid = pItem.MusicTag as YouTubeEntry;
      if (vid != null)
      {
        if (!Youtube2MP._settings.UseYouTubePlayer)
        {
          PlayblackUrl = Youtube2MP.StreamPlaybackUrl(vid, qa);
        }
        else
        {
          if (vid.Media.Contents.Count > 0)
          {
            PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri));
          }
          else
          {
            PlayblackUrl = vid.AlternateUri.ToString();
          }
        }
        list.Add(pItem);
        PlayListItem playlistItem = new PlayListItem();
        playlistItem.Type = PlayListItem.PlayListItemType.VideoStream;// Playlists.PlayListItem.PlayListItemType.Audio;
        
        playlistItem.FileName = PlayblackUrl;
        playlistItem.Description = pItem.Label;
        MusicTag tag = new MusicTag();
        playlistItem.Duration = pItem.Duration;
        playlistItem.MusicTag = pItem.MusicTag;
        playList.Add(playlistItem);
      }
    }

    public string GetFanArtImage(string artist)
    {
      return String.Format(@"{0}\{1}_fanart.jpg", Thumbs.MusicArtists, MediaPortal.Util.Utils.MakeFileName(artist));
    }

    #region download manager



    private string DownloadImage(string Url)
    {
      //string localFile = GetLocalImageFileName(Url);
      //if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      //{
      //  downloaQueue.Enqueue(new DownloadFileObject(localFile, Url));
      //}
      return DownloadImage(Url, null);
    }

    private string DownloadImage(string Url, string localFile, GUIListItem item)
    {
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, Url, item));
      }
      return localFile;
    }

    public string DownloadImage(string Url, GUIListItem listitem)
    {
      string localFile = GetLocalImageFileName(Url);
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, Url, listitem));
      }
      return localFile;
    }

    public void OnDownloadTimedEvent(object source, ElapsedEventArgs e)
    {
      if (!Client.IsBusy && downloaQueue.Count > 0)
      {
        curentDownlodingFile = (DownloadFileObject)downloaQueue.Dequeue();
        Client.DownloadFileAsync(new Uri(curentDownlodingFile.Url), Path.GetTempPath() + @"\station.png");
      }
    }

    public void DownloadLogoEnd(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
        if (curentDownlodingFile.ListItem != null && File.Exists(curentDownlodingFile.FileName))
        {
          curentDownlodingFile.ListItem.ThumbnailImage = curentDownlodingFile.FileName;
          curentDownlodingFile.ListItem.IconImageBig = curentDownlodingFile.FileName;
          curentDownlodingFile.ListItem.RefreshCoverArt();
        }
        //UpdateGui();
      }
    }
   
    protected  YouTubeQuery SetParamToYouTubeQuery(YouTubeQuery query, bool safe)
    {

      //order results by the number of views (most viewed first)
      query.OrderBy = "viewCount";
      query.StartIndex = 1;
      if (_setting.UseExtremFilter)
        query.NumberToRetrieve = 50;
      else
        query.NumberToRetrieve = 20;
      ////exclude restricted content from the search
      //query.Racy = "exclude";
      query.SafeSearch = YouTubeQuery.SafeSearchValues.None;
      if (uploadtime != YouTubeQuery.UploadTime.AllTime)
        query.Time = uploadtime;
      if (_setting.MusicFilter && !safe)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      return query;
    }
    #endregion
  }
}
