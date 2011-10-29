using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Xml;

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
      string replacedSpaces = item.Replace(" ", "_");
      Uri siteUri = new Uri("http://htbackdrops.com/api/12ed8117fe5b587c74fce9cdee069678/searchXML?keywords=" + replacedSpaces);
      string result = "";
      //create web request
      HttpWebRequest request = WebRequest.Create(siteUri) as HttpWebRequest;
      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        StreamReader reader = new StreamReader(response.GetResponseStream());
        result = reader.ReadToEnd();
      }

      //load xml from web request result and get the image id's
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(result);
      XmlNodeList nodelist = doc.SelectNodes("/search/images/image/id");

      if (nodelist.Count == 0)
      {
        return false;
      }
      //create array of id numbers
      ArrayList numArray = new ArrayList(nodelist.Count);
      //add id's to array
      foreach (XmlNode xmlnode in nodelist)
      {
        ImageUrls.Add(
          new FanArtItem(
            "http://htbackdrops.com/api/12ed8117fe5b587c74fce9cdee069678/download/" + xmlnode.InnerText + "/fullsize",
            item));
      }
/*
      List<FanArtItem> items = new List<FanArtItem>();
    //  string sourceurl = string.Format("http://htbackdrops.com/search.php?search_keywords={0}", HttpUtility.UrlEncode(item));
      string sourceurl = string.Format("http://www.htbackdrops.com/v2/thumbnails.php?search={0}&submit=search&album=search&title=checked&caption=checked&keywords=checked&type=AND", HttpUtility.UrlEncode(item));
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
  */
      return true;
    }

    public HTBFanArt()
    {
      ImageUrls = new List<FanArtItem>();
    }
  }
}
