using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.DataProvider
{
  public class FanArtItem
  {
    private string url;

    public string Url
    {
      get { return url; }
      set { url = value; }
    }

    private string title;

    public string Title
    {
      get { return title; }
      set { title = value; }
    }

    public FanArtItem()
    {
      Url = string.Empty;
      Title = string.Empty;
    }

    public FanArtItem(string _url, string _title)
    {
      Url = _url;
      Title = _title;
    }
  }
}
