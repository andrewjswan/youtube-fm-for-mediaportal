using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace YouTubePlugin
{
  public class DownloadFileObject
  {
    private string  _fileName;

    public string  FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    private string _url;

    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    private GUIListItem listItem;

    public GUIListItem ListItem
    {
      get { return listItem; }
      set { listItem = value; }
    }
	
    public DownloadFileObject(string file, string url)
    {
      FileName = file;
      Url = url;
      ListItem = null;
    }

    public DownloadFileObject(string file, string url, GUIListItem item)
    {
      FileName = file;
      Url = url;
      ListItem = item;
    }
  }
}
