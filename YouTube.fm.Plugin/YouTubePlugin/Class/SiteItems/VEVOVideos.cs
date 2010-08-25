using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  public class VevoVideos:ISiteItem
  {
    public VevoVideos()
    {
      Name = "Vevo Videos";
    }

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      Dictionary<string, GenericListItem> artists = new Dictionary<string, GenericListItem>();

      GenericListItemCollections res = new GenericListItemCollections();
      YouTubeQuery query = new YouTubeQuery("http://gdata.youtube.com/feeds/api/users/vevo/favorites");
      query.NumberToRetrieve = 50;
      query.OrderBy = "viewCount";
      do
      {
        YouTubeFeed videos = Youtube2MP.service.Query(query);
        foreach (YouTubeEntry youTubeEntry in videos.Entries)
        {
          UserVideos userVideos = new UserVideos();
          SiteItemEntry itemEntry = new SiteItemEntry();
          itemEntry.Provider = userVideos.Name;
          itemEntry.SetValue("id", youTubeEntry.Uploader.Value);
          string title = youTubeEntry.Uploader.Value;
          if (youTubeEntry.Title.Text.Contains("-"))
            title = youTubeEntry.Title.Text.Split('-')[0];
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = title,
                                         IsFolder = false,
                                         LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
                                         Tag = itemEntry
                                       };
          //  res.Items.Add(listItem);
          if (!artists.ContainsKey(listItem.Title))
            artists.Add(listItem.Title, listItem);

        }
        query.StartIndex += 50;
        if(videos.TotalResults<query.StartIndex+50)
          break;
      } while (true);
      foreach (KeyValuePair<string, GenericListItem> genericListItem in artists)
      {
        res.Items.Add(genericListItem.Value);
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry itemEntry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      itemEntry.Title = Name;
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
