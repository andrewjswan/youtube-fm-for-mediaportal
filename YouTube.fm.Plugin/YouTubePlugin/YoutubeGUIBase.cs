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

    public string GetBestUrl(ThumbnailCollection th)
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
      //if (_setting.ShowPlayableOnly && string.IsNullOrEmpty(vid.Video.Id))
      //  return false;
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

    public void DoPlay(YouTubeEntry vid)
    {
      if (vid != null)
      {
      
        if (vid.Media.Contents.Count > 0)
        {
          string PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri));
          if (!string.IsNullOrEmpty(PlayblackUrl))
          {
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
            g_Player.Play(PlayblackUrl);
            g_Player.ShowFullScreenWindow();
          }
          else
          {
            Err_message("Error Playing file !!!");
          }
        }
        else
        {
          g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
          //g_Player.PlayVideoStream(Youtube2MP.youtubecatch1(vid.AlternateUri.Content), vid.Title.Text);
          g_Player.PlayVideoStream(Youtube2MP.youtubecatch1(vid.Id.AbsoluteUri), vid.Title.Text);
          g_Player.ShowFullScreenWindow();
        }
      }
    }


    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      g_Player.Release();
      g_Player.PlayBackStopped -= g_Player_PlayBackStopped;
      ClearLabels("NowPlaying");
    }

    public void AddItemToPlayList(GUIListItem pItem)
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
      AddItemToPlayList(pItem, ref playList);
    }

    public void AddItemToPlayList(GUIListItem pItem, ref PlayList playList)
    {
      if (playList == null || pItem == null)
        return;
      string PlayblackUrl = "";
      
      List<GUIListItem> list = new List<GUIListItem>();
      YouTubeEntry vid = pItem.MusicTag as YouTubeEntry;
      if (vid != null)
      {
        if (vid.Media.Contents.Count > 0)
        {
          PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri));
        }
        else
        {
          PlayblackUrl = Youtube2MP.youtubecatch1(vid.Id.AbsoluteUri);
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

  }
}
