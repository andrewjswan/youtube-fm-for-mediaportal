using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  public class UserChannel : ISiteItem
  {
    public UserChannel()
    {
      Name = "UserChannel";
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
      {
        SiteItemEntry itemEntry = new SiteItemEntry();
        itemEntry.Provider = new UserPlaylists().Name;
        itemEntry.SetValue("id", entry.GetValue("id"));
        string title = "Playlist";
        GenericListItem listItem = new GenericListItem()
        {
          Title = title,
          IsFolder = false,
          LogoUrl = entry.GetValue("imgurl"),
          //DefaultImage = "defaultArtistBig.png",
          Tag = itemEntry
        };
        res.Add(listItem);
      }
      {
        SiteItemEntry itemEntry = new SiteItemEntry();
        itemEntry.Provider = new UserVideos().Name;
        itemEntry.SetValue("id", entry.GetValue("id"));
        string title = "Uploads";
        GenericListItem listItem = new GenericListItem()
        {
          Title = title,
          IsFolder = false,
          LogoUrl = entry.GetValue("imgurl"),
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

      string feedUrl = "http://gdata.youtube.com/feeds/api/users/"+entry.GetValue("id");
      ProfileEntry profile = (ProfileEntry)Youtube2MP.service.Get(feedUrl);

      string img = "";
      try
      {
        foreach (IExtensionElementFactory factory in profile.ExtensionElements)
        {
          if (factory.XmlName == "thumbnail")
            img = ((Google.GData.Extensions.XmlExtension)factory).Node.Attributes[0].Value;
        }
      }
      catch 
      {
      }
      entry.SetValue("imgurl", img);
      GenericListItem listItem = new GenericListItem()
      {
        Title = entry.Title,
        IsFolder = true,
        LogoUrl = img,
        Tag = entry
      };
      res.Items.Add(listItem);
      return res;
    }

    #endregion
  }
}
