using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YouTubePlugin.Class.Database;

namespace YouTubePlugin.Class.SiteItems
{
  public class Statistics : ISiteItem
  {
    private List<string> stats=new List<string>();
    public Statistics()
    {
      Name = "Play Statistics";
      stats.Add("Recently played videos");
      stats.Add("Most played videos");
      //stats.Add("Most played artists");
      //stats.Add("Random");
    }

    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      if (string.IsNullOrEmpty(entry.GetValue("level")))
      {
        for (int i = 0; i < stats.Count; i++)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          Statistics statistics = new Statistics();
          newentry.Provider = statistics.Name;
          newentry.Title = stats[i];
          newentry.SetValue("level", i.ToString());
          res.Items.Add(new GenericListItem()
                          {
                            IsFolder = true,
                            Title = newentry.Title,
                            Tag = newentry,
                            //LogoUrl = ArtistManager.Instance.GetArtistsImgUrl(GetArtistName(title[1]))
                          });
        }
      }
      if (entry.GetValue("level") == "0")//Recently played videos
      {
        return DatabaseProvider.InstanInstance.GetRecentlyPlayed();
      }
      if (entry.GetValue("level") == "1")//Recently played videos
      {
        return DatabaseProvider.InstanInstance.GetTopPlayed();
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry itemEntry)
    {
      itemEntry.Title = Name;
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
