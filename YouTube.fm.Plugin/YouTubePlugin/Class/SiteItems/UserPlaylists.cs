using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using MediaGroup = Google.GData.YouTube.MediaGroup;

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
      res.Title = entry.Title;
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(entry.GetValue("id")));
      PlaylistsFeed userPlaylists = Youtube2MP.service.GetPlaylists(query);
      res.Title = userPlaylists.Title.Text;
      foreach (PlaylistsEntry playlistsEntry in userPlaylists.Entries)
      {
        Google.GData.Extensions.XmlExtension e = playlistsEntry.FindExtension("group", "http://search.yahoo.com/mrss/") as Google.GData.Extensions.XmlExtension;

        string img = "http://i2.ytimg.com/vi/hqdefault.jpg";
        try
        {
          img = e.Node.FirstChild.Attributes["url"].Value;
        }
        catch 
        {
          
          
        }

        PlayList playList = new PlayList();
       
        SiteItemEntry itemEntry = new SiteItemEntry();
        itemEntry.Provider = playList.Name;
        itemEntry.SetValue("url", playlistsEntry.Content.AbsoluteUri);
        string title = playlistsEntry.Title.Text;
        GenericListItem listItem = new GenericListItem()
        {
          Title = title,
          IsFolder = false,
          LogoUrl = img.Replace("default", "hqdefault"),
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


    static public string GetBestUrl(ExtensionCollection<MediaThumbnail> th)
    {
      if (th != null && th.Count > 0)
      {
        int with = 0;
        string url = string.Empty;
        foreach (MediaThumbnail mediaThumbnail in th)
        {
          int w = 0;
          int.TryParse(mediaThumbnail.Width, out w);
          if (w > with)
          {
            url = mediaThumbnail.Url;
            with = w;
          }
        }
        return url;
      }
      return "http://i2.ytimg.com/vi/hqdefault.jpg";
    }
    #endregion
  }
}
