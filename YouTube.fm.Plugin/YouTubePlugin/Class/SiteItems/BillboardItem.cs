using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Raccoom.Xml;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin.Class.SiteItems
{
  class BillboardItem : ISiteItem
  {
    private Dictionary<string, string> Feeds = new Dictionary<string, string>(); 

    public BillboardItem()
    {
        Name = "Billboard feed";
        ConfigControl = new BillboardItemControl();
        Feeds.Add("Billboard Hot 100 Chart", "http://www.billboard.com/rss/charts/hot-100");
        Feeds.Add("Billboard Billboard 200 Chart", "http://www.billboard.com/rss/charts/billboard-200");
        Feeds.Add("Billboard Radio Songs Chart", "http://www.billboard.com/rss/charts/radio-songs");
        Feeds.Add("Billboard Hot Digital Songs Chart", "http://www.billboard.com/rss/charts/digital-songs");
        Feeds.Add("Billboard R&B/Hip-Hop Songs Chart", "http://www.billboard.com/rss/charts/r-b-hip-hop-songs");
        Feeds.Add("Billboard Country Songs Chart", "http://www.billboard.com/rss/charts/country-songs");
        Feeds.Add("Billboard Rock Songs Chart", "http://www.billboard.com/rss/charts/rock-songs");
        Feeds.Add("Billboard Latin Songs Chart", "http://www.billboard.com/rss/charts/latin-songs");
        Feeds.Add("Billboard Pop Songs Chart", "http://www.billboard.com/rss/charts/pop-songs");
        Feeds.Add("Billboard Dance/Club Play Songs Chart", "http://www.billboard.com/rss/charts/dance-club-play-songs");
        Feeds.Add("Billboard Jazz Songs Chart", "http://www.billboard.com/rss/charts/jazz-songs");
        Feeds.Add("Billboard Gospel Songs Chart", "http://www.billboard.com/rss/charts/gospel-songs");
        Feeds.Add("Billboard Christian Songs Chart", "http://www.billboard.com/rss/charts/christian-songs");
        Feeds.Add("Billboard Alternative Songs Chart", "http://www.billboard.com/rss/charts/alternative-songs");
        Feeds.Add("Billboard Rap Songs Chart", "http://www.billboard.com/rss/charts/rap-songs");
        Feeds.Add("Billboard Adult Pop Songs Chart", "http://www.billboard.com/rss/charts/adult-pop-songs");
        Feeds.Add("Billboard The Official U.K. Singles Chart Chart", "http://www.billboard.com/rss/charts/united-kingdom-songs");
        Feeds.Add("Billboard Germany Songs Chart", "http://www.billboard.com/rss/charts/germany-songs");
        Feeds.Add("Billboard Billboard Canadian Hot 100 Chart", "http://www.billboard.com/rss/charts/canadian-hot-100");
        Feeds.Add("Billboard Korea K-Pop Hot 100 Chart", "http://www.billboard.com/rss/charts/k-pop-hot-100");
        Feeds.Add("Billboard Japan Hot 100 Chart", "http://www.billboard.com/rss/charts/japan-hot-100");
    }

    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((BillboardItemControl)ConfigControl).SetEntry(entry,Feeds.Keys.ToArray());
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      if (entry.GetValue("all") == "true" && entry.GetValue("level") != "false")
      {
        res.Title = entry.Title;
        foreach (KeyValuePair<string, string> keyValuePair in Feeds)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          BillboardItem videoItem = new BillboardItem();
          newentry.Provider = videoItem.Name;
          newentry.Title = keyValuePair.Key;
          newentry.SetValue("feed", keyValuePair.Key);
          res.Items.Add(new GenericListItem()
          {
            IsFolder = false,
            Title = newentry.Title,
            Tag = newentry
          });
        }
      }
      else
      {
        string rssurl = Feeds[entry.GetValue("feed")];
        Uri uri = new Uri(rssurl);
        RssChannel myRssChannel = new RssChannel(uri);
        res.Title = myRssChannel.Title;
        foreach (RssItem item in myRssChannel.Items)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          VideoItem videoItem = new VideoItem();
          newentry.Provider = videoItem.Name;
          newentry.Title = item.Title;
          newentry.SetValue("level", "false");
          string[] title = item.Title.Split(',');
          newentry.SetValue("search", title[1].Trim() + " - " + title[0].Split(':')[1]);
          res.Items.Add(new GenericListItem()
                          {
                            IsFolder = false,
                            Title = newentry.Title,
                            Tag = newentry,
                            LogoUrl = ArtistManager.Instance.GetArtistsImgUrl(GetArtistName(title[1])),
                            DefaultImage = "defaultArtistBig.png"
                          });
        }
      }
      return res;
    }

    string GetArtistName(string name)
    {
      if (name.Contains("Featuring "))
        return name.Substring(0,name.IndexOf("Featuring ")).Trim();
      if (name.Contains("Feat."))
        return name.Substring(0, name.IndexOf("Feat.")).Trim();
      return name.Trim();
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry itemEntry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = itemEntry.Title;

      GenericListItem listItem = new GenericListItem()
                                   {
                                     Title = itemEntry.Title,
                                     IsFolder = true,
                                     //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
                                     Tag = itemEntry
                                   };
      res.Items.Add(listItem);
      return res;
    }

    #endregion
  }
}
