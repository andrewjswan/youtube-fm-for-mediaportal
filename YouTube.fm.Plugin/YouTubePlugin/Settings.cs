using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;


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


    public void Load()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        this.PluginName = xmlreader.GetValueAsString("youtubevideos", "PluginName", "LastTube");
        this.InitialDisplay = xmlreader.GetValueAsInt("youtubevideos", "InitialDisplay", 3);
        this.User = xmlreader.GetValueAsString("youtubevideos", "user", string.Empty);
        this.Password = xmlreader.GetValueAsString("youtubevideos", "password", string.Empty);
        this.InitialCat = xmlreader.GetValueAsInt("youtubevideos", "InitialCat", 1);
        this.InitialSearch = xmlreader.GetValueAsString("youtubevideos", "InitialSearch", string.Empty);
        this.MusicFilter = xmlreader.GetValueAsBool("youtubevideos", "MusicFilter", true);
        string his = xmlreader.GetValueAsString("youtubevideos", "searchhistory", string.Empty);
        this.Time = xmlreader.GetValueAsBool("youtubevideos", "time", false);
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
        xmlwriter.SetValue("youtubevideos", "InitialDisplay", this.InitialDisplay);
        xmlwriter.SetValue("youtubevideos", "InitialSearch", this.InitialSearch);
        xmlwriter.SetValueAsBool("youtubevideos", "MusicFilter", this.MusicFilter);
        xmlwriter.SetValueAsBool("youtubevideos", "time", this.Time);
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
      this.PluginName = "LastTube";
      this.User = "";
      this.Password = "";
      this.SearchHistory = new List<string>();
    }
    

  }
}
