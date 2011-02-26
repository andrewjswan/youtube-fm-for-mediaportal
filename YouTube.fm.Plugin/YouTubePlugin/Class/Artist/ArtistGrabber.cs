using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace YouTubePlugin.Class.Artist
{
  public class ArtistGrabber
  {
    public ArtistItem GetFromVideoSite(string site)
    {
      ArtistItem res=new ArtistItem();

      try
      {
        Regex regexObj = new Regex(@"/artist\?a=(?<id>.*?)&.*?<strong>(?<name>.*?)</strong>", RegexOptions.Singleline);
        Match matchResult = regexObj.Match(site);
        while (matchResult.Success)
        {
          res.Id = matchResult.Groups["id"].Value;
          res.Name = matchResult.Groups["name"].Value;
          matchResult = matchResult.NextMatch();
        }
      }
      catch (ArgumentException ex)
      {
        // Syntax error in the regular expression
      }
      return res;
    }

    public ArtistItem GetFromVideoUrl(string url)
    {
      string site = "";
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      site = client.DownloadString(url);
      return GetFromVideoSite(site);
    }
  }
}
