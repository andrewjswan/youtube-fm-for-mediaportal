using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

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

    private bool musicfilter;

    public bool MusicFilter
    {
      get { return musicfilter; }
      set { musicfilter = value; }
    }


    private int initialCat;

    public int InitialCat
    {
      get { return initialCat; }
      set { initialCat = value; }
    }


    private string initialSearch;

    public string InitialSearch
    {
      get { return initialSearch; }
      set { initialSearch = value; }
    }


    private string pluginName;

    public string PluginName
    {
      get { return pluginName; }
      set { pluginName = value; }
    }

    private List<string> searchHistory;

    public List<string> SearchHistory
    {
      get { return searchHistory; }
      set { searchHistory = value; }
    }


    private int initial;

    public int InitialDisplay
    {
      get { return initial; }
      set { initial = value; }
    }

    private string user;

    public string User
    {
      get { return user; }
      set { user = value; }
    }

    private string password;

    public string Password
    {
      get { return password; }
      set { password = value; }
    }

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


    public void Load()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        this.PluginName = xmlreader.GetValueAsString("youtubevideos", "PluginName", "YouTube.fm");
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
        foreach (string s in his.Split('|'))
        {
          if (!string.IsNullOrEmpty(s.Trim()))
            SearchHistory.Add(s);
        }
      }
    }

    public void Save()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("youtubevideos", "PluginName", this.PluginName);
        xmlwriter.SetValue("youtubevideos", "InitialDisplay", this.InitialDisplay);
        xmlwriter.SetValue("youtubevideos", "user", User);
        xmlwriter.SetValue("youtubevideos", "password", Password);
        xmlwriter.SetValue("youtubevideos", "InitialCat", this.InitialCat);
        xmlwriter.SetValue("youtubevideos", "InitialSearch", this.InitialSearch);
        xmlwriter.SetValue("youtubevideos", "VideoQuality", this.VideoQuality);
        xmlwriter.SetValue("youtubevideos", "InstantAction", (int)this.InstantAction);
        xmlwriter.SetValue("youtubevideos", "InstantCharInt", this.InstantChar);
        xmlwriter.SetValueAsBool("youtubevideos", "MusicFilter", this.MusicFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "time", this.Time);
        xmlwriter.SetValueAsBool("youtubevideos", "ShowNowPlaying", this.ShowNowPlaying);
        xmlwriter.SetValueAsBool("youtubevideos", "UseYouTubePlayer", this.UseYouTubePlayer);
        xmlwriter.SetValueAsBool("youtubevideos", "UseExtremFilter", this.UseExtremFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "UseSMSStyleKeyBoard", this.UseSMSStyleKeyBoard);
        string his = "";
        foreach (string s in SearchHistory)
        {
          his += s + "|";
        }
        xmlwriter.SetValue("youtubevideos", "searchhistory", his);
      }
    }

    public Settings()
    {
      this.PluginName = "YouTube.fm";
      this.User = "";
      this.Password = "";
      this.SearchHistory = new List<string>();
    }
    

  }
}
