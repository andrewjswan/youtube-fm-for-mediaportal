using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace YouTubePlugin.Class
{
  public static class Translation
  {
    #region Private variables

    //private static Logger logger = LogManager.GetCurrentClassLogger();
    private static Dictionary<string, string> _translations;
    private static readonly string _path = string.Empty;
    private static readonly DateTimeFormatInfo _info;

    #endregion

    #region Constructor

    static Translation()
    {
      string lang;

      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.Name;
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }

      Log.Info("Using language " + lang);

      _path = Config.GetSubFolder(Config.Dir.Language, "YouTube.Fm");

      if (!System.IO.Directory.Exists(_path))
        System.IO.Directory.CreateDirectory(_path);

      LoadTranslations(lang);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the translated strings collection in the active language
    /// </summary>
    public static Dictionary<string, string> Strings
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, string>();
          Type transType = typeof(Translation);
          FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
          foreach (FieldInfo field in fields)
          {
            _translations.Add(field.Name, field.GetValue(transType).ToString());
          }
        }
        return _translations;
      }
    }

    #endregion

    #region Public Methods

    public static int LoadTranslations(string lang)
    {
      XmlDocument doc = new XmlDocument();
      Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
      string langPath = "";
      try
      {
        langPath = Path.Combine(_path, lang + ".xml");
        doc.Load(langPath);
      }
      catch (Exception e)
      {
        if (lang == "en")
          return 0; // otherwise we are in an endless loop!

        if (e.GetType() == typeof(FileNotFoundException))
          Log.Warn("Cannot find translation file {0}.  Failing back to English", langPath);
        else
        {
          Log.Error("Error in translation xml file: {0}. Failing back to English", lang);
          Log.Error(e);
        }

        return LoadTranslations("en");
      }
      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
          }
          catch (Exception ex)
          {
            Log.Error("Error in Translation Engine");
            Log.Error(ex);
          }
      }

      Type TransType = typeof(Translation);
      FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
      foreach (FieldInfo fi in fieldInfos)
      {
        if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
          TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
        else
          Log.Info("Translation not found for field: {0}.  Using hard-coded English default.", fi.Name);
      }
      return TranslatedStrings.Count;
    }

    public static string GetByName(string name)
    {
      if (!Strings.ContainsKey(name))
        return name;

      return Strings[name];
    }

    public static string GetByName(string name, params object[] args)
    {
      return String.Format(GetByName(name), args);
    }

    /// <summary>
    /// Takes an input string and replaces all ${named} variables with the proper translation if available
    /// </summary>
    /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
    /// <returns>translated input string</returns>
    public static string ParseString(string input)
    {
      Regex replacements = new Regex(@"\$\{([^\}]+)\}");
      MatchCollection matches = replacements.Matches(input);
      foreach (Match match in matches)
      {
        input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
      }
      return input;
    }


    //public static string GetMediaType(MediaType mediaType)
    //{
    //  switch (mediaType)
    //  {
    //    case MyAlarm.MediaType.File:
    //      return File;

    //    case MyAlarm.MediaType.PlayList:
    //      return Playlist;

    //    case MyAlarm.MediaType.Message:
    //      return Message;

    //    default:
    //      return String.Empty;
    //  }
    //}

    public static string GetDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetDayName(dayOfWeek);
    }
    public static string GetShortestDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetShortestDayName(dayOfWeek);
    }

    #endregion

    #region Translations / Strings

    /// <summary>
    /// These will be loaded with the language files content
    /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
    /// if that also fails it will use the hardcoded strings as a last resort.
    /// </summary>
    

    // A
    public static string AddAllPlaylist = "Add All to playlist";
    public static string AddPlaylist = "Add to playlist";
    public static string AddFavorites = "Add to favorites";
    public static string AddWatchLater = "Add to Watch Later";
    public static string AllMusicVideosFrom = "All music videos from {0}";
    public static string AllVideosFromUser = "All videos from this user : {0}";
    public static string ArtistInfo = "Artist Info";
    public static string AnotherDonwnloadProgress = "Another donwnload is in progress";
    public static string AutomaticallyFillPlaylist = "Automatically fill playlist with";
    public static string Autor = "Autor";

    // C
    public static string ContextMenu = "Context Menu";

    // D
    public static string DidYouMean  = "Did you mean ?";
    public static string DownloadProgress = "Download progress";
    public static string DownloadVideo = "Download Video";
    public static string Dislike = "Dislike";
    public static string Duration = "Duration";

    // E

    // F
    public static string Fullscreen = "Fullscreen";
    public static string FavoriteCount = "FavoriteCount";


    // G

    // H
    public static string Home = "Home";

    // I
    public static string ItemAlreadyDownloaded  = "Item already downloaded !";
    public static string Info = "Info";

    // L
    public static string Like = "Like";

    // M
    public static string Message = "Message";



    // N
    public static string NextPage = "Next page";
    public static string NewSearch = "New Search";
    public static string NoSearchHistory = "No search history was found";
    public static string NoItemWasFound  = "No item was found !";
    public static string NoItemToDisplay = "No item to dispplay !";
    public static string NowPlaying = "Now playing";
    public static string NoVideoResponse = "This video doesn't have any video response !";

    // O
    public static string Options = "Options";

    // P
    public static string PlayNext = "Play Next";
    public static string Playlist = "Playlist";
    public static string PlaylistSummary = "Created or modified in MediaPortal";
    public static string PlaylistSavingProgress = "Playlist saving progress ";
    public static string PublishDate = "PublishDate";

    // R
    public static string RelatedVideos = "Related Videos";

    // S
    public static string Search = "Search";
    public static string SearchHistory = "Search History";
    public static string SimilarArtists = "Similar Artists";
    public static string ScrobbleSimilarVideos = "Youtube related videos";
    public static string ScrobbleNeighboursLike = "Tracks your neighbours like";
    public static string ScrobbleRecentlyPlayed = "Tracks your recently played";
    public static string ShowPreviousWindow = "Show Previous Window";
    public static string ScrobbleMode = "Scroble:";
    public static string SomePlaylistItemNotSaved = "Some playlist item not saved !";

    // T


    // U
    
    
    // V
    public static string VideoResponses = "Video responses for this video";
    public static string VideoInfo = "Video Info";
    public static string ViewCount = "View Count";

    // W
    public static string WrongRequestWrongUser = "Wrong request or wrong user identification ";
    public static string WrongUser = "Wrong username or password or account is disabled ";
    public static string WatchLater = "Watch Later";

    // Y

    #endregion

  }

}