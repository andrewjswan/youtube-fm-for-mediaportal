using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.Class
{
  public class SiteItemEntry
  {
    public SiteItemEntry()
    {
      ConfigString = "";
      Provider = "";
      Title = "";
    }

    public string Url { get; set; }
    public string Provider { get; set; }
    public string Title { get; set; }
    public string ConfigString { get; set; }
  }
}
