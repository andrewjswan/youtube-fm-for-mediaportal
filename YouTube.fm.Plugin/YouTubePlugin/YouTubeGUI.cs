using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Playlists;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using Action = MediaPortal.GUI.Library.Action;


namespace YouTubePlugin
{
  public enum View
  {
    List = 0,
    Icons = 1,
    BigIcons = 2,
    Albums = 3,
    PlayList = 4,
    Filmstrip = 5
  }
  [PluginIcons("YouTubePlugin.logo.png", "YouTubePlugin.logo_disabled.png")]
  public class YouTubeGUI : YoutubeGUIBase, ISetupForm 
  {

    #region MapSettings class
    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;

      public MapSettings()
      {
        // Set default view
        _SortBy = 0;
        _ViewAs = (int)View.List;
        _SortAscending = true;
      }

      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }
    #endregion

    #region Base variables

    #endregion


    #region locale vars


    private Stack NavigationStack = new Stack();
    MapSettings mapSettings = new MapSettings();
    static GUIDialogProgress dlgProgress;

    YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal",  "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");


    #endregion

    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIFacadeControl listControl = null;
    //[SkinControlAttribute(2)]
    //protected GUISortButtonControl sortButton = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl homeButton = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnSwitchView = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl searchButton = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl searchHistoryButton = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnPlayList = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnNowPlaying = null;

    #endregion


    public YouTubeGUI()
    {
      _setting.Load();
      GetID = GetWindowId();
      Youtube2MP._settings = _setting;
      Youtube2MP.service = service;
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = false;
      updateStationLogoTimer.Elapsed += OnDownloadTimedEvent;
      Client.DownloadFileCompleted += DownloadLogoEnd;
      VideoDownloader.DownloadComplete += VideoDownloader_DownloadComplete;
      VideoDownloader.ProgressChanged += VideoDownloader_ProgressChanged;
      ArtistManager.Instance.InitDatabase();      
      GUIWindowManager.Receivers += GUIWindowManager_Receivers;
 
    }

    void VideoDownloader_ProgressChanged(object sender, DownloadEventArgs e)
    {
      if (dlgProgress != null)
      {
        dlgProgress.SetLine(2, string.Format("{0} Mb / {1} Mb ({2}%)", e.TotalFileSize/1024/1024, e.CurrentFileSize/1024/1024, e.PercentDone));
        dlgProgress.ShowProgressBar(true);
        dlgProgress.SetPercentage(e.PercentDone);
        dlgProgress.Progress();
      }
    }

    void VideoDownloader_DownloadComplete(object sender, EventArgs e)
    {

      GUIDialogNotify dlg1 = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dlg1 != null)
      {
        dlg1.Reset();
        dlg1.SetHeading("Download done");
        dlg1.SetText(VideoDownloader.DownloadingTo);
        dlg1.Reset();
        dlg1.TimeOut = 5;
        dlg1.DoModal(GetID);
      }
      Youtube2MP._settings.LocalFile.Items.Add(new LocalFileStruct(VideoDownloader.DownloadingTo,Youtube2MP.GetVideoId(VideoDownloader.Entry), VideoDownloader.Entry.Title.Text));
      Youtube2MP._settings.LocalFile.Save();
      string imageFile = GetLocalImageFileName(GetBestUrl(VideoDownloader.Entry.Media.Thumbnails));
      try
      {
          if (File.Exists(imageFile))
          {
              File.Copy(imageFile, Path.GetDirectoryName(VideoDownloader.DownloadingTo) + "\\" + Path.GetFileNameWithoutExtension(VideoDownloader.DownloadingTo) + ".png");
          }
      }
      catch
      {
      }
      if (dlgProgress != null)
      {
        dlgProgress.SetPercentage(100);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Close();
        dlgProgress = null;
      }
    }

    void GUIWindowManager_Receivers(GUIMessage message)
    {
    }

    #region ISetupForm Members
    // return name of the plugin
    public string PluginName()
    {
      return _setting.PluginName;
    }
    // returns plugin description
    public string Description()
    {
      return "Plugin for expose the YouTube Music contents";
    }
    // returns author
    public string Author()
    {
      return "Dukus";
    }
    // shows the setup dialog
    public void ShowPlugin()
    {
      SetupForm setup = new SetupForm();
      setup._settings = _setting;
      setup.ShowDialog();
    }
    // enable / disable
    public bool CanEnable()
    {
      return true;
    }
    // returns the unique id again
    public int GetWindowId()
    {
      return 29050;
    }
    // default enable?
    public bool DefaultEnabled()
    {
      return true;
    }
    // has setup gui?
    public bool HasSetup()
    {
      return true ;
    }
    // home button
    public bool GetHome(out string strButtonText, out string strButtonImage,
      out string strButtonImageFocus, out string strPictureImage)
    {
      // set the values for the buttom
      strButtonText = _setting.PluginName;

      // no image or picture
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = "hover_youtubefm.png";

      return true;
    }
    // init the skin
    public override bool Init()
    {
      if (!string.IsNullOrEmpty(_setting.User))
      {
        service.setUserCredentials(_setting.User, _setting.Password);
        Youtube2MP.request.Service = service;
      }

      
      return Load(GUIGraphicsContext.Skin + @"\youtubevideosbase.xml");
    }

     //do the init before page load
    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      foreach (string name in Translation.Strings.Keys)
      {
        SetProperty("#Youtube.fm.Translation." + name + ".Label", Translation.Strings[name]);
      }

      if(Youtube2MP.LastFmProfile==null)
      {
        Youtube2MP.LastFmProfile = new LastProfile();
        try
        {
          Youtube2MP.LastFmProfile.Login(_setting.LastFmUser, _setting.LastFmPass);
        }
        catch (Exception)
        {
          
          
        }
      }
      
      UpdateGui();
      ShowPanel();

      GUIControl.FocusControl(GetID, listControl.GetID);
      GUIPropertyManager.SetProperty("#nowplaying", " ");
      if (MessageGUI.Item != null)
      {
        ArtistItem artistItem = MessageGUI.Item as ArtistItem;
        SiteItemEntry entry=new SiteItemEntry();
        entry.Provider = "Artists";
        entry.SetValue("letter", "false");
        entry.SetValue("id", artistItem.Id);
        addVideos(Youtube2MP.GetList(entry), true);
        UpdateGui();
        MessageGUI.Item = null;
      }
      else
      {
        if (NavigationStack.Count == 0)
        {
          ClearLabels("Curent");
          ClearLabels("NowPlaying");
          GUIPropertyManager.SetProperty("#header.title", _setting.PluginName);
          StartUpHome();
          UpdateGui();
          }
          else
          {
          DoBack();
          GUIControl.FocusControl(GetID, listControl.GetID);
        }
      }
      GUIControl.FocusControl(GetID, listControl.GetID);
      OnDownloadTimedEvent(null, null);
    }

      private string GetRegionOpt()
      {
          if (_setting.Region == "All")
              return "";
          if (_setting.Region == "Ask")
          {
              GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
              if (dlg == null) return "";
              dlg.Reset();
              //dlg.SetHeading(25653); // Sort options
              foreach (KeyValuePair<string, string> valuePair in Youtube2MP._settings.Regions)
              {
                  dlg.Add(valuePair.Key);
              }
              dlg.DoModal(GetID);
              if (dlg.SelectedId == -1) return "";
              return Youtube2MP._settings.Regions[dlg.SelectedLabelText];
          }
          return Youtube2MP._settings.Regions[_setting.Region];
      }

      private void InitList(string queryuri)
      {
          if (_setting.MusicFilter && queryuri != YouTubeQuery.CreateFavoritesUri(null))
              queryuri += "_Music";
          string reg = GetRegionOpt();
          if (!string.IsNullOrEmpty(reg))
              queryuri = queryuri.Replace("standardfeeds", "standardfeeds/" + reg);
          YouTubeQuery query = new YouTubeQuery(queryuri);

          query.NumberToRetrieve = 50;
          query.SafeSearch = YouTubeQuery.SafeSearchValues.None;
          if (uploadtime != YouTubeQuery.UploadTime.AllTime)
              query.Time = uploadtime;

          YouTubeFeed vidr = service.Query(query);

          if (vidr.Entries.Count > 0)
          {
              SaveListState(true);
              addVideos(vidr, false, query);
              GUIPropertyManager.SetProperty("#header.title", vidr.Title.Text);
              UpdateGui();
          }
          else
          {
              Err_message("No item was found !");
          }
      }

      // remeber the selection on page leave
    protected override void OnPageDestroy(int new_windowId)
    {
      SaveListState(false);
      _setting.Save();
      base.OnPageDestroy(new_windowId);
    }
    //// do the clicked action
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      ////
      //// look for button pressed
      ////
      //// record ?
      if (actionType == Action.ActionType.ACTION_RECORD)
      {
        //ExecuteRecord();
      }
      else if (control == btnSwitchView)
      {
        switch ((View)mapSettings.ViewAs)
        {
          case View.List:
            mapSettings.ViewAs = (int)View.Icons;
            break;
          case View.Icons:
            mapSettings.ViewAs = (int)View.BigIcons;
            break;
          case View.BigIcons:
            mapSettings.ViewAs = (int)View.Albums;
            break;
          case View.Albums:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;
          case View.Filmstrip:
            mapSettings.ViewAs = (int)View.Icons;
            break;
        }
        ShowPanel();
        GUIControl.FocusControl(GetID, control.GetID);
      }
      else if (control == listControl)
      {
        // execute only for enter keys
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // station selected
          DoListSelection();
        }
      }
      else if (control == searchButton)
      {
        DoSearch();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == homeButton)
      {
        StartUpHome();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == searchHistoryButton)
      {
        DoShowHistory();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == btnPlayList)
      {
        GUIWindowManager.ActivateWindow(29051);
      }
      else if (control == btnNowPlaying)
      {
        GUIWindowManager.ActivateWindow(29052);
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void DoShowHistory()
    {
      if (_setting.SearchHistory.Count > 0)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null) return;
        dlg.Reset();
        dlg.SetHeading(Translation.SearchHistory);
        for (int i = _setting.SearchHistory.Count; i > 0; i--)
        {
          dlg.Add(_setting.SearchHistory[i-1]);
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1) return;
        SearchVideo(dlg.SelectedLabelText);
        NavigationStack.Clear();
      }
      else
      {
        Err_message(Translation.NoSearchHistory);
      }
    }

    private void StartUpHome()
    {
      if (!_setting.OldStyleHome)
      {
        //mapSettings.ViewAs = (int)View.List;
        addVideos(Youtube2MP.GetHomeMenu(), false);
        ShowPanel();
      }
      else
      {
        switch (_setting.InitialDisplay)
        {
          case 1:
            ShowHome(_setting.InitialCat);
            break;
          case 2:
            SearchVideo(_setting.InitialSearch);
            break;
          case 3:
            DoHome();
            break;
          default:
            break;
        }
        ShowPanel();
      }
    }

    private void DoHome()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      //dlg.SetHeading(25653); // Sort options
      for (int i = 0; i < _setting.OldCats.Count; i++)
      {
        dlg.Add(_setting.OldCats[i]);
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;
      int select = dlg.SelectedLabel;
      ShowHome(select);
      NavigationStack.Clear();
      uploadtime = YouTubeQuery.UploadTime.AllTime;
    }

    void GetTimeOpt()
    {
      if (_setting.Time)
      {
        GUIDialogMenu dlg1 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg1 == null) return;
        dlg1.Reset();
        for (int i = 0; i < _setting.TimeList.Count; i++)
        {
          dlg1.Add(_setting.TimeList[i]);
        }

        dlg1.DoModal(GetID);
        if (dlg1.SelectedId != -1)
        {
          uploadtime = (YouTubeQuery.UploadTime)dlg1.SelectedLabel + 1;
        }
      }    
    }

    private void ShowHome(int poz)
    {
      switch (poz)
      {
        case 0:
          GetTimeOpt();
          InitList(YouTubeQuery.MostViewedVideo);
          UpdateGui();
          break;
        case 1:
          GetTimeOpt();
          InitList(YouTubeQuery.TopRatedVideo);
          UpdateGui();
          break;
        case 2:
          InitList(YouTubeQuery.RecentlyFeaturedVideo);
          UpdateGui();
          break;
        case 3:
          GetTimeOpt();
          InitList(YouTubeQuery.MostDiscussedVideo);
          UpdateGui();
          break;
        case 4:
          GetTimeOpt();
          InitList(YouTubeQuery.FavoritesVideo);
          UpdateGui();
          break;
        case 5:
          InitList(YouTubeQuery.MostLinkedVideo);
          UpdateGui();
          break;
        case 6:
          InitList(YouTubeQuery.MostRespondedVideo);
          UpdateGui();
          break;
        case 7:
          InitList(YouTubeQuery.MostRecentVideo);
          UpdateGui();
          break;
        case 8:
          InitList(YouTubeQuery.CreateFavoritesUri(null));
          UpdateGui();
          break;
        case 9:
          if (Youtube2MP._settings.LocalFile.Items.Count == 0)
          {
            Err_message("No downloaded item was found !");
          }
          else
          {
            SaveListState(true);
            foreach (LocalFileStruct entry in Youtube2MP._settings.LocalFile.Items)
            {
              GUIListItem item = new GUIListItem();
              // and add station name & bitrate
              item.Label = entry.Title;
              item.Label2 = "";
              item.IsFolder = false;

              string imageFile = Path.GetDirectoryName(entry.LocalFile) + "\\" + Path.GetFileNameWithoutExtension(entry.LocalFile) + ".png";
              if (File.Exists(imageFile))
              {
                item.ThumbnailImage = imageFile;
                //item.IconImage = "defaultVideoBig.png";
                item.IconImage = imageFile;
                item.IconImageBig = imageFile;
              }
              item.MusicTag = entry;
              item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              listControl.Add(item);
            }
            GUIPropertyManager.SetProperty("#header.title", Youtube2MP._settings.OldCats[9]);
            listControl.SelectedListItemIndex = 0;
            UpdateGui();
          }
          break;
      }
    }

    //// override action responses
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listControl.Focus)
        {
          if (NavigationStack.Count > 0)
          {
            DoBack();
            return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = listControl[0];
        
        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          DoBack();
          return;
        }
      }
      UpdateGui();
      base.OnAction(action);
    }
    // do regulary updates
    public override void Process()
    {
      // update the gui
      UpdateGui();
      base.Process();
    }

    protected void OnShowSortOptions()
    {
    }
 
    #endregion
    #region helper func's

    private void DoListSelection()
    {
      GUIListItem selectedItem = listControl.SelectedListItem;
      YouTubeEntry vide;

      if (selectedItem != null)
      {
        if (selectedItem.Label != "..")
        {
            YouTubeQuery qu = selectedItem.MusicTag as YouTubeQuery;
            if (qu != null)
            {
                YouTubeFeed vidr = service.Query(qu);
                Log.Debug("Next page: {0}", qu.Uri.ToString());
                if (vidr.Entries.Count > 0)
                {
                    SaveListState(true);
                    addVideos(vidr, false, qu);
                    UpdateGui();
                }
            }
          //--------------------


          LocalFileStruct file = selectedItem.MusicTag as LocalFileStruct;

          if (file != null)
          {
            Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + file.VideoId);
            Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
            vide = video.YouTubeEntry;
          }
          else
          {
            vide = selectedItem.MusicTag as YouTubeEntry;
          }

          SiteItemEntry entry = selectedItem.MusicTag as SiteItemEntry;
          if (entry != null)
          {
            GenericListItemCollections genericListItem = Youtube2MP.GetList(entry);
            if (entry.Provider == "VideoItem" && genericListItem.Items.Count > 0)
            {
              vide = genericListItem.Items[0].Tag as YouTubeEntry;
              DoPlay(vide, true, null);
              return;
            }
            else
            {
              addVideos(genericListItem, true);
              UpdateGui();
            }
          }

          if (vide != null)
          {
            GUIWaitCursor.Init();
            GUIWaitCursor.Show();
            DoPlay(vide, true, listControl.ListLayout);
          }
        }
        else
        {
          DoBack();
        }
      }
     // GUIWaitCursor.Hide();
      //throw new Exception("The method or operation is not implemented.");
    }

    private void DoSearch()
    {
      string searchString = "";

      if (_setting.SearchHistory.Count > 0)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null) return;
        dlg.Reset();
        dlg.SetHeading(Translation.SearchHistory);
        dlg.Add(Translation.NewSearch);
        for (int i = _setting.SearchHistory.Count; i > 0; i--)
        {
          dlg.Add(_setting.SearchHistory[i - 1]);
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1) return;
        searchString = dlg.SelectedLabelText;
        if (searchString == Translation.NewSearch)
          searchString = "";
      }

      
      VirtualKeyboard keyboard=(VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);

      if (null == keyboard) return;

      keyboard.Reset();
      keyboard.Text = searchString;
      keyboard.DoModal(GetWindowId());
      if (keyboard.IsConfirmed)
      {
        // input confirmed -- execute the search
        searchString = keyboard.Text;
      }

      if ("" != searchString)
      {
          SearchVideo(searchString);
          NavigationStack.Clear();
      }
    }

    private void SearchVideo(string searchString)
    {
        YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
        query = SetParamToYouTubeQuery(query, false);
        //query.VQ = searchString;
        query.Query = searchString;
        query.OrderBy = "relevance";
        
        YouTubeFeed vidr = service.Query(query);

        foreach (AtomLink link in vidr.Links)
        {
            if (link.Rel == "http://schemas.google.com/g/2006#spellcorrection")
            {
                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
                if (null == dlgYesNo)
                    return;
                dlgYesNo.SetHeading(Translation.DidYouMean); //resume movie?
                dlgYesNo.SetLine(1, link.Title);
                dlgYesNo.SetDefaultToYes(true);
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                if (dlgYesNo.IsConfirmed)
                {
                    SearchVideo(link.Title);
                    return;
                }
            }
        }

        if (vidr.Entries.Count > 0)
        {
            SaveListState(true);
            addVideos(vidr, false, query);
            UpdateGui();
            if (_setting.SearchHistory.Contains(searchString.Trim()))
                _setting.SearchHistory.Remove(searchString.Trim());
            _setting.SearchHistory.Add(searchString.Trim());
          _setting.Save();
        }
        else
        {
            Err_message(Translation.NoItemWasFound);
        }
    }

    private void DoBack()
    {
      if (NavigationStack.Count > 0)
      {
        GUIControl.ClearControl(GetID, listControl.GetID);
        NavigationObject obj = NavigationStack.Pop() as NavigationObject;
        obj.SetItems(listControl);
        listControl.SelectedListItemIndex = obj.Position;
        GUIPropertyManager.SetProperty("#header.title", obj.Title);
        mapSettings.ViewAs = (int)obj.CurrentView;
        ShowPanel();
      }
    }
    
    //public void UpdateList()
    //{
   
    //}

    void ShowPanel()
    {
      int itemIndex = listControl.SelectedListItemIndex;
      if (mapSettings.ViewAs == (int)View.BigIcons)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Albums)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.AlbumView;
      }
      else if (mapSettings.ViewAs == (int)View.Icons)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.SmallIcons;
      }
      else if (mapSettings.ViewAs == (int)View.List)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.List;
      }
      else if (mapSettings.ViewAs == (int)View.Filmstrip)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
      }
      else if (mapSettings.ViewAs == (int)View.PlayList)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.Playlist;
      }

      if (itemIndex > -1)
      {
        GUIControl.SelectItemControl(GetID, listControl.GetID, itemIndex);
      }
     
    }

    /// <summary>
    /// Called when [show context menu].
    /// </summary>
    protected override void OnShowContextMenu()
    {
      GUIListItem selectedItem = listControl.SelectedListItem;
      string artistName = string.Empty;

      YouTubeEntry videoEntry;
      LocalFileStruct file = selectedItem.MusicTag as LocalFileStruct;
      if (file != null)
      {
        Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + file.VideoId);
        Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
        videoEntry = video.YouTubeEntry;
      }
      else
      {
        videoEntry = selectedItem.MusicTag as YouTubeEntry;
        if(videoEntry == null)
        {
          SiteItemEntry entry = selectedItem.MusicTag as SiteItemEntry;
          if (entry != null)
          {
            GenericListItemCollections genericListItem = Youtube2MP.GetList(entry);
            if (entry.Provider == "VideoItem" && genericListItem.Items.Count > 0)
            {
              videoEntry = genericListItem.Items[0].Tag as YouTubeEntry;
            }
          }
        }
        if (videoEntry == null)
          return;
        Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" +Youtube2MP.GetVideoId(videoEntry));
        Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
        videoEntry = video.YouTubeEntry;
      }

      if (videoEntry == null)
        return;

      if (videoEntry.Title.Text.Contains("-"))
        artistName = videoEntry.Title.Text.Split('-')[0].Trim();

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(Translation.ContextMenu); // menu
      dlg.Add(Translation.RelatedVideos);
      dlg.Add(Translation.VideoResponses);
      dlg.Add(string.Format(Translation.AllVideosFromUser,videoEntry.Authors[0].Name));
      dlg.Add(Translation.AddPlaylist);
      dlg.Add(Translation.AddAllPlaylist);
      dlg.Add(Translation.AddFavorites);
      dlg.Add(Translation.Options);
      dlg.Add(Translation.DownloadVideo);
      if (!string.IsNullOrEmpty(artistName) && !string.IsNullOrEmpty(ArtistManager.Instance.GetArtistsByName(artistName).Name))
        dlg.Add(string.Format(Translation.AllMusicVideosFrom , artistName));

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      if(dlg.SelectedLabelText==Translation.RelatedVideos)
      {
        if (videoEntry.RelatedVideosUri != null)
        {
          YouTubeQuery query = new YouTubeQuery(videoEntry.RelatedVideosUri.Content);
          YouTubeFeed vidr = service.Query(query);
          if (vidr.Entries.Count > 0)
          {
            SaveListState(true);
            addVideos(vidr, false, query);
            UpdateGui();
          }
          else
          {
            Err_message(Translation.NoItemWasFound);
          }
        }
      }
      else if (dlg.SelectedLabelText == Translation.VideoResponses)
      {
        if (videoEntry.VideoResponsesUri != null)
        {
          YouTubeQuery query = new YouTubeQuery(videoEntry.VideoResponsesUri.Content);
          YouTubeFeed vidr = service.Query(query);
          if (vidr.Entries.Count > 0)
          {
            SaveListState(true);
            addVideos(vidr, false, query);
            UpdateGui();
          }
          else
          {
            Err_message(Translation.NoVideoResponse);
          }
        }
 
      }
      else if (dlg.SelectedLabelText == string.Format(Translation.AllVideosFromUser, videoEntry.Authors[0].Name))
      {
        YouTubeQuery query = new YouTubeQuery(string.Format("http://gdata.youtube.com/feeds/api/users/{0}/uploads", videoEntry.Authors[0].Name));
        YouTubeFeed vidr = service.Query(query);
        if (vidr.Entries.Count > 0)
        {
          SaveListState(true);
          addVideos(vidr, false, query);
          UpdateGui();
        }
        else
        {
          Err_message(Translation.NoItemWasFound);
        }
      }
      else if (dlg.SelectedLabelText == Translation.AddPlaylist)
      {
        VideoInfo inf = SelectQuality(videoEntry);
        if (inf.Quality != VideoQuality.Unknow)
        {
          AddItemToPlayList(selectedItem, inf);
        }        
      }
      else if (dlg.SelectedLabelText == Translation.AddAllPlaylist)
      {

        VideoInfo inf = SelectQuality(videoEntry);
        inf.Items = new Dictionary<string, string>();
        if (inf.Quality != VideoQuality.Unknow)
        {
          GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
          if (dlgProgress != null)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(Translation.AddAllPlaylist);
            dlgProgress.SetLine(1, "");
            dlgProgress.SetLine(2, "");
            dlgProgress.SetPercentage(0);
            dlgProgress.Progress();
            dlgProgress.ShowProgressBar(true);
            dlgProgress.StartModal(GetID);
          }
          int i = 0;
          for (int j = 0; j < listControl.Count; j++)
          {
            GUIListItem item = listControl[j];
            if (dlgProgress != null)
            {
              double pr = ((double)i / (double)listControl.Count) * 100;
              dlgProgress.SetLine(1, item.Label);
              dlgProgress.SetPercentage((int)pr);
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled)
                break;
            }
            i++;
            AddItemToPlayList(item, new VideoInfo(inf));
          }
          if (dlgProgress != null)
            dlgProgress.Close();
        }
      }
      else if (dlg.SelectedLabelText == Translation.AddFavorites)
      {
        try
        {
          service.Insert(new Uri(YouTubeQuery.CreateFavoritesUri(null)), videoEntry);
        }
        catch (Exception)
        {
          Err_message(Translation.WrongRequestWrongUser);
        }
      }
      else if (dlg.SelectedLabelText == Translation.Options)
      {
        DoOptions();
      }
      else if (dlg.SelectedLabelText == Translation.AddFavorites)
      {
        if (Youtube2MP._settings.LocalFile.Get(Youtube2MP.GetVideoId(videoEntry)) != null)
        {
          Err_message(Translation.ItemAlreadyDownloaded);
        }
        else
        {
          if (VideoDownloader.IsBusy)
          {
            Err_message(Translation.AnotherDonwnloadProgress);
            dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
            if (dlgProgress != null)
            {
              dlgProgress.Reset();
              dlgProgress.SetHeading(Translation.DownloadProgress);
              dlgProgress.SetLine(1, "");
              dlgProgress.SetLine(2, "");
              dlgProgress.SetPercentage(0);
              dlgProgress.Progress();
              dlgProgress.ShowProgressBar(true);
              dlgProgress.DoModal(GetID);
            }
          }
          else
          {
            VideoInfo inf = SelectQuality(videoEntry);
            string streamurl = Youtube2MP.StreamPlaybackUrl(videoEntry, inf);
            VideoDownloader.AsyncDownload(streamurl,
                                          Youtube2MP._settings.DownloadFolder + "\\" +
                                          Utils.GetFilename(videoEntry.Title.Text + "{" + Youtube2MP.GetVideoId(videoEntry) + "}") +
                                          Path.GetExtension(streamurl));
            VideoDownloader.Entry = videoEntry;
          }
        }
      }
      else if (dlg.SelectedLabelText == string.Format(Translation.AllMusicVideosFrom, artistName))
      {
        ArtistItem artistItem = ArtistManager.Instance.GetArtistsByName(artistName);
        addVideos(ArtistManager.Instance.Grabber.GetArtistVideosIds(artistItem.Id), true);
      }
    }


    private void DoOptions()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      do
      {
        dlg.Reset();
        dlg.SetHeading(498); // menu
        dlg.Add(string.Format("NowPlaying in place of Fullscreen: {0}", _setting.ShowNowPlaying));
        dlg.Add(string.Format("Ask for time period: {0}", _setting.Time));
        dlg.Add(string.Format("Enable music videos filtering: {0}", _setting.MusicFilter));
        dlg.Add(string.Format("Use extrem filter music videos: {0}", _setting.UseExtremFilter));
        dlg.Add(string.Format("Use SMS style keyboard: {0}", _setting.UseSMSStyleKeyBoard));
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
          return;
        switch (dlg.SelectedLabel)
        {
          case 0:
            _setting.ShowNowPlaying = !_setting.ShowNowPlaying;
            break;
          case 1:
            _setting.Time = !_setting.Time;
            break;
          case 2:
            _setting.MusicFilter = !_setting.MusicFilter;
            break;
          case 3:
            _setting.UseExtremFilter = !_setting.UseExtremFilter;
            break;
          case 4:
            _setting.UseSMSStyleKeyBoard = !_setting.UseSMSStyleKeyBoard;
            break;
        }

      } while (true);
    }

    public void UpdateGui()
    {

      string textLine = string.Empty;
      View view = (View)mapSettings.ViewAs;
      bool sortAsc = mapSettings.SortAscending;
      switch (view)
      {
        case View.List:
          textLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          textLine = GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          textLine = GUILocalizeStrings.Get(529);
          break;
        case View.Filmstrip:
          textLine = GUILocalizeStrings.Get(733);
          break;
        case View.PlayList:
          textLine = GUILocalizeStrings.Get(101);
          break;

      }
      GUIControl.SetControlLabel(GetID, btnSwitchView.GetID, textLine);

    }

    void SortChanged(object sender, SortEventArgs e)
    {
      // save the new state
      mapSettings.SortAscending = e.Order != SortOrder.Descending;
      // update the list
      //UpdateList();
      //UpdateButtonStates();
      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
    }

    #endregion

    void addVideos(GenericListItemCollections itemCollections, bool level)
    {
      if (itemCollections.Items.Count < 1)
      {
        Err_message(Translation.NoItemToDisplay);
        return;
      }

      SaveListState(true);

      if (itemCollections.FolderType == 1)
        mapSettings.ViewAs = (int)View.Albums;
      else
        mapSettings.ViewAs = (int) View.List;


      GUIPropertyManager.SetProperty("#header.title", itemCollections.Title);
      updateStationLogoTimer.Enabled = false;
      downloaQueue.Clear();
      if (level)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        Utils.SetDefaultIcons(item);
        listControl.Add(item);
      }


      foreach (GenericListItem listItem in itemCollections.Items)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = listItem.Title;
        item.Label2 = listItem.Title2;
        item.IsFolder = listItem.IsFolder;
        item.Duration = listItem.Duration;
        item.Rating = listItem.Rating;
        Utils.SetDefaultIcons(item);

        if (!string.IsNullOrEmpty(listItem.LogoUrl))
        {
          string imageFile = GetLocalImageFileName(listItem.LogoUrl);
          if (File.Exists(imageFile))
          {
            item.ThumbnailImage = imageFile;
            item.IconImage = imageFile;
            item.IconImageBig = imageFile;
          }
          else
          {
            Utils.SetDefaultIcons(item);
            DownloadImage(listItem.LogoUrl, item);
          }
        }
        item.MusicTag = listItem.Tag;
        item.OnItemSelected += item_OnItemSelected;
        listControl.Add(item);
      }

      listControl.SelectedListItemIndex = 0;

      UpdateGui();
      ShowPanel();
      OnDownloadTimedEvent(null, null);
    }

    void addVideos(YouTubeFeed videos, bool level,YouTubeQuery qu)
    {
      mapSettings.ViewAs = (int)View.Albums;
      int count = 0;
      if (level)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        listControl.Add(item);
      }
      GUIPropertyManager.SetProperty("#header.title", videos.Title.Text);
      updateStationLogoTimer.Enabled = false;
      downloaQueue.Clear();
      foreach (YouTubeEntry entry in videos.Entries)
      {
        if (filterVideoContens(entry))
        {
          GUIListItem item = new GUIListItem();
          // and add station name & bitrate
          item.Label = entry.Title.Text; 
          item.IsFolder = false;
          count++;
          try
          {
            if (entry.Duration != null)
            item.Duration = Convert.ToInt32(entry.Duration.Seconds, 10);
            if (entry.Rating != null)
              item.Rating = (float) entry.Rating.Average*2;
          }
          catch
          {

          }

          string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
          if (File.Exists(imageFile))
          {
            item.ThumbnailImage = imageFile;
            //item.IconImage = "defaultVideoBig.png";
            item.IconImage = imageFile;
            item.IconImageBig = imageFile;
          }
          else
          {
            Utils.SetDefaultIcons(item);
            DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
          }
          item.MusicTag = entry;
          item.OnItemSelected+=item_OnItemSelected;
          listControl.Add(item);
        } 
      }
      if (qu.NumberToRetrieve > 0 && videos.TotalResults > qu.NumberToRetrieve)
      {
        GUIListItem item = new GUIListItem();
        item.Label = string.Format(Translation.NextPage+" {0} - {1} ", qu.StartIndex + count, qu.StartIndex + qu.NumberToRetrieve + count);
        qu.StartIndex += qu.NumberToRetrieve;
        item.Label = "";
        item.Label2 = Translation.NextPage;
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        item.MusicTag = qu;
        listControl.Add(item);
      }
      listControl.SelectedListItemIndex = 0;
      UpdateGui();
      ShowPanel();
      OnDownloadTimedEvent(null, null);
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      YouTubeEntry vid = item.MusicTag as YouTubeEntry ;
      if (vid != null)
      {
        SetLabels(vid, "Curent");
      }
      else
      {
        ClearLabels("Curent"); ;
      }
    }

    private void SaveListState(bool clear)
    {
      if (listControl.ListLayout.ListItems.Count > 0)
      {
        NavigationStack.Push(new NavigationObject(listControl.ListLayout, GUIPropertyManager.GetProperty("#header.title"), listControl.SelectedListItemIndex, (View)mapSettings.ViewAs));
      }
      if (clear)
      {
        ClearLabels("Curent");
        GUIControl.ClearControl(GetID, listControl.GetID);
        Youtube2MP.temp_player.Reset();
        Youtube2MP.temp_player.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO).Clear();
      }
    }
    

  }
}
