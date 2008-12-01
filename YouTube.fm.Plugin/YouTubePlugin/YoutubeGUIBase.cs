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
      //ClearLabels(type);
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Title", vid.Video.Title);
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Year", vid.Video.CopyrightYear.ToString());
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Rating", vid.Video.Rating.ToString());
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Image", GetLocalImageFileName(vid.Image.Url));
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Artist.Name", vid.Artist.Name);
      //GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Artist.Image", GetLocalImageFileName(provider.GetArtistImage(vid.Artist.Id,400)));
      //if (vid.Albums.Count > 0)
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Album1.Title", vid.Albums[0].Release.Title);
      //if (vid.Albums.Count > 1)
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Album2.Title", vid.Albums[1].Release.Title);
      //if (vid.Categories.Count > 0)
      //{
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category1.Title", vid.Categories[0].Name);
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category1.Type", vid.Categories[0].Type.ToString());
      //}
      //if (vid.Categories.Count > 1)
      //{
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category2.Title", vid.Categories[1].Name);
      //  GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category2.Type", vid.Categories[1].Type.ToString());
      //}
    }

    public void ClearLabels(string type)
    {
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Title", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Year", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Video.Rating", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Artist.Name", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Artist.Image", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Album1.Title", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Album2.Title", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category1.Title", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category1.Type", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category2.Title", " ");
      GUIPropertyManager.SetProperty("#YahooMusic." + type + ".Category2.Type", " ");
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
      
        //SetLabels(vid, "NowPlaying");
        //foreach (MediaContent mediaContent in vid.Media.Contents)
        //{
        //  Log.Debug("\tMedia Location: " + mediaContent.Attributes["url"].ToString());
        //  Log.Debug("\tMedia Type: " + mediaContent.Attributes["format"]);
        //  Log.Debug("\tDuration: " + mediaContent.Attributes["duration"]);
        //}
        if (vid.Media.Contents.Count > 0)
        {
          string PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", getIDSimple(vid.Id.AbsoluteUri));
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
          g_Player.PlayVideoStream(youtubecatch2(vid.AlternateUri.Content), vid.Title.Text);
          g_Player.ShowFullScreenWindow();
        }
      }
    }


    private string getIDSimple(string googleID)
    {
      int lastSlash = googleID.LastIndexOf("/");
      return googleID.Substring(lastSlash + 1);
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
          PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", getIDSimple(vid.Id.AbsoluteUri));
        }
        else
        {
          PlayblackUrl = youtubecatch2(vid.AlternateUri.Content);
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

    private string youtubecatch2(string url)
    {
      string str = getContent(url);
      StreamWriter wr = new StreamWriter(File.Create(MediaPortal.Util.Utils.EncryptLine(url)));
      wr.Write(str);
      wr.Close();
      int i = 0;
      int i1 = 0;
      string str1 = "/watch_fullscreen?";
      i = str.IndexOf(str1, System.StringComparison.CurrentCultureIgnoreCase);
      i1 = str.IndexOf(";", i, System.StringComparison.CurrentCultureIgnoreCase);
      string str3 = str.Substring(i + str1.Length, i1 - (i + str1.Length));
      string str7 = str3.Substring(str3.IndexOf("&title=") + 7);
      str7 = str7.Substring(0, str7.Length - 1);
      return string.Concat("http://youtube.com/get_video?", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")), str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));
    }

    private string getContent(string url)
    {
      string str;
      try
      {
        string str1 = "where=46038";
        System.Net.HttpWebRequest httpWebRequest = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentLength = (long)str1.Length;
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream());
        streamWriter.Write(str1);
        streamWriter.Close();
        System.IO.StreamReader streamReader = new System.IO.StreamReader((httpWebRequest.GetResponse() as System.Net.HttpWebResponse).GetResponseStream());
        str = streamReader.ReadToEnd();
        streamReader.Close();
      }
      catch (System.Exception exception1)
      {
        str = string.Concat("Error: ", exception1.Message.ToString());
      }
      return str;
    }
  }
}
