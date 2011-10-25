using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using MediaPortal.Database;
using MediaGroup = Google.GData.YouTube.MediaGroup;

namespace YouTubePlugin.Class.SiteItems
{
  public class UserDownloadedVideos : ISiteItem
  {
    public UserDownloadedVideos()
    {
      Name = "UserDownloadedVideos";
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
      res.Title = "User Downloaded Videos";
      foreach (LocalFileStruct localFileStruct in Youtube2MP._settings.LocalFile.Items)
      {
        GenericListItem listItem = new GenericListItem()
                                     {
                                       Title = localFileStruct.Title,
                                       IsFolder = false,
                                       LogoUrl = "",
                                       Tag = localFileStruct,
                                       Title2 = "",
                                       DefaultImage =
                                         Path.GetDirectoryName(localFileStruct.LocalFile) + "\\" +
                                         Path.GetFileNameWithoutExtension(localFileStruct.LocalFile) + ".png"
                                       //ParentTag = artistItem
                                     };
        res.Items.Add(listItem);
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
                      {
                        IsFolder = true,
                        Title = "User Downloaded Videos",
                        Tag = new SiteItemEntry() {Provider = "UserDownloadedVideos"}
                      });
      return res;
    }

    #endregion
  }
}
