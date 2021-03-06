using System.Collections.Generic;
using System.Windows.Forms;

namespace YouTubePlugin.Class
{
  public interface ISiteItem
  {
    Control ConfigControl { get; set; }
    void Configure(SiteItemEntry entry);
    string Name { get; set; }
    GenericListItemCollections GetList(SiteItemEntry entry);
    GenericListItemCollections HomeGetList(SiteItemEntry entry);
  }
}