using System;
using System.Collections.Generic;
using System.Text;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;

namespace YouTubePlugin
{
  public class LocalFileStruct
  {
    public LocalFileStruct()
    {
      LocalFile = string.Empty;
      VideoId = string.Empty;
    }

    public LocalFileStruct(string local, string id, string title)
    {
      LocalFile = local;
      VideoId = id;
      Title = title;
    }

    private string localFile;
    public string LocalFile
    {
      get { return localFile; }
      set { localFile = value; }
    }

    private string title;

    public string Title
    {
      get { return title; }
      set { title = value; }
    }

    private string videoId;

    public string VideoId
    {
      get { return videoId; }
      set { videoId = value; }
    }

  }
}
