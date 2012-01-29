using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Lastfm.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.Class.Database;


namespace YouTubePlugin
{
  public class PlayParams
  {
    public YouTubeEntry vid;
    public bool fullscr;
    public GUIListControl facade;
  }

  public class YoutubeGUIBase : GUIWindow
  {
    public Settings _setting = new Settings();
    protected YoutubePlaylistPlayer playlistPlayer;
    public Timer updateStationLogoTimer = new Timer(0.3 * 1000);
    public WebClient Client = new WebClient();
    public Queue downloaQueue = new Queue();
    private DownloadFileObject curentDownlodingFile;
    protected YouTubeQuery.UploadTime uploadtime = YouTubeQuery.UploadTime.AllTime;
    public FileDownloader VideoDownloader = new FileDownloader();

    private static YouTubeEntry label_last_entry;
    private static string label_last_type;

    public void SetLabels(YouTubeEntry vid, string type)
    {
      if (vid == label_last_entry && type == label_last_type)
        return;

      ClearLabels(type, false);
      label_last_entry = vid;
      label_last_type = type;
      try
      {
        if (vid.Duration != null && vid.Duration.Seconds != null)
        {
          int sec = int.Parse(vid.Duration.Seconds);
          int min = sec/60;
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Duration",
                                         string.Format("{0}:{1:0#}", min, (sec - (min*60))));
        }
        LocalFileStruct fileStruct = Youtube2MP._settings.LocalFile.Get(Youtube2MP.GetVideoId(vid));

        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.IsDownloaded",
                                       fileStruct != null ? "true" : "false");

        int watchcount = DatabaseProvider.InstanInstance.GetWatchCount(vid);
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.WatchCount",
                                       watchcount.ToString("0,0"));
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.IsWatched", watchcount > 0 ? "true" : "false");
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.PublishDate", vid.Published.ToShortDateString());
        if (vid.Authors != null && vid.Authors.Count > 0)
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Autor", vid.Authors[0].Name);
        if (vid.Rating != null)
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Rating", (vid.Rating.Average*2).ToString());
        if (vid.Statistics != null)
        {
          if (vid.Statistics.ViewCount != null)
            GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.ViewCount",
                                           Youtube2MP.FormatNumber(vid.Statistics.ViewCount));
          if (vid.Statistics.FavoriteCount != null)
            GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FavoriteCount",
                                           Youtube2MP.FormatNumber(vid.Statistics.FavoriteCount));
        }
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Image",
                                       GetLocalImageFileName(GetBestUrl(vid.Media.Thumbnails)));

        if (vid.Media.Description != null)
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Summary", vid.Media.Description.Value);
        if (vid.YtRating != null && !string.IsNullOrEmpty(vid.YtRating.NumLikes) &&
            !string.IsNullOrEmpty(vid.YtRating.NumDislikes))
        {
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.NumLike",
                                         Youtube2MP.FormatNumber(vid.YtRating.NumLikes));
          GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.NumDisLike",
                                         Youtube2MP.FormatNumber(vid.YtRating.NumDislikes));
          double numlike = Convert.ToDouble(vid.YtRating.NumLikes);
          double numdislike = Convert.ToDouble(vid.YtRating.NumDislikes);
          if (numlike + numdislike > 0)
          {
            double proc = numlike/(numdislike + numlike)*100;
            GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.PercentLike", proc.ToString());
          }
        }
        bool ishd =
          vid.ExtensionElements.Any(
            extensionElementFactory =>
            extensionElementFactory.XmlPrefix == "yt" && extensionElementFactory.XmlName == "hd");
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.IsHD", ishd ? "true" : "false");
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      if (vid.Title.Text.Contains("-"))
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", vid.Title.Text.Split('-')[1].Trim());
        if (type == "NowPlaying")
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", vid.Title.Text.Split('-')[1].Trim().Trim());
        }
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", vid.Title.Text.Split('-')[0].Trim());
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FanArt",
                                       GetFanArtImage(vid.Title.Text.Split('-')[0]).Trim());
      }
      else
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", vid.Title.Text);
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", " ");
      }

      if (type != "Curent")
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", GetArtistName(vid));
        string imgurl =
          ArtistManager.Instance.GetArtistsImgUrl(GUIPropertyManager.GetProperty("#Youtube.fm." + type + ".Artist.Name"));
        string artistimg = GetLocalImageFileName(imgurl);
        if (!string.IsNullOrEmpty(imgurl))
        {
          DownloadFile(imgurl, artistimg);
          if (File.Exists(artistimg))
          {
            GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Image", artistimg);
          }
        }
      }
    }

    public bool IsVideoUsable(YouTubeEntry tubeEntry)
    {
      // this is cas can hapen only if the entry is generated, so no information about video avaiablity
      if (tubeEntry.State == null)
        return true;
      if (tubeEntry.State.Name == "deleted")
        return false;
      if (tubeEntry.State.Name == "rejected" && tubeEntry.State.Reason == "suspended")
        return false;

      return true;
    }

    internal static void SetProperty(string property, string value)
    {
      if (property == null)
        return;

      //// If the value is empty always add a space
      //// otherwise the property will keep 
      //// displaying it's previous value
      if (String.IsNullOrEmpty(value))
        value = " ";

      GUIPropertyManager.SetProperty(property, value);
    }

    static public void ClearLabels(string type)
    {
      ClearLabels(type, true);
    }

    static public void ClearLabels(string type, bool all)
    {
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Title", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Duration", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Autor", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.PublishDate", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Image", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.ViewCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.WatchCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FavoriteCount", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Comments", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Rating", "0");

      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.NumLike", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.NumDisLike", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.PercentLike", "0");
      
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Artist.Name", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.FanArt", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.Summary", " ");
      if(all)
      {
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.IsHD", "false");
        GUIPropertyManager.SetProperty("#Youtube.fm." + type + ".Video.IsWatched", "false");
      }
    }

    public string FormatTitle(YouTubeEntry vid)
    {

      return string.Format("{0}", vid.Title.Text);
    }
  
    static public string GetBestUrl(ExtensionCollection<MediaThumbnail> th)
    {
      if (th !=null && th.Count > 0)
      {
        int with = 0;
        string url = string.Empty;
        foreach (MediaThumbnail mediaThumbnail in th)
        {
          int w = 0;
          int.TryParse(mediaThumbnail.Width, out w);
          if (w > with)
          {
            url = mediaThumbnail.Url;
            with = w;
          }
        }
        return url;
      }
      return "http://i2.ytimg.com/vi/hqdefault.jpg";
    }

    static public string GetLocalImageFileName(string strURL)
    {
      if (strURL == "")
        return string.Empty;
      if (strURL == "@")
        return string.Empty;
      string url = String.Format("youtubevideos-{0}.jpg", Utils.EncryptLine(strURL));
      return Path.Combine(Youtube2MP._settings.CacheDir, url); ;
    }

    public bool filterVideoContens(YouTubeEntry vid)
    {
      if (_setting.MusicFilter && _setting.UseExtremFilter)
      {
        if (vid.Title.Text.Contains("-"))
          return true;
        else
          return false;
      }
      return true;
    }

    public void Err_message(int langid)
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(25660);
        dlgOK.SetLine(1, langid);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GetID);
      }
    }

    public void Err_message(string message)
    {
      Youtube2MP.Err_message(message);
    }


    public void PlayNext(YouTubeEntry entry)
    {
      PlayList playlist = null;
      int poz = 0;
      if (Youtube2MP.temp_player.CurrentSong > -1)
      {
        poz = Youtube2MP.temp_player.CurrentSong;
        playlist = Youtube2MP.temp_player.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
      }
      if (Youtube2MP.player.CurrentSong > -1)
      {
        poz = Youtube2MP.player.CurrentSong;
        playlist = Youtube2MP.player.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
      }
      if (playlist == null)
        return;
      GUIWaitCursor.Hide();
      VideoInfo qa = SelectQuality(entry);
      if (qa.Quality == VideoQuality.Unknow)
        return;
      GUIWaitCursor.Show();
      AddItemToPlayList(entry, ref playlist, qa, poz );
    }

    public void BackGroundDoPlay(object param_)
    {
      PlayParams param = (PlayParams) param_;
      YouTubeEntry vid=param.vid;
      bool fullscr=param.fullscr;
      GUIListControl facade = param.facade;
      if (vid != null)
      {
        GUIWaitCursor.Hide();
        VideoInfo qa = SelectQuality(vid);
        if (qa.Quality == VideoQuality.Unknow)
        {
          Youtube2MP.PlayBegin = false;
          return;
        }
        GUIWaitCursor.Show();
        Youtube2MP.temp_player.Reset();
        Youtube2MP.temp_player.RepeatPlaylist = true;
        Youtube2MP.temp_player.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
        PlayList playlist = Youtube2MP.temp_player.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
        playlist.Clear();
        g_Player.PlayBackStopped += g_Player_PlayBackStopped;
        g_Player.PlayBackEnded += g_Player_PlayBackEnded;

        AddItemToPlayList(vid, ref playlist, qa, -1);

        if (facade != null)
        {
          qa.Items = new Dictionary<string, string>();
          int selected = facade.SelectedListItemIndex;
          for (int i = selected + 1; i < facade.ListItems.Count; i++)
          {
            try
            {
              AddItemToPlayList(facade.ListItems[i], ref playlist, new VideoInfo(qa), false);
            }
            catch (Exception ex)
            {
              Log.Error(ex);
            }
          }
        }
        else
        {
          Youtube2MP.temp_player.RepeatPlaylist = false;
        }

        PlayListPlayer.SingletonPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_NONE;
        Youtube2MP.player.CurrentPlaylistType = PlayListType.PLAYLIST_NONE;
        g_Player.Stop();
        Youtube2MP.temp_player.Play(0);
        GUIWaitCursor.Hide();

        if (g_Player.Playing && fullscr)
        {
          if (_setting.ShowNowPlaying)
          {
            if (GUIWindowManager.ActiveWindow != 29052)
              GUIWindowManager.ActivateWindow(29052);
          }
          else
          {
            g_Player.ShowFullScreenWindow();
          }
        }

        if (!g_Player.Playing)
        {
          Err_message("Unable to playback the item ! ");
        }
      }      
      GUIWaitCursor.Hide();
      Youtube2MP.PlayBegin = false;
    }

    public void DoPlay(YouTubeEntry vid, bool fullscr, GUIListControl facade)
    {
      if (Youtube2MP.PlayBegin)
        return;

      Youtube2MP.PlayBegin = true;
      PlayParams playParams = new PlayParams() {facade = facade, fullscr = fullscr, vid = vid};

      BackgroundWorker playbackgroundWorker = new BackgroundWorker();
      playbackgroundWorker.DoWork += new DoWorkEventHandler(playbackgroundWorker_DoWork);
      GUIWaitCursor.Init();
      GUIWaitCursor.Show();
      playbackgroundWorker.RunWorkerAsync(playParams);
      //Thread _thread = new Thread(new ParameterizedThreadStart(BackGroundDoPlay));
      //_thread.Start(playParams);
    }

    void playbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      BackGroundDoPlay(e.Argument);      
    }

    public VideoInfo SelectQuality(YouTubeEntry vid)
    {
      return Youtube2MP.SelectQuality(vid);
    }

    void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
      try
      {
        Youtube2MP.YouTubePlaying = false;
        g_Player.Release();
        g_Player.PlayBackStopped -= g_Player_PlayBackStopped;
        g_Player.PlayBackEnded -= g_Player_PlayBackEnded;
        ClearLabels("NowPlaying");
        Youtube2MP.player.DoOnStop();
        //if (GUIWindowManager.ActiveWindow == 29052)
        //  GUIWindowManager.ShowPreviousWindow();
      }
      catch
      {
      }
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      try
      {
        Youtube2MP.YouTubePlaying = false;
        g_Player.Release();
        g_Player.PlayBackStopped -= g_Player_PlayBackStopped;
        g_Player.PlayBackEnded -= g_Player_PlayBackEnded;
        Youtube2MP.player.DoOnStop();
        ClearLabels("NowPlaying");
        //if (GUIWindowManager.ActiveWindow == 29052)
        //  GUIWindowManager.ShowPreviousWindow();
      }
      catch
      {
      }
    }

    public void AddItemToPlayList(GUIListItem pItem, VideoInfo qa)
    {
      PlayList playList = Youtube2MP.player.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
      AddItemToPlayList(pItem, ref playList, qa, true);
    }

    public void AddItemToPlayList(GUIListItem pItem, ref PlayList playList, VideoInfo qa, bool check)
    {
      if (playList == null || pItem == null)
        return;
      if (pItem.MusicTag == null)
        return;

      string PlayblackUrl = "";

      YouTubeEntry vid;

      LocalFileStruct file = pItem.MusicTag as LocalFileStruct;
      if (file != null)
      {
        Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + file.VideoId);
        Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
        vid = video.YouTubeEntry;
      }
      else
      {
        vid = pItem.MusicTag as YouTubeEntry;
        if (vid == null && check)
        {
          SiteItemEntry entry = pItem.MusicTag as SiteItemEntry;
          if (entry != null)
          {
            GenericListItemCollections genericListItem = Youtube2MP.GetList(entry);
            if (entry.Provider == "VideoItem" && genericListItem.Items.Count > 0)
            {
              vid = genericListItem.Items[0].Tag as YouTubeEntry;
            }
          }
        }

        if (vid != null && vid.Authors.Count == 0 && check)
        {
          Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(vid));
          try
          {
            Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
            vid = video.YouTubeEntry;
          }
          catch (Exception)
          {
            vid = null;
          }
        }
      }

      if (vid != null)
      {
        if (vid.Media.Contents.Count > 0)
        {
          PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri));
        }
        else
        {
          PlayblackUrl = vid.AlternateUri.ToString();
        }

        PlayListItem playlistItem = new PlayListItem();
        playlistItem.Type = PlayListItem.PlayListItemType.VideoStream; // Playlists.PlayListItem.PlayListItemType.Audio;
        qa.Entry = vid;
        playlistItem.FileName = PlayblackUrl;
        playlistItem.Description = pItem.Label;
        if (vid.Duration != null && vid.Duration.Seconds != null)
          playlistItem.Duration = Convert.ToInt32(vid.Duration.Seconds, 10);
        playlistItem.MusicTag = qa;
        playList.Add(playlistItem);
      }
    }

    public void AddItemToPlayList(YouTubeEntry vid, ref PlayList playList, VideoInfo qa, int nextitem)
    {
        if (playList == null || vid == null)
            return;
        string PlayblackUrl = "";

        List<GUIListItem> list = new List<GUIListItem>();

        if (vid != null)
        {
          PlayblackUrl = vid.Media.Contents.Count > 0
                           ? string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri))
                           : vid.AlternateUri.ToString();
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.Type = PlayListItem.PlayListItemType.VideoStream;
          qa.Entry = vid;
          playlistItem.FileName = PlayblackUrl;
          playlistItem.Description = vid.Title.Text;
          if (vid.Duration != null && vid.Duration.Seconds != null)
            playlistItem.Duration = Convert.ToInt32(vid.Duration.Seconds, 10);

          playlistItem.MusicTag = qa;
          if (nextitem < 0)
          {
            playList.Add(playlistItem);
          }
          else
          {
            playList.Insert(playlistItem, nextitem);
          }
        }
    }

    static public string GetFanArtImage(string artist)
    {
      return Youtube2MP._settings.FanartDir.Replace("%artist%", Utils.MakeFileName(artist));
    }

    #region download manager

    protected void DownloadFile(string url,string localFile)
    {
      try
      {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, localFile);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }


    private string DownloadImage(string Url)
    {
      //string localFile = GetLocalImageFileName(Url);
      //if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      //{
      //  downloaQueue.Enqueue(new DownloadFileObject(localFile, Url));
      //}
      return DownloadImage(Url, null);
    }

    private string DownloadImage(string Url, string localFile, GUIListItem item)
    {
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, Url, item));
      }
      return localFile;
    }

    public string DownloadImage(string Url, GUIListItem listitem)
    {
      if (string.IsNullOrEmpty(Url))
        return string.Empty;
      string localFile = GetLocalImageFileName(Url);
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(Url))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, Url, listitem));
      }
      return localFile;
    }

    public void OnDownloadTimedEvent(object source, ElapsedEventArgs e)
    {
      BackgroundWorker backgroundWorker1 = new BackgroundWorker();
      backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
      backgroundWorker1.RunWorkerAsync();
    }

    void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
      OnDownloadTimedEvent();
    }

    public void OnDownloadTimedEvent()
    {
      if (!Client.IsBusy && downloaQueue.Count > 0)
      {
        curentDownlodingFile = (DownloadFileObject) downloaQueue.Dequeue();
        try
        {
          if (curentDownlodingFile.ListItem != null)
          {
            SiteItemEntry siteItemEntry = curentDownlodingFile.ListItem.MusicTag as SiteItemEntry;
            if (siteItemEntry != null && !string.IsNullOrEmpty(siteItemEntry.GetValue("id")))
            {
              ArtistItem artistItem = ArtistManager.Instance.GetArtistsById(siteItemEntry.GetValue("id"));
              if (string.IsNullOrEmpty(curentDownlodingFile.Url) || curentDownlodingFile.Url.Contains("@") ||
                  curentDownlodingFile.Url.Contains("ytimg.com"))
              {
                try
                {
                  Artist artist = new Artist(artistItem.Name, Youtube2MP.LastFmProfile.Session);
                  artistItem.Img_url = artist.GetImageURL(ImageSize.Huge);
                  ArtistManager.Instance.Save(artistItem);
                  curentDownlodingFile.Url = artistItem.Img_url;
                  curentDownlodingFile.FileName = GetLocalImageFileName(curentDownlodingFile.Url);
                }
                catch
                {
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
        if (!string.IsNullOrEmpty(curentDownlodingFile.FileName) &&!File.Exists(curentDownlodingFile.FileName))
        {
          try
          {
            Client.DownloadFileAsync(new Uri(curentDownlodingFile.Url), Path.GetTempPath() + @"\station.png");
          }
          catch
          {
            downloaQueue.Enqueue(curentDownlodingFile);
          }

        }
        else
        {
          OnDownloadTimedEvent(null, null);
        }
      }
    }

    public void DownloadLogoEnd(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        try
        {
          File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
          if (curentDownlodingFile.ListItem != null && File.Exists(curentDownlodingFile.FileName))
          {
            curentDownlodingFile.ListItem.ThumbnailImage = curentDownlodingFile.FileName;
            curentDownlodingFile.ListItem.IconImage = curentDownlodingFile.FileName;
            curentDownlodingFile.ListItem.IconImageBig = curentDownlodingFile.FileName;
            curentDownlodingFile.ListItem.RefreshCoverArt();
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
        //UpdateGui();
      }
      OnDownloadTimedEvent(null, null);
    }
   
    protected  YouTubeQuery SetParamToYouTubeQuery(YouTubeQuery query, bool safe)
    {

      //order results by the number of views (most viewed first)
      query.OrderBy = "viewCount";
      query.StartIndex = 1;
      //query.LR = "hu";
      if (_setting.UseExtremFilter)
        query.NumberToRetrieve = 50;
      else
        query.NumberToRetrieve = 50;
      ////exclude restricted content from the search
      //query.Racy = "exclude";
      query.SafeSearch = YouTubeQuery.SafeSearchValues.None;
      if (uploadtime != YouTubeQuery.UploadTime.AllTime)
        query.Time = uploadtime;
      if (_setting.MusicFilter && !safe)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      return query;
    }

    protected ArtistItem GetArtist(YouTubeEntry entry) 
    {
      ArtistItem artistItem = DatabaseProvider.InstanInstance.GetArtist(entry);
      if (artistItem != null)
        return artistItem;
      string vidId = Youtube2MP.GetVideoId(entry);
      artistItem = ArtistManager.Instance.SitesCache.GetByVideoId(vidId) != null
                                ? ArtistManager.Instance.Grabber.GetFromVideoSite(
                                  ArtistManager.Instance.SitesCache.GetByVideoId(vidId).SIte)
                                : ArtistManager.Instance.Grabber.GetFromVideoId(vidId);

      if (string.IsNullOrEmpty(artistItem.Id) && entry.Title.Text.Contains("-"))
      {
        artistItem =
          ArtistManager.Instance.GetArtistsByName(entry.Title.Text.Split('-')[0].TrimEnd());
      }

      if (!string.IsNullOrEmpty(artistItem.Id))
      {
        ArtistManager.Instance.Save(artistItem);
        DatabaseProvider.InstanInstance.Save(entry, artistItem);
      }
      return artistItem;
    }

    protected string GetArtistName(YouTubeEntry entry)
    {
      ArtistItem artistItem = GetArtist(entry);
      if (!string.IsNullOrEmpty(artistItem.Name))
        return artistItem.Name;
      if (entry.Title.Text.Contains("-"))
      {
        return entry.Title.Text.Split('-')[0].TrimEnd();
      }
      return "";
    }


    #endregion
  }
}
