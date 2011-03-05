using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTubePlugin.Class
{
  public class SitesCache
  {
    public List<SiteContent> Sites { get; set; }

    public SitesCache()
    {
      Sites = new List<SiteContent>();
    }

    public void Add(SiteContent content)
    {
      foreach (SiteContent siteContent in Sites)
      {
       if(siteContent.ArtistId==content.ArtistId || siteContent.VideoId==content.VideoId)
         return;
      }
      Sites.Add(content);
    }

    public SiteContent GetByVideoId(string id)
    {
      foreach (SiteContent siteContent in Sites)
      {
        if (siteContent.VideoId == id)
          return siteContent;
      }
      return null;
    }

    public SiteContent GetByArtistId(string id)
    {
      foreach (SiteContent siteContent in Sites)
      {
        if (siteContent.ArtistId == id)
          return siteContent;
      }
      return null;
    }

    public SiteContent GetByUrl(string url)
    {
      foreach (SiteContent siteContent in Sites)
      {
        if (siteContent.Url == url)
          return siteContent;
      }
      return null;
    }

  }
}
