using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
      if (File.Exists(filename) && (File.GetLastWriteTime(filename) - DateTime.Now).Days < 2)
      {
        _menu.Load(MenuFileName);
      }
      else
      {
        try
        {
          WebClient client = new WebClient();
          client.DownloadFile(
            "http://youtube-fm-for-mediaportal.googlecode.com/svn/trunk/YouTube.fm.Plugin/DATA/youtubefmFeatureMenu.xml",
            filename);
        }
        catch
        {
        }
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
      res.Title = entry.Title;
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
      //entry.Title = Name;
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
