using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  public class SearchVideo:ISiteItem
  {
    public SearchVideo()
    {
      Name = "Search for videos";
      ConfigControl = new SearchVideoControl();
    }

    public Control ConfigControl { get; set; }

    public void Configure(SiteItemEntry entry)
    {
      ((SearchVideoControl)ConfigControl).SetEntry(entry);
    }
    
    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      query.Query = entry.GetValue("term");
      query.NumberToRetrieve = 50;
      query.OrderBy = "relevance";
      
      if (Youtube2MP._settings.MusicFilter)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      if (!string.IsNullOrEmpty(entry.GetValue("sortint")))
      {
        switch (Convert.ToInt32(entry.GetValue("sortint")))
        {
          case 0:
            query.OrderBy = "relevance";
            break;
          case 1:
            query.OrderBy = "published";
            break;
          case 2:
            query.OrderBy = "viewCount";
            break;
          case 3:
            query.OrderBy = "rating";
            break;
        }
      }

      if (entry.GetValue("time") == "Today")
        query.Time = YouTubeQuery.UploadTime.Today;
      if (entry.GetValue("time") == "This Week")
        query.Time = YouTubeQuery.UploadTime.ThisWeek;
      if (entry.GetValue("time") == "This Month")
        query.Time = YouTubeQuery.UploadTime.ThisMonth;

      YouTubeFeed videos = Youtube2MP.service.Query(query);
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
