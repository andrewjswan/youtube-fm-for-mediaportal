using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.YouTube;
using YouTubePlugin.Class.Artist;

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
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      res.Add(VevoFavorite());
      res.Add(VevoUploads());
      List<ArtistItem> artists = ArtistManager.Instance.GetVevoArtists();
      foreach (ArtistItem artistItem in artists)
      {
        UserVideos userVideos = new UserVideos();
        SiteItemEntry itemEntry = new SiteItemEntry();
        itemEntry.Provider = userVideos.Name;
        itemEntry.SetValue("id", artistItem.User);
        string title = artistItem.Name;
        GenericListItem listItem = new GenericListItem()
        {
          Title = title,
          IsFolder = false,
          LogoUrl = artistItem.Img_url,
          DefaultImage = "defaultArtistBig.png",
          Tag = itemEntry
        };
        res.Add(listItem);
      }
      res.ItemType = ItemType.Artist;
      return res;
    }

    private GenericListItem VevoFavorite()
    {
      SiteItemEntry itemEntry = new SiteItemEntry();
      itemEntry.Provider = new FavoritesVideos().Name;
      itemEntry.SetValue("user", "vevo");
      string title = "Vevo favorites";
      GenericListItem listItem = new GenericListItem()
      {
        Title = title,
        IsFolder = false,
        DefaultImage = "defaultArtistBig.png",
        Tag = itemEntry
      };
      return listItem;
    }

    private GenericListItem VevoUploads()
    {
      SiteItemEntry itemEntry = new SiteItemEntry();
      itemEntry.Provider = new UserVideos().Name;
      itemEntry.SetValue("id", "vevo");
      string title = "Vevo uploads";
      GenericListItem listItem = new GenericListItem()
      {
        Title = title,
        IsFolder = false,
        DefaultImage = "defaultArtistBig.png",
        Tag = itemEntry
      };
      return listItem;
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
