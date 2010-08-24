using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.YouTube;

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

    public List<GenericListItem> GetList(SiteItemEntry entry)
    {
      List<GenericListItem> res = new List<GenericListItem>();
      string query = YouTubeQuery.TopRatedVideo;
      bool usetime = true;
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
          query = YouTubeQuery.CreateFavoritesUri(null);
          break;
      }

      YouTubeQuery tubeQuery = new YouTubeQuery(query);
      tubeQuery.NumberToRetrieve = 50;
      tubeQuery.SafeSearch = YouTubeQuery.SafeSearchValues.None;

      YouTubeFeed videos = Youtube2MP.service.Query(tubeQuery);

      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        GenericListItem listItem = new GenericListItem()
                                     {
                                       Title = youTubeEntry.Title.Text,
                                       IsFolder = false,
                                       LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
                                       Tag = youTubeEntry
                                     };
        res.Add(listItem);
      }
      return res;
    }
  }
}
