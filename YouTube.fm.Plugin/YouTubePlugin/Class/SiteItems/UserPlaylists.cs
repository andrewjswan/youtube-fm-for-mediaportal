using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  public class UserPlaylists : ISiteItem
  {
    public UserPlaylists()
    {
      Name = "UserPlaylists";
      ConfigControl = new UserPlaylitsControl();
    }

    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((UserPlaylitsControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.FolderType = 1;

      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(entry.GetValue("id")));
      PlaylistsFeed userPlaylists = Youtube2MP.service.GetPlaylists(query);
      foreach (PlaylistsEntry playlistsEntry in userPlaylists.Entries)
      {
        PlayList playList = new PlayList();
        SiteItemEntry itemEntry = new SiteItemEntry();
        itemEntry.Provider = playList.Name;
        itemEntry.SetValue("url", playlistsEntry.Content.AbsoluteUri);
        string title = playlistsEntry.Title.Text;
        GenericListItem listItem = new GenericListItem()
        {
          Title = title,
          IsFolder = false,
          //LogoUrl = artistItem.Img_url,
          //DefaultImage = "defaultArtistBig.png",
          Tag = itemEntry
        };
        res.Add(listItem);
      }
      res.ItemType = ItemType.Item;
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();

      GenericListItem listItem = new GenericListItem()
      {
        Title = entry.Title,
        IsFolder = true,
        //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
        Tag = entry
      };
      res.Items.Add(listItem);
      return res;
    }

    #endregion
  }
}
