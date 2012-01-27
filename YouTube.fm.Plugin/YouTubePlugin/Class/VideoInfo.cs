using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Google.GData.YouTube;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin
{
  public class VideoInfo
  {

    public Dictionary<string,string > PlaybackUrls { get; set; }

    public VideoQuality Quality { get; set; }

    public YouTubeEntry Entry { get; set; }

    public DateTime Date { get; set; }

    public bool IsInited { get; set; }

    public string Token
    {
      get
      {
        if (PlaybackUrls.Count > 0)
        {
          if (DateTime.Now.Subtract(Date).Minutes > 10)
            return "";
          return "???";
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
          if (Items.ContainsKey("fmt_list"))
            return System.Web.HttpUtility.UrlDecode(Items["fmt_list"]);
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
      if (PlaybackUrls.ContainsKey(fmt))
        return PlaybackUrls[fmt];
      if (PlaybackUrls.Count > 0)
      {
        var enumerator = PlaybackUrls.GetEnumerator();
        if (enumerator.MoveNext())
          return enumerator.Current.Value;
      }
      return "";
    }

    public string ReplaceJSon(string s)
    {
      s = System.Web.HttpUtility.UrlDecode(s);
      s = s.Replace(@"\/", "/");
      s = s.Replace("\\u0024", "$");
      s = s.Replace("\\u0025", "%");
      s = s.Replace("\\u0026", "&");
      s = s.Replace("\\u0017", "?");
      return s;
    }

    public Dictionary<string, string> Items = new Dictionary<string, string>();
    public void Get(string videoId)
    {
      //Init();
      PlaybackUrls.Clear();
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      try
      {
        //
        string contents = client.DownloadString(string.Format("http://youtube.com/get_video_info?video_id={0}&has_verified=1", videoId));
        //string[] elemest = System.Web.HttpUtility.UrlDecode(contents).Split('&');
        string[] elemest = (contents).Split('&');

        foreach (string s in elemest)
        {
          Items.Add(s.Split('=')[0], ReplaceJSon(s.Split('=')[1]));
        }

        Date = DateTime.Now;
        IsInited = true;
        if (!Items.ContainsKey("token"))
        {
          string site = "";
          try
          {
            site = client.DownloadString(string.Format("http://www.youtube.com/watch?v={0}&has_verified=1", videoId));
          }
          catch (Exception ex)
          {
            Log.Error("Error download info for video {0}",videoId);
            Log.Debug(ex.StackTrace);
          }
          
          //-----

          //Regex swfJsonArgs =
          //  new Regex(
          //    @"(?:var\s)?(?:swfArgs|'SWF_ARGS')\s*(?:=|\:)\s(?<json>\{.+\})|(?:\<param\sname=\\""flashvars\\""\svalue=\\""(?<params>[^""]+)\\""\>)|(flashvars=""(?<params>[^""]+)"")",
          //    RegexOptions.Compiled | RegexOptions.CultureInvariant);
          Regex swfJsonArgs =
            new Regex(
              @"var swf = ""(?<vars>.*?)\}",
              RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

          //

          Match m = swfJsonArgs.Match(site);
          if (m.Success)
          {
            if (m.Groups["vars"].Success)
            {

              string result = System.Web.HttpUtility.HtmlDecode(m.Groups["vars"].Value);

              Regex rx = new Regex(@"\\[uU]([0-9A-F]{4})");
              result = rx.Replace(result, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());

              NameValueCollection qscoll = HttpUtility.ParseQueryString(HttpUtility.HtmlDecode(result));
              foreach (string s in qscoll.AllKeys)
              {
                Items.Add(s, qscoll[s]);
              }

            }
            else if (m.Groups["json"].Success)
            {
              //Items.Clear();
              //foreach (var z in Newtonsoft.Json.Linq.JObject.Parse(m.Groups["json"].Value))
              //{
              //  Items.Add(z.Key, z.Value.Value<string>(z.Key));
              //}
            }
          }



          ArtistItem artistItem = ArtistManager.Instance.Grabber.GetFromVideoSite(site);
          ArtistManager.Instance.SitesCache.Add(new SiteContent {SIte = site, ArtistId = artistItem.Id, VideoId = videoId});
          ArtistManager.Instance.AddArtist(artistItem);

          Regex regexObj = new Regex(", \"t\": \"(?<token>.*?)\"", RegexOptions.Singleline);
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

      //-------------
      if (Items.ContainsKey("url_encoded_fmt_stream_map") && Items.ContainsKey("fmt_list"))
      {
        string[] FmtUrlMap = Items["url_encoded_fmt_stream_map"].Split(',');
        string[] FmtList = Items["fmt_list"].Split(',');

        for (int i = 0; i < FmtUrlMap.Length; i++)
        {
          var urlOptions = HttpUtility.ParseQueryString(FmtUrlMap[i]);
          string type = urlOptions.Get("type");
          if (!string.IsNullOrEmpty(type))
          {
            type = Regex.Replace(type, @"; codecs=""[^""]*""", "");
            type = type.Substring(type.LastIndexOfAny(new char[] {'/', '-'}) + 1);
          }
          string finalUrl = urlOptions.Get("url");
          PlaybackUrls.Add(FmtList[i].Split('/')[0], finalUrl + "&ext=." + type.Replace("webm", "mkv"));
        }
      }
    }

    public void Init()
    {
      Items.Clear();
      PlaybackUrls = new Dictionary<string, string>();
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
