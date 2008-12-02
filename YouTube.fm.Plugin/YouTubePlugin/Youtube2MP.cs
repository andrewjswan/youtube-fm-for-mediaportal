using System;
using System.Collections.Generic;
using System.Text;

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
  static public class Youtube2MP
  {
    public static YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal", "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");

    public static Settings _settings;

    public static string PlaybackUrl(YouTubeEntry vid)
    {
      string PlayblackUrl = "";
      if (vid.Media.Contents.Count > 0)
      {
        PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", getIDSimple(vid.Id.AbsoluteUri));
      }
      else
      {
        PlayblackUrl = youtubecatch2(vid.AlternateUri.Content);
      }

      return PlayblackUrl;
    }

    public static Song YoutubeEntry2Song(YouTubeEntry en)
    {
      Song song = new Song();
      string title = en.Title.Text;
      if (title.Contains("-"))
      {
        song.Artist = title.Split('-')[0];
        song.Title = title.Split('-')[1];
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

    public static bool YoutubeEntry2Song(string fileurl, ref Song song)
    {
      YouTubeEntry en = null;
      if (fileurl.Contains("http://www.youtube.com/v/"))
      {
        String videoEntryUrl = "http://gdata.youtube.com/feeds/api/videos/" + getIDSimple(fileurl);
        //Log.Error("Get video id : {0}", videoEntryUrl);
        en = (YouTubeEntry)service.Get(videoEntryUrl);
      }
      if (en == null)
        return false;

      string title = en.Title.Text;
      if (title.Contains("-"))
      {
        song.Artist = title.Split('-')[0];
        song.Title = title.Split('-')[1];
      }
      else
        song.Artist = title;
      song.FileName = fileurl;

      return true;
    }

    static public string getIDSimple(string googleID)
    {
      int lastSlash = googleID.LastIndexOf("/");
      string id="";
      if (googleID.Contains("&"))
        id = googleID.Substring(lastSlash + 1, googleID.IndexOf('&') - lastSlash - 1);
      else
        id = googleID.Substring(lastSlash + 1);
      return id;
    }


    public static bool GetSongsByArtist(string artist,ref List<Song> songs)
    {
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      query.VQ = artist;
      //order results by the number of views (most viewed first)
      query.OrderBy = "viewCount";
      //exclude restricted content from the search
      query.NumberToRetrieve = 20;
      query.Racy = "exclude";
      query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));

      YouTubeFeed vidr = service.Query(query);
      foreach (YouTubeEntry entry in vidr.Entries)
      {
        if (entry.Title.Text.Contains("-"))
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
      return string.Concat("http://youtube.com/get_video?", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")), str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));
      //return string.Concat("http://www.youtube.com/v/", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")),"?",str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));
   
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
