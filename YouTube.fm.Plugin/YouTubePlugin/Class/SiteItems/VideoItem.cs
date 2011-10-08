using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  class VideoItem : ISiteItem
  {
    public VideoItem()
    {
      Name = "VideoItem";
      ConfigControl = new VideoItemControl();
    }

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((VideoItemControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      query.Query = string.IsNullOrEmpty(entry.GetValue("search")) ? entry.Title : entry.GetValue("search");
      
      query.NumberToRetrieve = 1;
      query.OrderBy = "relevance";

      if (Youtube2MP._settings.MusicFilter)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      YouTubeFeed videos = Youtube2MP.service.Query(query);
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        res.Items.Add(Youtube2MP.YouTubeEntry2ListItem(youTubeEntry));
      }
      res.FolderType = 1;
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
      {
        IsFolder = false,
        Title = entry.Title,
        Tag = entry
      });
      return res;
    }
  }
}
