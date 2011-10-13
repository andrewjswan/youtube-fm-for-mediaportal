using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using Lastfm.Services;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin
{
  public class YoutubeGuiInfoEx : YouTubeGuiInfoBase
  {
    public string VideoId { get; set; }
    public YouTubeEntry YouTubeEntry { get; set; }
    public ArtistItem ArtistItem { get; set; }
    private BackgroundWorker Worker_Youtube = new BackgroundWorker();
    private BackgroundWorker Worker_Artist = new BackgroundWorker();
    private BackgroundWorker Worker_Fast = new BackgroundWorker();

    public override int GetID
    {
      get
      {
        return 29053;
      }
      set
      {
      }
    }

    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    public override bool Init()
    {
      Worker_Fast.DoWork += Worker_Fast_DoWork;
      Worker_Youtube.DoWork += Worker_Youtube_DoWork;
      Worker_Artist.DoWork += Worker_Artist_DoWork;
      return Load(GUIGraphicsContext.Skin + @"\youtubeinfoex.xml");
    }

    void Worker_Fast_DoWork(object sender, DoWorkEventArgs e)
    {
      ClearLists();
      SetLabels(YouTubeEntry,"Info");
      LoadRelatated(YouTubeEntry);
      LoadSimilarArtists(YouTubeEntry);
      Worker_Youtube.RunWorkerAsync();
      Worker_Artist.RunWorkerAsync();
      GUIWaitCursor.Hide();
    }

    void Worker_Youtube_DoWork(object sender, DoWorkEventArgs e)
    {
      Uri videoEntryUrl =
        new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(YouTubeEntry));
      Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
      Feed<Comment> comments = Youtube2MP.request.GetComments(video);
      string cm = "\n------------------------------------------\n";
      foreach (Comment c in comments.Entries)
      {
        cm += c.Author + " : " + c.Content + "\n------------------------------------------\n";
      }
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Comments", cm);
    }

    void Worker_Artist_DoWork(object sender, DoWorkEventArgs e)
    {
      try
      {
        string imgurl =
          ArtistManager.Instance.GetArtistsImgUrl(GUIPropertyManager.GetProperty("#Youtube.fm.Info.Artist.Name"));
        if (!string.IsNullOrEmpty(imgurl))
        {
          string artistimg = GetLocalImageFileName(imgurl);
          DownloadImage(artistimg, null);
          OnDownloadTimedEvent();
        }

        Track track = new Track(GUIPropertyManager.GetProperty("#Youtube.fm.Info.Artist.Name"), GUIPropertyManager.GetProperty("#Youtube.fm.Info.Video.Title"), Youtube2MP.LastFmProfile.Session);
        //string s = track.Wiki.getContent();
        GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Bio",  HttpUtility.HtmlDecode(track.Artist.Bio.getContent()));
        string tags = " ";
        foreach (TopTag tag in track.Artist.GetTopTags())
        {
          tags += tag.Item.Name + "|";
        }
        GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Tags", tags);
      }
      catch (Exception)
      {

      }
    }

    protected override void OnPageLoad()
    {
      ClearInfoLabels();
      GUIWaitCursor.Init();
      GUIWaitCursor.Show();
      Worker_Fast.RunWorkerAsync();
      base.OnPageLoad();
    }

    static public void ClearInfoLabels()
    {
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Title", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Duration", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Autor", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.PublishDate", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Image", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.ViewCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.WatchCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.FavoriteCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Comments", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Rating", "0");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.FanArt", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.Summary", " ");

      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.NumLike", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.NumDisLike", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Video.PercentLike", "0");

      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Name", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Bio", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Tags", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm.Info.Artist.Image", " ");
    }
  }
}
