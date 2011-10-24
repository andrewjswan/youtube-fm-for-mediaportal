using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.YouTube;
using MediaPortal.GUI.Library;

namespace YouTubePlugin.Class.SiteItems
{
  public class StandardFeedItem : ISiteItem
  {
    public StandardFeedItem()
    {
      Name = "Standard feed";
      ConfigControl = new StandardFeedItemControl();
    }

    public Control ConfigControl { get; set; }

    public void Configure(SiteItemEntry entry)
    {
      ((StandardFeedItemControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }

    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      string query = YouTubeQuery.TopRatedVideo;
      bool usetime = true;
      res.Title = entry.Title;
      switch (Convert.ToInt32(entry.GetValue("feedint")))
      {
        case 0:
          query = YouTubeQuery.MostViewedVideo;
          break;
        case 1:
          query = YouTubeQuery.TopRatedVideo;
          break;
        case 2:
          query = YouTubeQuery.RecentlyFeaturedVideo;
          break;
        case 3:
          query = YouTubeQuery.MostDiscussedVideo;
          break;
        case 4:
          query = YouTubeQuery.FavoritesVideo;
          break;
        case 5:
          usetime = false;
          query = YouTubeQuery.MostLinkedVideo;
          break;
        case 6:
          usetime = false;
          query = YouTubeQuery.MostRespondedVideo;
          break;
        case 7:
          usetime = false;
          query = YouTubeQuery.MostRecentVideo;
          break;
        case 8:
          usetime = false;
          query = YouTubeQuery.StandardFeeds + "most_shared";
          break;
        case 9:
          usetime = false;
          query = YouTubeQuery.StandardFeeds + "on_the_web";
          break;

      }

      if (!string.IsNullOrEmpty(entry.GetValue("region")))
      {
        string reg = Youtube2MP._settings.Regions[entry.GetValue("region")];
        if (!string.IsNullOrEmpty(reg))
          query = query.Replace("standardfeeds", "standardfeeds/" + reg);
      }
         
      if (Youtube2MP._settings.MusicFilter)
        query += "_Music";

      YouTubeQuery tubeQuery = new YouTubeQuery(query);
      tubeQuery.NumberToRetrieve = 50;
      tubeQuery.SafeSearch = YouTubeQuery.SafeSearchValues.None;

      if (usetime)
      {
        if (entry.GetValue("time") == "Today")
          tubeQuery.Time = YouTubeQuery.UploadTime.Today;
        if (entry.GetValue("time") == "This Week")
          tubeQuery.Time = YouTubeQuery.UploadTime.ThisWeek;
        if (entry.GetValue("time") == "This Month")
          tubeQuery.Time = YouTubeQuery.UploadTime.ThisMonth;
      }

      YouTubeFeed videos = Youtube2MP.service.Query(tubeQuery);
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        res.Items.Add(Youtube2MP.YouTubeEntry2ListItem(youTubeEntry));
      }
      res.FolderType = 1;
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry itemEntry)
    {
      GenericListItemCollections res = new GenericListItemCollections();

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

  }
}
