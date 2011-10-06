using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public class Folder : ISiteItem
  {
    public Folder()
    {
      Name = "Folder";
      ConfigControl = new FolderControl();
    }
    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((FolderControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      foreach (SiteItemEntry itemEntry in entry.Parent.Items)
      {
        if (itemEntry.ParentFolder == entry.Title)
          res.Items.Add(new GenericListItem {Title = itemEntry.Title, Tag = itemEntry, IsFolder = true,});
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
      {
        IsFolder = true,
        Title = entry.Title,
        Tag =entry
      // new SiteItemEntry() { Provider = "Artists" }
      });
      return res;
    }
  }
}
