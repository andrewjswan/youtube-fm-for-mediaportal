using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTubePlugin.Class
{
  public enum SiteType
  {
    Video,
    Artist
  }

  public class SiteContent
  {
    public string SIte { get; set; }
    public string Url { get; set; }
    public string VideoId { get; set; }
    public string ArtistId { get; set; }
    public SiteType SiteType { get; set; }
  }
}
