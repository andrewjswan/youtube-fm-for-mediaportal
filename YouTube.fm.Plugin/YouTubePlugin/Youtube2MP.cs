using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;

namespace YouTubePlugin
{
  public enum VideoQuality : int
  {
    Normal = 0,
    High = 1,
    HD = 2,
  }

  static public class Youtube2MP
  {
    public static YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal", "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");

    public static Settings _settings;

    public static Dictionary<string, YouTubeEntry> UrlHolder = new Dictionary<string, YouTubeEntry>();

    public static string StreamPlaybackUrl(YouTubeEntry vid, VideoQuality qu)
    {
      //return youtubecatch1(vid.Id.AbsoluteUri);
      string url = youtubecatch1(vid.AlternateUri.Content, qu);
      if (UrlHolder.ContainsKey(vid.Title.Text))
      {
        UrlHolder[vid.Title.Text] = vid;
      }
      else
      {
        UrlHolder.Add(vid.Title.Text, vid);
      }
      return url;
    }

    public static string StreamPlaybackUrl(string vidurl, VideoQuality qu)
    {
      return youtubecatch1("/" + getIDSimple(vidurl), qu);
    }

    public static string PlaybackUrl(YouTubeEntry vid)
    {
      string PlayblackUrl = "";
      if (_settings.UseYouTubePlayer)
      {
        if (vid.Media.Contents.Count > 0)
        {
          PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", getIDSimple(vid.Id.AbsoluteUri));
        }
        else
        {
          PlayblackUrl = vid.Id.AbsoluteUri;
        }
      }
      else
      {
        PlayblackUrl = vid.Id.AbsoluteUri;
      }

      return PlayblackUrl;
    }

    public static Song YoutubeEntry2Song(YouTubeEntry en)
    {
      Song song = new Song();
      string title = en.Title.Text;
      if (title.Contains("-"))
      {
        song.Artist = title.Split('-')[0].Trim();
        song.Title = title.Split('-')[1].Trim();
      }
      else
        song.Artist = title;
      song.FileName = PlaybackUrl(en);
      if (en.Media.Content != null)
      {
        song.Duration = Convert.ToInt32(en.Media.Content.Attributes["duration"].ToString(), 10);
      }
      return song;
    }

    public static bool YoutubeEntry2Song(string fileurl, ref Song song, ref YouTubeEntry en)
    {
      if (fileurl.Contains("youtube."))
      {
        String videoEntryUrl = "http://gdata.youtube.com/feeds/api/videos/" + getIDSimple(fileurl);
        en = (YouTubeEntry)service.Get(videoEntryUrl);
      }
      if (en == null)
        return false;

      string title = en.Title.Text;
      if (title.Contains("-"))
      {
        song.Artist = title.Split('-')[0].Trim();
        song.Title = title.Split('-')[1].Trim();
      }
      else
        song.Artist = title;

      song.FileName = fileurl;
      song.Duration = Convert.ToInt32(en.Duration.Seconds);
      song.Track = 0;
      song.URL = fileurl;
      song.TimesPlayed = 1;

      return true;
    }

    
    static private YouTubeEntry nowplayingentry;

    static public YouTubeEntry NowPlayingEntry
    {
      get { return nowplayingentry; }
      set { nowplayingentry = value; }
    }

    static private Song nowPlayingSong;

    static public Song NowPlayingSong
    {
      get { return nowPlayingSong; }
      set { nowPlayingSong = value; }
    }

    
    public static bool YoutubeEntry2Song(string fileurl, ref Song song)
    {
      YouTubeEntry en = null;
      return YoutubeEntry2Song(fileurl, ref song, ref en);
    }

    static public string getIDSimple(string googleID)
    {
      string id="";
      if (googleID.Contains("video_id"))
      {
        Uri erl = new Uri(googleID);
        string[] param = erl.Query.Substring(1).Split('&');
        foreach (string s in param)
        {
          if (s.Split('=')[0] == "video_id")
          {
            id = s.Split('=')[1];
          }
        }
      }
      else if (googleID.Contains("video:"))
      {
          int lastVideo = googleID.LastIndexOf("video:");
          if (googleID.IndexOf(":", lastVideo+6) != -1)
              id = googleID.Substring(lastVideo+6, googleID.IndexOf(":", lastVideo+6)-(lastVideo+6));
          else
              id = googleID.Substring(lastVideo+6);
      }
      else if (googleID.Contains("v="))
      {
        Uri erl = new Uri(googleID);
        string[] param = erl.Query.Substring(1).Split('&');
        foreach (string s in param)
        {
          if (s.Split('=')[0] == "v")
          {
            id = s.Split('=')[1];
          }
        }
      }
      else
      {
        int lastSlash = googleID.LastIndexOf("/");
        if (googleID.Contains("&"))
          id = googleID.Substring(lastSlash + 1, googleID.IndexOf('&') - lastSlash - 1);
        else
          id = googleID.Substring(lastSlash + 1);
      }
      return id;
    }


    public static bool GetSongsByArtist(string artist,ref List<Song> songs,ref YouTubeFeed vidr)
    {
      Log.Debug("Youtube GetSongsByArtist for : {0}", artist);
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      //query.VQ = artist;
      query.Query = artist;
      //order results by the number of views (most viewed first)
      query.OrderBy = "relevance";
      //exclude restricted content from the search
      query.NumberToRetrieve = 20;
      //query.Racy = "exclude";
      query.SafeSearch = YouTubeQuery.SafeSearchValues.Strict;
      query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));

      vidr = service.Query(query);
      foreach (YouTubeEntry entry in vidr.Entries)
      {
        if (entry.Title.Text.ToUpper().Contains(artist.ToUpper().Trim())&&entry.Title.Text.Contains("-"))
          songs.Add(YoutubeEntry2Song(entry));
      }
      return true;
    }

    static public string youtubecatch2(string url)
    {
      string str = getContent(url);
      int i = 0;
      int i1 = 0;
      string str1 = "/watch_fullscreen?";
      i = str.IndexOf(str1, System.StringComparison.CurrentCultureIgnoreCase);
      i1 = str.IndexOf(";", i, System.StringComparison.CurrentCultureIgnoreCase);
      string str3 = str.Substring(i + str1.Length, i1 - (i + str1.Length));
      string str7 = str3.Substring(str3.IndexOf("&title=") + 7);
      str7 = str7.Substring(0, str7.Length - 1);
      return string.Concat("http://youtube.com/get_video?", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")), str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));//+ "&fmt=18"
      //return string.Concat("http://www.youtube.com/v/", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")),"?",str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));
   
    }

    static public string youtubecatch1(string url,VideoQuality qa)
    {
      Stream response = RetrieveData(string.Format("http://www.youtube.com/api2_rest?method=youtube.videos.get_video_token&video_id={0}", getIDSimple(url)));
      if (response == null)
        return "";
      StreamReader reader = new StreamReader(response, System.Text.Encoding.UTF8, true);
      String sXmlData = reader.ReadToEnd().Replace('\0', ' ');
      response.Close();
      reader.Close();
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(sXmlData);
      XmlNode node = doc.SelectSingleNode("/ut_response/t");
      switch (qa)
      {
        case VideoQuality.Normal:
          return string.Format("http://youtube.com/get_video?video_id={0}&t={1}&ext=.flv", getIDSimple(url), node.InnerText);
        case VideoQuality.High:
          return string.Format("http://youtube.com/get_video?video_id={0}&t={1}&fmt=18&ext=.flv", getIDSimple(url), node.InnerText);
        case VideoQuality.HD:
          return string.Format("http://youtube.com/get_video?video_id={0}&t={1}&fmt=22&ext=.flv", getIDSimple(url), node.InnerText);
      }
      return string.Format("http://youtube.com/get_video?video_id={0}&t={1}&ext=.flv", getIDSimple(url), node.InnerText);
    }

    static private Stream RetrieveData(string sUrl)
    {
      if (sUrl == null || sUrl.Length < 1 || sUrl[0] == '/')
      {
        return null;
      }
      //sUrl = this.Settings.UpdateUrl(sUrl);
      HttpWebRequest request = null;
      HttpWebResponse response = null;
      try
      {
        request = (HttpWebRequest)WebRequest.Create(sUrl);
        request.Timeout = 20000;
        response = (HttpWebResponse)request.GetResponse();

        if (response != null) // Get the stream associated with the response.
          return response.GetResponseStream();

      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      finally
      {
        //if (response != null) response.Close(); // screws up the decompression
      }

      return null;
    }

    static private string getContent(string url)
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
        Log.Error(exception1);
      }
      return str;
    }
  }
}
