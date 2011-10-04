using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.Class
{
  public class GenericListItem
  {
    public string Title { get; set; }
    public string Title2 { get; set; }
    public string Title3 { get; set; }
    public string LogoUrl { get; set; }
    public string LogoFile { get; set; }
    public bool IsFolder { get; set; }
    public object Tag { get; set; }
    public int Duration { get; set; }
    public float Rating { get; set; }
  }
}
