using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Google.GData.YouTube;
using Google.YouTube;
using MediaPortal.Util;
using YouTubePlugin.Class;
using YouTubePlugin.Class.SiteItems;
using PlayList = YouTubePlugin.Class.SiteItems.PlayList;
using Statistics = YouTubePlugin.Class.SiteItems.Statistics;

namespace YouTubePlugin
{
  public enum VideoQuality : int
  {
    Normal = 0,
    High = 1,
    HD = 2,
    FullHD = 3,
    Unknow = 4,
  }
  static public class Youtube2MP
  {
    public const int ITEM_IN_LIST = 25;
    public static bool PlayBegin { get; set; }
    public static bool YouTubePlaying { get; set; }
    public static FileDownloader VideoDownloader { get; set; }

    public static YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");

    public static YoutubePlaylistPlayer player = new YoutubePlaylistPlayer();

    public static YoutubePlaylistPlayer temp_player = new YoutubePlaylistPlayer();


    public static YouTubeRequest request = new YouTubeRequest(new YouTubeRequestSettings("My YouTube Videos For MediaPortal", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg"));
    public static Settings _settings;

    public static Dictionary<string, YouTubeEntry> UrlHolder = new Dictionary<string, YouTubeEntry>();

    public static LastProfile LastFmProfile { get; set; }

    static Youtube2MP()
    {
      PlayBegin = false;
      YouTubePlaying = false;
      AddSiteItem(new StandardFeedItem());
      AddSiteItem(new SearchVideo());
      AddSiteItem(new SearchHistory());
      AddSiteItem(new UserVideos());
      AddSiteItem(new FavoritesVideos());
      AddSiteItem(new VevoVideos());
      AddSiteItem(new ArtistView());
      AddSiteItem(new PlayList());
      AddSiteItem(new Folder());
      AddSiteItem(new VideoItem());
      AddSiteItem(new LastFmTopTracks());
      AddSiteItem(new LastFmUser());
      AddSiteItem(new BillboardItem());
      AddSiteItem(new Statistics());
      AddSiteItem(new UserDownloadedVideos());
      AddSiteItem(new Disco());
      AddSiteItem(new Browse());
      AddSiteItem(new UserPlaylists());
      AddSiteItem(new UserChannel());
      AddSiteItem(new Featured());
    }

    public static Dictionary<string, ISiteItem> SiteItemProvider = new Dictionary<string, ISiteItem>();


    static public void AddSiteItem(ISiteItem siteItem)
    {
      SiteItemProvider.Add(siteItem.Name, siteItem);
    }

    static public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      return SiteItemProvider[entry.Provider].GetList(entry);
    }

    static public GenericListItemCollections GetHomeMenu()
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = "Home";
      foreach (SiteItemEntry itemEntry in _settings.MainMenu.Items)
      {
        if (String.IsNullOrEmpty(itemEntry.ParentFolder))
          res.Add(SiteItemProvider[itemEntry.Provider].HomeGetList(itemEntry));
      }
      foreach (GenericListItem genericListItem in res.Items)
      {
        string file = GUIGraphicsContext.Skin + "\\Media\\Youtube.Fm\\" + genericListItem.Title + ".png";
        if (File.Exists(file))
          genericListItem.DefaultImage = file;
      }
      return res;
    }

    public static string GetVideoId(YouTubeEntry vid)
    {
      if (!String.IsNullOrEmpty(vid.VideoId))
        return vid.VideoId;
      return getIDSimple(vid.Id.AbsoluteUri);
    }


    public static string StreamPlaybackUrl(YouTubeEntry vid, VideoInfo qu)
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

    public static string StreamPlaybackUrl(string vidurl, VideoInfo qu)
    {
      return youtubecatch1("/" + getIDSimple(vidurl), qu);
    }

    public static string PlaybackUrl(YouTubeEntry vid)
    {
      string PlayblackUrl = "";
      PlayblackUrl = vid.Id.AbsoluteUri;
      return PlayblackUrl;
    }

    public static YouTubeEntry NowPlayingEntry { get; set; }
    public static YouTubeEntry NextPlayingEntry { get; set; }


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


    static public string youtubecatch2(string url)
    {
      string str = getContent(url);
      int i = 0;
      int i1 = 0;
      string str1 = "/watch_fullscreen?";
      i = str.IndexOf(str1, StringComparison.CurrentCultureIgnoreCase);
      i1 = str.IndexOf(";", i, StringComparison.CurrentCultureIgnoreCase);
      string str3 = str.Substring(i + str1.Length, i1 - (i + str1.Length));
      string str7 = str3.Substring(str3.IndexOf("&title=") + 7);
      str7 = str7.Substring(0, str7.Length - 1);
      return String.Concat("http://youtube.com/get_video?", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")), str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));//+ "&fmt=18"
      //return string.Concat("http://www.youtube.com/v/", str3.Substring(str3.IndexOf("video_id"), str3.IndexOf("&", str3.IndexOf("video_id")) - str3.IndexOf("video_id")),"?",str3.Substring(str3.IndexOf("&l"), str3.IndexOf("&", str3.IndexOf("&l") + 1) - str3.IndexOf("&l")), str3.Substring(str3.IndexOf("&t"), str3.IndexOf("&", str3.IndexOf("&t") + 1) - str3.IndexOf("&t")));
   
    }

    static public string youtubecatch1(string url,VideoInfo qa)
    {
       
      if (_settings.LocalFile.Get(getIDSimple(url)) != null)
      {
        return _settings.LocalFile.Get(getIDSimple(url)).LocalFile;
      }
      if (String.IsNullOrEmpty(qa.Token) || !qa.IsInited)
      {
        qa.Get(getIDSimple(url));
      }
      if (qa.Quality == VideoQuality.HD && !qa.FmtMap.Contains("22/"))
      {
        qa.Quality = VideoQuality.High;
      }
      switch (qa.Quality)
      {
        case VideoQuality.Normal:
          return qa.GetPlaybackUrl("34"); // string.Format("http://youtube.com/get_video?video_id={0}&t={1}&ext=.flv", getIDSimple(url), qa.Token);)
        case VideoQuality.High:
          return qa.GetPlaybackUrl("18");//string.Format("http://youtube.com/get_video?video_id={0}&t={1}&fmt=18&ext=.mp4", getIDSimple(url), qa.Token);
        case VideoQuality.HD:
          return qa.GetPlaybackUrl("22"); // string.Format("http://youtube.com/get_video?video_id={0}&t={1}&fmt=22&ext=.mp4", getIDSimple(url), qa.Token);
        case VideoQuality.FullHD:
          return qa.GetPlaybackUrl("37"); // string.Format("http://youtube.com/get_video?video_id={0}&t={1}&fmt=22&ext=.mp4", getIDSimple(url), qa.Token);

      }
      return qa.GetPlaybackUrl("34");  //string.Format("http://youtube.com/get_video?video_id={0}&t={1}&ext=.flv", getIDSimple(url), qa.Token);
    }

    static private Stream RetrieveData(string sUrl)
    {
      if (String.IsNullOrEmpty(sUrl) || sUrl[0] == '/')
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
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentLength = (long)str1.Length;
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
        streamWriter.Write(str1);
        streamWriter.Close();
        StreamReader streamReader = new StreamReader((httpWebRequest.GetResponse() as HttpWebResponse).GetResponseStream());
        str = streamReader.ReadToEnd();
        streamReader.Close();
      }
      catch (Exception exception1)
      {
        str = String.Concat("Error: ", exception1.Message.ToString());
        Log.Error(exception1);
      }
      return str;
    }

    public static GenericListItem GetPager(SiteItemEntry entry, YouTubeFeed videos)
    {
      if (videos.TotalResults > videos.StartIndex + ITEM_IN_LIST)
      {
        SiteItemEntry newEntry = entry.Copy();
        newEntry.StartItem += ITEM_IN_LIST;
        GenericListItem listItem = new GenericListItem()
                                     {
                                       Title = Translation.NextPage,
                                       IsFolder = true,
                                       DefaultImage = "NextPage.png",
                                       Tag = newEntry
                                     };
        return listItem;
      }
      return null;
    }


    public static GenericListItem YouTubeEntry2ListItem(YouTubeEntry youTubeEntry)
    {
      GenericListItem item = new GenericListItem()
                               {
                                 Title = youTubeEntry.Title.Text,
                                 IsFolder = false,
                                 LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
                                 Tag = youTubeEntry
                               };
      if (youTubeEntry.Duration != null)
        item.Duration = Convert.ToInt32(youTubeEntry.Duration.Seconds, 10);
      item.Title2 = Utils.SecondsToHMSString(item.Duration);
      return item;
    }

    public static string FormatNumber(string numeber)
    {
      if (String.IsNullOrEmpty(numeber))
        return " ";
      int i = Convert.ToInt32(numeber);
      return i.ToString("0,0");
      return " ";
    }

    public static void Err_message(string message)
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(25660);
        dlgOK.SetLine(1, message);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    public static VideoInfo SelectQuality(YouTubeEntry vid)
    {
      VideoInfo info = new VideoInfo();
      info.Get(GetVideoId(vid));
      if (!String.IsNullOrEmpty(info.Reason))
      {
        Err_message(info.Reason);
        info.Quality = VideoQuality.Unknow;
        return info;
      }

      switch (_settings.VideoQuality)
      {
        case 0:
          info.Quality = VideoQuality.Normal;
          break;
        case 1:
          info.Quality = VideoQuality.High;
          break;
        case 2:
          info.Quality = VideoQuality.HD;
          break;
        case 3:
          info.Quality = VideoQuality.FullHD;
          break;
        case 4:
          {
            string title = vid.Title.Text;
            if (info.FmtMap.Contains("18"))
              info.Quality = VideoQuality.High;
            if (info.FmtMap.Contains("22"))
              info.Quality = VideoQuality.HD;
            if (info.FmtMap.Contains("37"))
              info.Quality = VideoQuality.FullHD;
            break;
          }
        case 5:
          {

            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) info.Quality = VideoQuality.Normal;
            dlg.Reset();
            dlg.SetHeading("Select video quality");
            dlg.Add("Normal quality");
            dlg.Add("High quality");
            if (info.FmtMap.Contains("22"))
            {
              dlg.Add("HD quality");
            }
            if (info.FmtMap.Contains("37"))
            {
              dlg.Add("Full HD quality");
            }
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId == -1) info.Quality = VideoQuality.Unknow;
            switch (dlg.SelectedLabel)
            {
              case 0:
                info.Quality = VideoQuality.Normal;
                break;
              case 1:
                info.Quality = VideoQuality.High;
                break;
              case 2:
                info.Quality = VideoQuality.HD;
                break;
              case 3:
                info.Quality = VideoQuality.FullHD;
                break;
            }
          }
          break;
      }
      return info;
    }

    static public string GetLocalImageFileName(string strURL)
    {
      if (strURL == "")
        return String.Empty;
      if (strURL == "@")
        return String.Empty;
      string url = String.Format("youtubevideos-{0}.jpg", Utils.EncryptLine(strURL));
      return Path.Combine(_settings.CacheDir, url); ;
    }

    static public void DownloadFile(string url, string localFile)
    {
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(localFile))
        return;
      try
      {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, localFile);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

  }
}
