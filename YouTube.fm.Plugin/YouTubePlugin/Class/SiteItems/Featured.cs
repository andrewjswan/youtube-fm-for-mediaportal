using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace YouTubePlugin.Class.SiteItems
{
  public class Featured : ISiteItem
  {
    private const string MenuFileName = "youtubefmFeatureMenu.xml";
    private SiteItemEnumerator _menu = new SiteItemEnumerator();
    public Featured()
    {
      Name = "Featured";
      string filename = Config.GetFile(Config.Dir.Config, MenuFileName);
      if(File.Exists(filename))
      {
        _menu.Load(MenuFileName);
      }
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

      foreach (SiteItemEntry itemEntry in _menu.Items)
      {
        if (string.IsNullOrEmpty(itemEntry.ParentFolder))
          res.Add(Youtube2MP.SiteItemProvider[itemEntry.Provider].HomeGetList(itemEntry));
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      entry.Title = Name;
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
