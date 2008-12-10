using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;

namespace YouTubePlugin.DataProvider
{
  public class HTBFanArt
  {
    private List<FanArtItem> imageUrls;

    public List<FanArtItem> ImageUrls
    {
      get { return imageUrls; }
      set { imageUrls = value; }
    }

    public bool Search(string item)
    {
      ImageUrls.Clear();
      List<FanArtItem> items = new List<FanArtItem>();
      string sourceurl = string.Format("http://www.htbackdrops.com/thumbnails.php?album=search&search={0}", HttpUtility.UrlEncode(item));
      try
      {
      WebClient client = new WebClient();
      string resp = client.DownloadString(new Uri(sourceurl));
        Regex RegexObj = new Regex("Filename=(?<file>.*?).Filesize.*?<span class=\"thumb_title\">(?<name>.*?)</span>",
          RegexOptions.Singleline);
        Match MatchResults = RegexObj.Match(resp);
        while (MatchResults.Success)
        {
          items.Add(new FanArtItem(string.Format("http://www.htbackdrops.com/albums/userpics/{0}", MatchResults.Groups["file"].Value), MatchResults.Groups["name"].Value));
          MatchResults = MatchResults.NextMatch();
        }
        foreach (FanArtItem it in items)
        {
          if (it.Title.ToUpper() == item.ToUpper())
          {
            ImageUrls.Add(it);
          }
        }
      }
      catch (ArgumentException )
      {
        return false;
      }
      return true;
    }

    public HTBFanArt()
    {
      ImageUrls = new List<FanArtItem>();
    }
  }
}
