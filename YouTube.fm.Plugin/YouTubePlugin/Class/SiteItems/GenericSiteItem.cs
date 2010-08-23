using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public class GenericSiteItem:ISiteItem
  {
    public GenericSiteItem()
    {
      Name = "Standard feed";
      ConfigControl = new GenericSiteItemControl();
    }
    
    public Control ConfigControl { get; set; }

    public void Configure(SiteItemEntry entry)
    {
      ((GenericSiteItemControl) ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
  }
}
