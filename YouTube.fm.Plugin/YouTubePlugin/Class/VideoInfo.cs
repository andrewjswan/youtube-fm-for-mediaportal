using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Google.GData.YouTube;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin
{
  public class VideoInfo
  {
    private VideoQuality quality;

    public VideoQuality Quality
    {
      get { return quality; }
      set { quality = value; }
    }

    private YouTubeEntry entry;
    public YouTubeEntry Entry
    {
        get { return entry; }
        set { entry = value; }
    }

    private DateTime date;
    public DateTime Date
    {
        get { return date; }
        set { date = value; }
    }

    private bool isInited;
    public bool IsInited
    {
      get { return isInited; }
      set { isInited = value; }
    }

    public string Token
    {
      get
      {
          if (Items.ContainsKey("token"))
          {
              if (DateTime.Now.Subtract(Date).Minutes > 10)
                  return "";
              return Items["token"];
          }
          else
              return string.Empty;
      }
    }

    
    public string FmtMap
    {
      get
      {
        if (Items.ContainsKey("fmt_map"))
          return System.Web.HttpUtility.UrlDecode(Items["fmt_map"]);
        else
          return string.Empty;
      }
    }

    public string Reason
    {
      get
      {
        if (Items.ContainsKey("reason"))
          return System.Web.HttpUtility.UrlDecode(Items["reason"]);
        else
          return string.Empty;
      }
    }

    public VideoInfo()
    {
      Init();
    }
    
    public VideoInfo(VideoInfo info)
    {
        Init();
        this.Entry = info.Entry;
        this.Quality = info.Quality;
        this.Date = info.Date;
    }

    public string GetPlaybackUrl(string fmt)
    {
      if (!Items.ContainsKey("fmt_url_map"))
        return "";
      string[] ss = Items["fmt_url_map"].Split(',');
      if(!ss[0].Contains("|"))
        ss = System.Web.HttpUtility.UrlDecode(Items["fmt_url_map"]).Split(',');
      foreach (string sitem in ss)
      {
        string s = System.Web.HttpUtility.UrlDecode(sitem);
        string[] urls = s.Split('|');
        if (urls[0] == fmt)
          return urls[1].Replace(@"\/","/");
      }
      if (ss.Length > 0)
        return ss[0].Split('|')[1].Replace(@"\/", "/");
      return "";
    }

    public Dictionary<string, string> Items = new Dictionary<string, string>();
    public void Get(string videoId)
    {
      //Init();
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      try
      {
        string contents = client.DownloadString(string.Format("http://youtube.com/get_video_info?video_id={0}", videoId));
        //string[] elemest = System.Web.HttpUtility.UrlDecode(contents).Split('&');
        string[] elemest = (contents).Split('&');

        foreach (string s in elemest)
        {
          Items.Add(s.Split('=')[0], s.Split('=')[1]);
        }
        Date = DateTime.Now;
        IsInited = true;
        if (!Items.ContainsKey("token"))
        {
          string site = "";
          try
          {
            site = client.DownloadString(string.Format("http://www.youtube.com/watch?v={0}", videoId));
          }
          catch (Exception ex)
          {
            Log.Error("Error download info for video {0}",videoId);
            Log.Debug(ex.StackTrace);
          }
          

          ArtistItem artistItem = ArtistManager.Instance.Grabber.GetFromVideoSite(site);
          ArtistManager.Instance.SitesCache.Add(new SiteContent()
                                                  {SIte = site, ArtistId = artistItem.Id, VideoId = videoId});
          ArtistManager.Instance.AddArtist(artistItem);

          Regex regexObj = new Regex(", \"t\": \"(?<token>.*?)\", \"", RegexOptions.Singleline);
          Match matchResult = regexObj.Match(site);
          if (matchResult.Success)
          {
            Items.Add("token", matchResult.Groups["token"].Value);
            if (Items.ContainsKey("reason"))
              Items.Remove("reason");
          }

          Regex regexObj1 = new Regex(", \"fmt_map\": \"(?<fmt_map>.*?)\", \"", RegexOptions.Singleline);
          Match matchResult1 = regexObj1.Match(site);
          if (matchResult1.Success)
          {
            Items.Add("fmt_map", matchResult1.Groups["fmt_map"].Value);
          }
          Regex regexObj2 = new Regex(", \"fmt_url_map\": \"(?<fmt_url_map>.*?)\", \"", RegexOptions.Singleline);
          Match matchResult2 = regexObj2.Match(site);
          if (matchResult2.Success)
          {
            Items.Add("fmt_url_map", matchResult2.Groups["fmt_url_map"].Value);
          }

          //fmt_url_map
        }
        else
        {
          if (ArtistManager.Instance.SitesCache.GetByVideoId(videoId) == null)
          {
            string site = client.DownloadString(string.Format("http://www.youtube.com/watch?v={0}", videoId));
            ArtistItem artistItem = ArtistManager.Instance.Grabber.GetFromVideoSite(site);
            ArtistManager.Instance.SitesCache.Add(new SiteContent()
                                                    {SIte = site, ArtistId = artistItem.Id, VideoId = videoId});
            ArtistManager.Instance.AddArtist(artistItem);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        Init();
      }
    }

    public void Init()
    {
      Items.Clear();
      Quality = VideoQuality.Normal;
      switch (Youtube2MP._settings.VideoQuality)
      {
        case 0:
          Quality = VideoQuality.Normal;
          break;
        case 1:
          Quality = VideoQuality.High;
          break;
        case 2:
          Quality = VideoQuality.HD;
          break;
        case 3:
          Quality = VideoQuality.FullHD;
          break;
        case 4:
          {
            if (FmtMap.Contains("18"))
              Quality = VideoQuality.High;
            if (FmtMap.Contains("22/"))
              Quality = VideoQuality.HD;
            if (FmtMap.Contains("37/"))
              Quality = VideoQuality.FullHD;
            break;
          }
        case 5:
          {
            Quality = VideoQuality.High;
          }
          break;
      }
      IsInited = false;
    }

  }
}
