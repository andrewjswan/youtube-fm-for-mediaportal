using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.Class.Database;
using Action = MediaPortal.GUI.Library.Action;


namespace YouTubePlugin
{
  public enum View
  {
    List = 0,
    Icons = 1,
    BigIcons = 2,
    Albums = 3,
    Filmstrip = 4,
    CoverFlow = 5
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
    private ItemType _lastItemType = ItemType.Item;

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
      _setting.CreateFolders();
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
      DatabaseProvider.InstanInstance.InitDatabase();
      
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
      GUIPropertyManager.SetProperty("#Youtube.fm.Download.Progress", e.PercentDone.ToString());
    }

    void VideoDownloader_DownloadComplete(object sender, EventArgs e)
    {
      string video_file = VideoDownloader.DownloadingTo.Replace(".___", "");
      GUIDialogNotify dlg1 = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dlg1 != null)
      {
        dlg1.Reset();
        dlg1.SetHeading("Download done");
        dlg1.SetText(video_file);
        dlg1.Reset();
        dlg1.TimeOut = 5;
        dlg1.DoModal(GetID);
      }
      Youtube2MP._settings.LocalFile.Items.Add(new LocalFileStruct(video_file,Youtube2MP.GetVideoId(VideoDownloader.Entry), VideoDownloader.Entry.Title.Text));
      Youtube2MP._settings.LocalFile.Save();
      string imageFile = GetLocalImageFileName(GetBestUrl(VideoDownloader.Entry.Media.Thumbnails));
      try
      {
        File.Move(VideoDownloader.DownloadingTo, video_file);
        if (File.Exists(imageFile))
        {
          File.Copy(imageFile,
                    Path.GetDirectoryName(video_file) + "\\" + Path.GetFileNameWithoutExtension(video_file) + ".png");
        }
      }
      catch
      {
      }
      GUIPropertyManager.SetProperty("#Youtube.fm.IsDownloading", "false");
      GUIPropertyManager.SetProperty("#Youtube.fm.Download.Progress", "0");
      GUIPropertyManager.SetProperty("#Youtube.fm.Download.Item", " ");

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
      Youtube2MP.request.Service = service;
      if (!string.IsNullOrEmpty(_setting.User))
      {
        try
        {
          service.setUserCredentials(_setting.User, _setting.Password);
          string feedUrl = "http://gdata.youtube.com/feeds/api/users/default";
          try
          {
            var profileEntry = (ProfileEntry)service.Get(feedUrl);
          }
          catch (InvalidCredentialsException)
          {
            Log.Warn("Wrong youtube username or password or account is disabled ");
            service.Credentials = null;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          service.Credentials = null;
        }
      }
       
      if (Youtube2MP.LastFmProfile == null)
      {
        Youtube2MP.LastFmProfile = new LastProfile();
        try
        {
          Youtube2MP.LastFmProfile.Login(_setting.LastFmUser, _setting.LastFmPass);
        }
        catch (Exception exception)
        {
          Log.Warn("Wrong last.fm username or password or account is disabled ");
        }
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

      UpdateGui();
      ShowPanel();

      GUIControl.FocusControl(GetID, listControl.GetID);
      GUIPropertyManager.SetProperty("#nowplaying", " ");
      if (MessageGUI.Item != null)
      {
        ArtistItem artistItem = MessageGUI.Item as ArtistItem;
        SiteItemEntry entry = new SiteItemEntry();
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
          GUIPropertyManager.SetProperty("#Youtube.fm.IsDownloading", "false");
          GUIPropertyManager.SetProperty("#Youtube.fm.Download.Progress", "0");
          GUIPropertyManager.SetProperty("#Youtube.fm.Download.Item", " ");

          GUIPropertyManager.SetProperty("#header.title", " ");
          GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/Home");
          StartUpHome();
          UpdateGui();
          switch (_setting.StartUpOpt)
          {
            case 1:
              {
                DoSearch();
              }
              break;
            case 2:
              {
                for (int i = 0; i < listControl.Count; i++)
                {
                  SiteItemEntry item = listControl[i].MusicTag as SiteItemEntry;
                  if (item != null && item.Provider == "Disco")
                  {
                    listControl.SelectedItem = i;
                    listControl.SelectedListItemIndex = i;
                    DoListSelection();
                    break;
                  }
                }
              }
              break;
            case 3:
              {
                for (int i = 0; i < listControl.Count; i++)
                {
                  SiteItemEntry item = listControl[i].MusicTag as SiteItemEntry;
                  if (item != null && item.Provider == "Browse")
                  {
                    listControl.SelectedItem = i;
                    listControl.SelectedListItemIndex = i;
                    DoListSelection();
                    break;
                  }
                }
              }
              break;
          }
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
    if (control == btnSwitchView)
      {
        //switch ((View)mapSettings.ViewAs)
        //{
        //  case View.List:
        //    mapSettings.ViewAs = (int)View.Icons;
        //    break;
        //  case View.Icons:
        //    mapSettings.ViewAs = (int)View.BigIcons;
        //    break;
        //  case View.BigIcons:
        //    mapSettings.ViewAs = (int)View.Albums;
        //    break;
        //  case View.Albums:
        //    mapSettings.ViewAs = (int)View.Filmstrip;
        //    break;
        //  case View.Filmstrip:
        //    mapSettings.ViewAs = (int)View.CoverFlow;
        //    break;
        //  case View.CoverFlow:
        //    mapSettings.ViewAs = (int)View.List;
        //    break;
        //}
        OnShowLayouts();
        GetLayout(_lastItemType);
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
        //NavigationStack.Clear();
      }
      else
      {
        Err_message(Translation.NoSearchHistory);
      }
    }

    private void StartUpHome()
    {
      addVideos(Youtube2MP.GetHomeMenu(), false);
      ShowPanel();
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

 
    #endregion
    #region helper func's

    protected virtual GUIFacadeControl.Layout GetLayoutNumber(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "list":
          return GUIFacadeControl.Layout.List;
        case "icons":
        case "smallicons":
          return GUIFacadeControl.Layout.SmallIcons;
        case "big icons":
        case "largeicons":
          return GUIFacadeControl.Layout.LargeIcons;
        case "albums":
        case "albumview":
          return GUIFacadeControl.Layout.AlbumView;
        case "filmstrip":
          return GUIFacadeControl.Layout.Filmstrip;
        case "playlist":
          return GUIFacadeControl.Layout.Playlist;
        case "coverflow":
        case "cover flow":
          return GUIFacadeControl.Layout.CoverFlow;
        default:
          if (!string.IsNullOrEmpty(s))
            Log.Error("{0}::GetLayoutNumber: Unknown String - {1}", new object[2]
            {
              (object) "WindowPluginBase",
              (object) s
            });
          return GUIFacadeControl.Layout.List;
      }
    }

    protected virtual void OnShowLayouts()
    {
      GUIDialogMenu guiDialogMenu1 = (GUIDialogMenu)GUIWindowManager.GetWindow(2012);
      if (guiDialogMenu1 == null)
        return;
      guiDialogMenu1.Reset();
      guiDialogMenu1.SetHeading(792);
      guiDialogMenu1.Add(GUILocalizeStrings.Get(101));
      guiDialogMenu1.Add(GUILocalizeStrings.Get(100));
      guiDialogMenu1.Add(GUILocalizeStrings.Get(417));
      guiDialogMenu1.Add(GUILocalizeStrings.Get(529));
      guiDialogMenu1.Add(GUILocalizeStrings.Get(733));
      guiDialogMenu1.Add(GUILocalizeStrings.Get(791));

      guiDialogMenu1.SelectedLabel = mapSettings.ViewAs;
      guiDialogMenu1.DoModal(this.GetID);
      if (guiDialogMenu1.SelectedId == -1)
        return;
      mapSettings.ViewAs = guiDialogMenu1.SelectedId - 1;
    }

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
              if (entry.Provider == "Disco" & !g_Player.Playing)
              {
                listControl.SelectedListItemIndex = 1;
                GUIWaitCursor.Show();
                DoPlay(genericListItem.Items[0].Tag as YouTubeEntry, true, listControl.ListLayout);
              }
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
      }
    }

    private void SearchVideo(string searchString)
    {
        YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
        query = SetParamToYouTubeQuery(query, false);
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
          vidr.Title.Text = "Search/" + searchString;
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

    private void SetLayout(ItemType itemType)
    {
      switch (itemType)
      {
        case ItemType.Item:
          mapSettings.ViewAs = _setting.LayoutItem;
          break;
        case ItemType.Artist:
          mapSettings.ViewAs = _setting.LayoutArtist;
          break;
        case ItemType.Video:
          mapSettings.ViewAs = _setting.LayoutVideo;
          break;
      }
    }

    private void GetLayout(ItemType itemType)
    {
      switch (itemType)
      {
        case ItemType.Item:
          _setting.LayoutItem = mapSettings.ViewAs;
          break;
        case ItemType.Artist:
          _setting.LayoutArtist = mapSettings.ViewAs;
          break;
        case ItemType.Video:
          _setting.LayoutVideo = mapSettings.ViewAs;
          break;
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
        GUIPropertyManager.SetProperty("#currentmodule", obj.Title);
        GUIPropertyManager.SetProperty("#itemtype", obj.ItemType);
        GUIPropertyManager.SetProperty("#itemcount", listControl.Count.ToString());
        mapSettings.ViewAs = (int)obj.CurrentView;
        ShowPanel();
      }
    }
    
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
      else if (mapSettings.ViewAs == (int)View.CoverFlow)
      {
        listControl.CurrentLayout = GUIFacadeControl.Layout.CoverFlow;
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
      SiteItemEntry entry = selectedItem.MusicTag as SiteItemEntry;
      if (file != null)
      {
        Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + file.VideoId);
        Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
        videoEntry = video.YouTubeEntry;
      }
      else
      {
        videoEntry = selectedItem.MusicTag as YouTubeEntry;
        if (videoEntry == null)
        {
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
        Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + Youtube2MP.GetVideoId(videoEntry));
        Video video = Youtube2MP.request.Retrieve<Video>(videoEntryUrl);
        videoEntry = video.YouTubeEntry;
      }

      if (videoEntry == null)
        return;

      artistName = GetArtistName(videoEntry);

      ArtistItem artistItem = GetArtist(videoEntry);

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(Translation.ContextMenu); // menu
      if (Youtube2MP.player.CurrentSong > -1 || Youtube2MP.temp_player.CurrentSong > -1)
        dlg.Add(Translation.PlayNext);
      dlg.Add(Translation.Info);
      dlg.Add(Translation.RelatedVideos);
      dlg.Add(Translation.VideoResponses);
      dlg.Add(string.Format(Translation.AllVideosFromUser, videoEntry.Authors[0].Name));
      dlg.Add(Translation.AddPlaylist);
      dlg.Add(Translation.AddAllPlaylist);
      if (Youtube2MP.service.Credentials != null)
      {
        dlg.Add(Translation.AddFavorites);
        dlg.Add(Translation.AddWatchLater);
      }
      dlg.Add(Translation.Options);
      dlg.Add(Translation.DownloadVideo);
      if (!string.IsNullOrEmpty(artistName) &&
          !string.IsNullOrEmpty(ArtistManager.Instance.GetArtistsByName(artistName).Name))
        dlg.Add(string.Format(Translation.AllMusicVideosFrom, artistName));
      if (!string.IsNullOrEmpty(artistItem.Id))
        dlg.Add(Translation.SimilarArtists);

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      if (dlg.SelectedLabelText == Translation.RelatedVideos)
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
        YouTubeQuery query =
          new YouTubeQuery(string.Format("http://gdata.youtube.com/feeds/api/users/{0}/uploads",
                                         videoEntry.Authors[0].Name));
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
          GUIDialogProgress dlgProgress =
            (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
              double pr = ((double) i/(double) listControl.Count)*100;
              dlgProgress.SetLine(1, item.Label);
              dlgProgress.SetLine(2, i.ToString() + "/" + listControl.Count.ToString());
              dlgProgress.SetPercentage((int) pr);
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
      else if (dlg.SelectedLabelText == Translation.AddWatchLater)
      {
        PlayListMember pm = new PlayListMember();
        pm.Id = videoEntry.VideoId;
        Youtube2MP.request.Insert(new Uri("https://gdata.youtube.com/feeds/api/users/default/watch_later"), pm);
      }
      else if (dlg.SelectedLabelText == Translation.Options)
      {
        DoOptions();
      }
      else if (dlg.SelectedLabelText == Translation.DownloadVideo)
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
            dlgProgress = (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
                                          Utils.MakeFileName(Utils.GetFilename(videoEntry.Title.Text + "{" +
                                                                               Youtube2MP.GetVideoId(videoEntry) + "}")) +
                                          Path.GetExtension(streamurl) + ".___");
            GUIPropertyManager.SetProperty("#Youtube.fm.IsDownloading", "true");
            GUIPropertyManager.SetProperty("#Youtube.fm.Download.Progress", "0");
            GUIPropertyManager.SetProperty("#Youtube.fm.Download.Item", videoEntry.Title.Text);

            VideoDownloader.Entry = videoEntry;
          }
        }
      }
      else if (dlg.SelectedLabelText == string.Format(Translation.AllMusicVideosFrom, artistName))
      {
        addVideos(ArtistManager.Instance.Grabber.GetArtistVideosIds(artistItem.Id), true);
      }
      else if (dlg.SelectedLabelText == Translation.Info)
      {
        YoutubeGuiInfoEx scr = (YoutubeGuiInfoEx) GUIWindowManager.GetWindow(29053);
        scr.YouTubeEntry = videoEntry;
        //if (entry!=null)
        //{
        //  ArtistItem artistItem=ent
        //}
        GUIWindowManager.ActivateWindow(29053);
      }
      else if (dlg.SelectedLabelText == Translation.PlayNext)
      {
        PlayNext(videoEntry);
      }
      else if (dlg.SelectedLabelText == Translation.SimilarArtists)
      {
        List<ArtistItem> similarartist = new List<ArtistItem>();
        similarartist = ArtistManager.Instance.Grabber.GetSimilarArtists(artistItem.Id);
        GenericListItemCollections res = new GenericListItemCollections();
        foreach (ArtistItem item in similarartist)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = "Artists";
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", item.Id);
          newentry.SetValue("name", item.Name);
          res.ItemType = ItemType.Artist;
          try
          {
            GenericListItem listItem = new GenericListItem()
            {
              Title = item.Name,
              LogoUrl =
                string.IsNullOrEmpty(item.Img_url.Trim()) ? "@" : item.Img_url,
              IsFolder = true,
              DefaultImage = "defaultArtistBig.png",
              Tag = newentry
            };
            res.Items.Add(listItem);
          }
          catch (Exception exception)
          {
          }
        }
        res.Title = "Artists/Similar/" + artistItem.Name;
        addVideos(res, true);
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
        dlg.Add(string.Format("Enable music videos filtering: {0}", _setting.MusicFilter));
        dlg.Add(string.Format("Use extrem filter music videos: {0}", _setting.UseExtremFilter));
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
          return;
        switch (dlg.SelectedLabel)
        {
          case 0:
            _setting.ShowNowPlaying = !_setting.ShowNowPlaying;
            break;
          case 1:
            _setting.MusicFilter = !_setting.MusicFilter;
            break;
          case 2:
            _setting.UseExtremFilter = !_setting.UseExtremFilter;
            break;
        }
      } while (true);
    }

    public void UpdateGui()
    {

      string textLine = string.Empty;
      View view = (View)mapSettings.ViewAs;
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
        case View.CoverFlow:
          textLine = GUILocalizeStrings.Get(791);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSwitchView.GetID, textLine);

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

      //GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/" + itemCollections.Title);
      GUIPropertyManager.SetProperty("#currentmodule", itemCollections.Title);
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
        item.Label = listItem.Title;
        item.Label2 = listItem.Title2;
        item.Label3 = listItem.Title3;
        item.IsFolder = listItem.IsFolder;
        item.Duration = listItem.Duration;
        item.Rating = listItem.Rating;
        
        if (string.IsNullOrEmpty(listItem.DefaultImage))
        {
          string file = GUIGraphicsContext.Skin + "\\Media\\Youtube.Fm\\" + listItem.Title.Replace(":", "_") + ".png";
          if (File.Exists(file))
            listItem.DefaultImage = file;
        }

        if (string.IsNullOrEmpty(listItem.DefaultImage))
        {
          Utils.SetDefaultIcons(item);
        }
        else
        {
          item.ThumbnailImage = listItem.DefaultImage;
          item.IconImage = listItem.DefaultImage;
          item.IconImageBig = listItem.DefaultImage;
        }

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
            DownloadImage(listItem.LogoUrl, item);
          }
        }
        item.MusicTag = listItem.Tag;
        item.OnItemSelected += item_OnItemSelected;
        //YouTubeEntry tubeEntry = listItem.Tag as YouTubeEntry;
        //if (Youtube2MP.NowPlayingEntry != null && tubeEntry != null && Youtube2MP.GetVideoId(Youtube2MP.NowPlayingEntry) == Youtube2MP.GetVideoId(tubeEntry))
        //{
        //  item.Selected = true;
        //}
        listControl.Add(item);
      }

      listControl.SelectedListItemIndex = 0;
      GUIPropertyManager.SetProperty("#itemcount", (level ? listControl.Count - 1 : listControl.Count).ToString());
      GUIPropertyManager.SetProperty("#itemtype", itemCollections.ItemTypeName);
      SetLayout(itemCollections.ItemType);
      _lastItemType = itemCollections.ItemType;
      UpdateGui();
      ShowPanel();
      OnDownloadTimedEvent(null, null);
    }

    void addVideos(YouTubeFeed videos, bool level,YouTubeQuery qu)
    {
      int count = 0;
      if (level)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        Utils.SetDefaultIcons(item);
        listControl.Add(item);
      }
      //GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/" + videos.Title.Text);
      GUIPropertyManager.SetProperty("#currentmodule",  videos.Title.Text);
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
            item.Label2 = MediaPortal.Util.Utils.SecondsToHMSString(item.Duration);
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
        Utils.SetDefaultIcons(item);
        item.MusicTag = qu;
        listControl.Add(item);
      }
      GUIPropertyManager.SetProperty("#itemcount", (level ? listControl.Count - 1 : listControl.Count).ToString());
      GUIPropertyManager.SetProperty("#itemtype", Translation.Videos);
      _lastItemType = ItemType.Video;
      SetLayout(ItemType.Video);
      listControl.SelectedListItemIndex = 0;
      UpdateGui();
      ShowPanel();
      OnDownloadTimedEvent(null, null);
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item == null || parent == null)
        return;

      YouTubeEntry vid = listControl.SelectedListItem.MusicTag as YouTubeEntry;
      
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
        NavigationStack.Push(new NavigationObject(listControl.ListLayout, GUIPropertyManager.GetProperty("#currentmodule"), GUIPropertyManager.GetProperty("#itemtype"), listControl.SelectedListItemIndex, (View)mapSettings.ViewAs));
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
