using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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
          return Items["token"];
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

    public Dictionary<string, string> Items = new Dictionary<string, string>();
    public void Get(string videoId)
    {
      Init();
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      try
      {
        string contents = client.DownloadString(string.Format("http://youtube.com/get_video_info?video_id={0}",videoId));
        //string[] elemest = System.Web.HttpUtility.UrlDecode(contents).Split('&');
        string[] elemest = (contents).Split('&');

        foreach (string s in elemest)
        {
          Items.Add(s.Split('=')[0], s.Split('=')[1]);
        }
        IsInited = true;
      }
      catch
      {
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
          {
            if (FmtMap.Contains("18"))
              Quality = VideoQuality.High;
            if (FmtMap.Contains("22/"))
              Quality = VideoQuality.HD;
            break;
          }
        case 4:
          {
            Quality = VideoQuality.High;
          }
          break;
      }
      IsInited = false;
    }

  }
}
