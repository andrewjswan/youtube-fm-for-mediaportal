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
    }
    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      throw new NotImplementedException();
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      throw new NotImplementedException();
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
      {
        IsFolder = true,
        Title = entry.Title,
        //Tag =entry.// new SiteItemEntry() { Provider = "Artists" }
      });
      return res;
    }
  }
}
