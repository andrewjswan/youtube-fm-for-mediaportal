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
      string sourceurl = string.Format("http://htbackdrops.com/search.php?search_keywords={0}", HttpUtility.UrlEncode(item));
      try
      {
      WebClient client = new WebClient();
      string resp = client.DownloadString(new Uri(sourceurl));
      Regex RegexObj = new Regex(@"/details.php\?image_id=(?<id>.*?)&amp;mode=search&amp;sessionid=.*?<b>(?<name>.*?)</b>",
          RegexOptions.Singleline);
        Match MatchResults = RegexObj.Match(resp);
        while (MatchResults.Success)
        {
          items.Add(new FanArtItem(string.Format("http://htbackdrops.com/download.php?image_id={0}", MatchResults.Groups["id"].Value), MatchResults.Groups["name"].Value));
          MatchResults = MatchResults.NextMatch();
        }
        foreach (FanArtItem it in items)
        {
          if (it.Title.ToUpper() == item.ToUpper())
          {
              bool c = false;
              foreach (FanArtItem item2 in imageUrls)
              {
                  if (it.Url == item2.Url)
                      c = true;
              }
              if (!c)
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
