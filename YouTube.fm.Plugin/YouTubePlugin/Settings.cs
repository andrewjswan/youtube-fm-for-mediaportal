using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class;
using Action = MediaPortal.GUI.Library.Action;

namespace YouTubePlugin
{
  public class Settings
  {


    public List<string> OldCats
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
        cats.Add("Most Share"); //8
        cats.Add("Trending Videos"); //9
        return cats;
      }
    }

    public List<string> TimeList
    {
      get
      {
        List<string> time = new List<string>();
        time.Add("All Time");
        time.Add("Today");
        time.Add("This Week");
        time.Add("This Month");
        return time;
      }
    }

    public Dictionary<string, string> Regions
    {
      get
      {
        Dictionary<string, string> list = new Dictionary<string, string>();
        list.Add("All", "");
        list.Add("Argentina", "AR");
        list.Add("Australia", "AU");
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
        list.Add("South Africa", "ZA");
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


    //public int InitialCat { get; set; }
    
    public int StartUpOpt { get; set; }

    public string InitialSearch { get; set; }


    public string PluginName { get; set; }

    public List<string> SearchHistory { get; set; }

    public int InitialDisplay { get; set; }

    public string User { get; set; }

    public string Password { get; set; }

    public string FanartDir { get; set; }
    public string CacheDir { get; set; }

    public bool LoadOnlineFanart { get; set; }

    public bool ShowNowPlaying { get; set; }

    public bool UseExtremFilter { get; set; }

    public int VideoQuality { get; set; }

    public bool UseSMSStyleKeyBoard { get; set; }

    public Action.ActionType InstantAction { get; set; }

    public string DownloadFolder { get; set; }

    private bool _oldStyleHome;
    public bool OldStyleHome
    {
      get { return false; }
      set { _oldStyleHome = value; }
    }

    public string LastFmUser { get; set; }
    public string LastFmPass { get; set; }
    public bool LastFmNowPlay { get; set; }
    public bool LastFmSubmit { get; set; }

    public void Load()
    {
      using (var xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        this.PluginName = xmlreader.GetValueAsString("youtubevideos", "PluginName", "YouTube.fm");
        this.InitialDisplay = xmlreader.GetValueAsInt("youtubevideos", "InitialDisplay", 3);
        this.User = xmlreader.GetValueAsString("youtubevideos", "user", string.Empty);
        this.Password = xmlreader.GetValueAsString("youtubevideos", "password", string.Empty);
        this.StartUpOpt = xmlreader.GetValueAsInt("youtubevideos", "StartUpOpt", 0);
        this.VideoQuality = xmlreader.GetValueAsInt("youtubevideos", "VideoQuality", 0);
        this.InitialSearch = xmlreader.GetValueAsString("youtubevideos", "InitialSearch", string.Empty);
        this.MusicFilter = xmlreader.GetValueAsBool("youtubevideos", "MusicFilter", true);
        this.UseSMSStyleKeyBoard = xmlreader.GetValueAsBool("youtubevideos", "UseSMSStyleKeyBoard", true);
        string his = xmlreader.GetValueAsString("youtubevideos", "searchhistory", string.Empty);
        this.ShowNowPlaying = xmlreader.GetValueAsBool("youtubevideos", "ShowNowPlaying", true);
        this.UseExtremFilter = xmlreader.GetValueAsBool("youtubevideos", "UseExtremFilter", false);
        this.LoadOnlineFanart = xmlreader.GetValueAsBool("youtubevideos", "LoadOnlineFanart", true);
        this.CacheDir = xmlreader.GetValueAsString("youtubevideos", "CacheDir", string.Empty);
        this.FanartDir = xmlreader.GetValueAsString("youtubevideos", "FanartFolder", string.Empty);
        this.DownloadFolder = xmlreader.GetValueAsString("youtubevideos", "DownloadFolder",
                                                         Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                                         "\\Videos");
        this.OldStyleHome = xmlreader.GetValueAsBool("youtubevideos", "OldStyleHome", false);
        this.LastFmUser = xmlreader.GetValueAsString("youtubevideos", "LastFmUser", string.Empty);
        this.LastFmPass = xmlreader.GetValueAsString("youtubevideos", "LastFmPass", string.Empty);
        this.LastFmNowPlay = xmlreader.GetValueAsBool("youtubevideos", "LastFmNowPlay", false);
        this.LastFmSubmit = xmlreader.GetValueAsBool("youtubevideos", "LastFmSubmit", false);
        foreach (string s in his.Split('|'))
        {
          if (!string.IsNullOrEmpty(s.Trim()))
            SearchHistory.Add(s);
        }
      }
      if (string.IsNullOrEmpty(CacheDir))
      {
        CacheDir = Config.GetFile(Config.Dir.Thumbs, @"Youtube.Fm\Cache");
      }
      if (string.IsNullOrEmpty(FanartDir))
      {
        FanartDir = Config.GetFile(Config.Dir.Thumbs, @"Youtube.Fm\Fanart\", "%artist%.png");
      }
      this.LocalFile.Load();
      MainMenu.Load("youtubefmMenu.xml");
    }

    public void Save()
    {
      using (var xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("youtubevideos", "PluginName", this.PluginName);
        xmlwriter.SetValue("youtubevideos", "InitialDisplay", this.InitialDisplay);
        xmlwriter.SetValue("youtubevideos", "user", User);
        xmlwriter.SetValue("youtubevideos", "password", Password);
        xmlwriter.SetValue("youtubevideos", "StartUpOpt", this.StartUpOpt);
        xmlwriter.SetValue("youtubevideos", "InitialSearch", this.InitialSearch);
        xmlwriter.SetValue("youtubevideos", "VideoQuality", this.VideoQuality);
        xmlwriter.SetValue("youtubevideos", "DownloadFolder", this.DownloadFolder);
        xmlwriter.SetValue("youtubevideos", "FanartFolder", this.FanartDir);
        xmlwriter.SetValue("youtubevideos", "CacheDir", this.CacheDir);
        xmlwriter.SetValue("youtubevideos", "LastFmUser", this.LastFmUser);
        xmlwriter.SetValue("youtubevideos", "LastFmPass", this.LastFmPass);
        xmlwriter.SetValueAsBool("youtubevideos", "LastFmNowPlay", this.LastFmNowPlay);
        xmlwriter.SetValueAsBool("youtubevideos", "LastFmSubmit", this.LastFmSubmit);
        xmlwriter.SetValueAsBool("youtubevideos", "MusicFilter", this.MusicFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "ShowNowPlaying", this.ShowNowPlaying);
        xmlwriter.SetValueAsBool("youtubevideos", "UseExtremFilter", this.UseExtremFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "UseSMSStyleKeyBoard", this.UseSMSStyleKeyBoard);
        xmlwriter.SetValueAsBool("youtubevideos", "LoadOnlineFanart", this.LoadOnlineFanart);
        xmlwriter.SetValueAsBool("youtubevideos", "OldStyleHome", this.OldStyleHome);
        string his = "";
        foreach (string s in SearchHistory)
        {
          his += s + "|";
        }
        xmlwriter.SetValue("youtubevideos", "searchhistory", his);
      }
      CreateFolders();
      this.LocalFile.Save();
      MainMenu.Save("youtubefmMenu.xml");
      MediaPortal.Profile.Settings.SaveCache();
    }

    public void CreateFolders()
    {
      try
      {
        if (!Directory.Exists(CacheDir))
        {
          Directory.CreateDirectory(CacheDir);
        }
        if (!Directory.Exists(Path.GetDirectoryName(FanartDir)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(FanartDir));
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
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
