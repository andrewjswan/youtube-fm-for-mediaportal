using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class;

namespace YouTubePlugin
{
  public class Settings
  {

    
    public List<string> Cats
    {
      get
      {
        List<string> cats = new List<string>();
        cats.Add("Most Viewed"); //0
        cats.Add("Top Rated"); // 1
        cats.Add("Recently Featured"); //2
        cats.Add("Most Discussed"); //3
        cats.Add("Top Favorites"); //4
        cats.Add("Most Linked"); //5
        cats.Add("Most Responded"); //6
        cats.Add("Most Recent"); //7
        cats.Add("Favorites"); //8
        cats.Add("Downloaded items"); //9
        return cats;
      }
    }

    public List<string> TimeList
    {
      get
      {
        List<string> time = new List<string>();
        time.Add("Today");
        time.Add("This Week");
        time.Add("This Month");
        time.Add("All Time");
        return time;
      }
    }

      public Dictionary<string ,string> Regions
      {
          get
          {
              Dictionary<string, string> list = new Dictionary<string, string>();
              list.Add("All", "");
              list.Add("Australia","AU");
              list.Add("Brazil", "BR");
              list.Add("Canada", "CA");
              list.Add("Czech Republic", "CZ");
              list.Add("France", "FR");
              list.Add("Germany", "DE");
              list.Add("Great Britain", "GB");
              list.Add("Holland", "NL");
              list.Add("Hong Kong", "HK");
              list.Add("India", "IN");
              list.Add("Ireland", "IE");
              list.Add("Israel", "IL");
              list.Add("Italy", "IT");
              list.Add("Japan", "JP");
              list.Add("Mexico", "MX");
              list.Add("New Zealand", "NZ");
              list.Add("Poland", "PL");
              list.Add("Russia", "RU");
              list.Add("South Korea", "KR");
              list.Add("Spain", "ES");
              list.Add("Sweden", "SE");
              list.Add("Taiwan", "TW");
              list.Add("United States", "US");
              return list;
          }
      }

      public LocalFileEnumerator LocalFile { get; set; }

    public SiteItemEnumerator MainMenu { get; set; }

      public bool MusicFilter { get; set; }


      public int InitialCat { get; set; }


      public string InitialSearch { get; set; }


      public string PluginName { get; set; }

      public string Region { get; set; }

      public List<string> SearchHistory { get; set; }
      
      public int InitialDisplay { get; set; }

      public string User { get; set; }

      public string Password { get; set; }

      public string FanartDir { get; set; }

      public bool LoadOnlineFanart { get; set; }


      private bool time;
    public bool Time
    {
      get { return time; }
      set { time = value; }
    }

    private bool showNowPlaying;

    public bool ShowNowPlaying
    {
      get { return showNowPlaying; }
      set { showNowPlaying = value; }
    }

    private bool useYouTubePlayer;

    public bool UseYouTubePlayer
    {
      get { return false; }
      set { useYouTubePlayer = value; }
    }

    private bool useExtremFilter;

    public bool UseExtremFilter
    {
      get { return useExtremFilter; }
      set { useExtremFilter = value; }
    }

    private int videoQuality;

    public int VideoQuality
    {
      get { return videoQuality; }
      set { videoQuality = value; }
    }

    private bool useSMSStyleKeyBoard;

    public bool UseSMSStyleKeyBoard
    {
      get { return useSMSStyleKeyBoard; }
      set { useSMSStyleKeyBoard = value; }
    }

    private Action.ActionType instantAction;

    public Action.ActionType InstantAction
    {
      get { return instantAction; }
      set { instantAction = value; }
    }

    private int instantChar;

    public int InstantChar
    {
      get { return instantChar; }
      set { instantChar = value; }
    }

    private string downloadFolder;

    public string DownloadFolder
    {
      get { return downloadFolder; }
      set { downloadFolder = value; }
    }


    public void Load()
    {
      using (var xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        this.PluginName = xmlreader.GetValueAsString("youtubevideos", "PluginName", "YouTube.fm");
        this.Region = xmlreader.GetValueAsString("youtubevideos", "Region", "Ask");
        this.InitialDisplay = xmlreader.GetValueAsInt("youtubevideos", "InitialDisplay", 3);
        this.User = xmlreader.GetValueAsString("youtubevideos", "user", string.Empty);
        this.Password = xmlreader.GetValueAsString("youtubevideos", "password", string.Empty);
        this.InitialCat = xmlreader.GetValueAsInt("youtubevideos", "InitialCat", 1);
        this.VideoQuality = xmlreader.GetValueAsInt("youtubevideos", "VideoQuality", 0);
        this.InstantAction = (Action.ActionType)xmlreader.GetValueAsInt("youtubevideos", "InstantAction", (int)(Action.ActionType.REMOTE_1));
        this.InitialSearch = xmlreader.GetValueAsString("youtubevideos", "InitialSearch", string.Empty);
        this.InstantChar = xmlreader.GetValueAsInt("youtubevideos", "InstantCharInt", 01);
        this.MusicFilter = xmlreader.GetValueAsBool("youtubevideos", "MusicFilter", true);
        this.UseSMSStyleKeyBoard = xmlreader.GetValueAsBool("youtubevideos", "UseSMSStyleKeyBoard", true);
        string his = xmlreader.GetValueAsString("youtubevideos", "searchhistory", string.Empty);
        this.Time = xmlreader.GetValueAsBool("youtubevideos", "time", false);
        this.ShowNowPlaying = xmlreader.GetValueAsBool("youtubevideos", "ShowNowPlaying", true);
        this.UseYouTubePlayer = xmlreader.GetValueAsBool("youtubevideos", "UseYouTubePlayer", false);
        this.UseExtremFilter = xmlreader.GetValueAsBool("youtubevideos", "UseExtremFilter", false);
        this.LoadOnlineFanart = xmlreader.GetValueAsBool("youtubevideos", "LoadOnlineFanart", true);
        this.FanartDir = xmlreader.GetValueAsString("youtubevideos", "FanartFolder", string.Empty);
        this.DownloadFolder = xmlreader.GetValueAsString("youtubevideos", "DownloadFolder", Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\My Videos");
        foreach (string s in his.Split('|'))
        {
          if (!string.IsNullOrEmpty(s.Trim()))
            SearchHistory.Add(s);
        }
      }
      this.LocalFile.Load();
      MainMenu.Load("youtubefmMenu.xml");
    }

    public void Save()
    {
      using (var xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("youtubevideos", "PluginName", this.PluginName);
        xmlwriter.SetValue("youtubevideos", "Region", this.Region);
        xmlwriter.SetValue("youtubevideos", "InitialDisplay", this.InitialDisplay);
        xmlwriter.SetValue("youtubevideos", "user", User);
        xmlwriter.SetValue("youtubevideos", "password", Password);
        xmlwriter.SetValue("youtubevideos", "InitialCat", this.InitialCat);
        xmlwriter.SetValue("youtubevideos", "InitialSearch", this.InitialSearch);
        xmlwriter.SetValue("youtubevideos", "VideoQuality", this.VideoQuality);
        xmlwriter.SetValue("youtubevideos", "InstantAction", (int)this.InstantAction);
        xmlwriter.SetValue("youtubevideos", "InstantCharInt", this.InstantChar);
        xmlwriter.SetValue("youtubevideos", "DownloadFolder", this.DownloadFolder);
        xmlwriter.SetValue("youtubevideos", "FanartFolder", this.FanartDir);
        xmlwriter.SetValueAsBool("youtubevideos", "MusicFilter", this.MusicFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "time", this.Time);
        xmlwriter.SetValueAsBool("youtubevideos", "ShowNowPlaying", this.ShowNowPlaying);
        xmlwriter.SetValueAsBool("youtubevideos", "UseYouTubePlayer", this.UseYouTubePlayer);
        xmlwriter.SetValueAsBool("youtubevideos", "UseExtremFilter", this.UseExtremFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "UseSMSStyleKeyBoard", this.UseSMSStyleKeyBoard);
        xmlwriter.SetValueAsBool("youtubevideos", "LoadOnlineFanart", this.LoadOnlineFanart);
        string his = "";
        foreach (string s in SearchHistory)
        {
          his += s + "|";
        }
        xmlwriter.SetValue("youtubevideos", "searchhistory", his);
      }
      this.LocalFile.Save();
      MainMenu.Save("youtubefmMenu.xml");
      MediaPortal.Profile.Settings.SaveCache();
    }

    public Settings()
    {
      this.PluginName = "YouTube.fm";
      this.User = "";
      this.Password = "";
      this.SearchHistory = new List<string>();
      this.LocalFile = new LocalFileEnumerator();
      MainMenu = new SiteItemEnumerator();
    }
    

  }
}
