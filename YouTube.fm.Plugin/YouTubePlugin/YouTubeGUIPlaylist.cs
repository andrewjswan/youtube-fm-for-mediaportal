#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Web;
using System.Threading;
using System.Text;
using System.Timers;
using System.Runtime.CompilerServices;
using Lastfm.Services;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using YouTubePlugin.Class;
using Action = MediaPortal.GUI.Library.Action;
using Playlist = Google.YouTube.Playlist;
using Timer = System.Timers.Timer;

namespace YouTubePlugin
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class YouTubeGUIPlaylist : YoutubeGUIBase
  {
    public enum ScrobbleMode
    {
      Youtube = 0,
      Neighbours = 1,
      Recent = 2,
      //Friends = 2,
      //Tags = 3,
      //Random = 5,
    }

    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      Albums = 3,
      FilmStrip = 4,
      PlayList = 5
    }




    // add the beginning artist again to avoid drifting away in style.

    //MusicDatabase mdb = null;
    DirectoryHistory m_history = new DirectoryHistory();
    string m_strDirectory = string.Empty;
    int m_iItemSelected = -1;
    int m_iLastControl = 0;
    int m_nTempPlayListWindow = 0;
    string m_strTempPlayListDirectory = string.Empty;
    string m_strCurrentFile = string.Empty;
    private bool ScrobblerOn = false;
    //private bool _enableScrobbling = false;
    protected View currentView = View.List;
    protected string _currentPlaying = string.Empty;
    public PlayListType _playlistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
    private ScrobbleMode currentScrobbleMode = ScrobbleMode.Youtube;
    //-----------------------
    BackgroundWorker scroblerBackgroundWorker = new BackgroundWorker();
    private int maxScrobbledEntry = 3;
    private string playlistname = string.Empty;
    //-----------------------


    protected delegate void ThreadRefreshList();


    [SkinControlAttribute(50)]
    protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(10)]
    protected GUIButtonControl btnSavedPlaylists = null;
    [SkinControlAttribute(20)]
    protected GUIButtonControl btnShuffle = null;
    [SkinControlAttribute(21)]
    protected GUIButtonControl btnSave = null;
    [SkinControlAttribute(22)]
    protected GUIButtonControl btnClear = null;
    [SkinControlAttribute(26)]
    protected GUIButtonControl btnNowPlaying = null;
    [SkinControlAttribute(27)]
    protected GUIToggleButtonControl btnScrobble = null;
    [SkinControlAttribute(28)]
    protected GUIButtonControl btnScrobbleMode = null;
    [SkinControlAttribute(30)]
    protected GUIToggleButtonControl btnRepeatPlaylist = null;


    public YouTubeGUIPlaylist()
    {
      GetID = 29051;
      playlistPlayer = Youtube2MP.player;
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = false;
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(OnDownloadTimedEvent);
      Client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadLogoEnd);

      //_virtualDirectory.AddDrives();
      //_virtualDirectory.SetExtensions(MediaPortal.Util.Utils.AudioExtensions);
    }



    #region overrides
    public override bool Init()
    {
      //added by Sam
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      scroblerBackgroundWorker.DoWork += scroblerBackgroundWorker_DoWork;
      return Load(GUIGraphicsContext.Skin + @"\youtubeplaylist.xml");
    }

    void scroblerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      if (!ScrobblerOn)
        return;
      switch (currentScrobbleMode)
      {
        case ScrobbleMode.Youtube:
          ScrobbleYoutube();
          break;
        case ScrobbleMode.Neighbours:
          ScrobbleNeighbours();
          break;
        case ScrobbleMode.Recent:
          ScrobbleRecent();
          break;
      }
      DoRefreshList();
    }

    bool CheckTitle(string title)
    {
      PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
      foreach (PlayListItem playListItem in playList)
      {
        if (playListItem.Description == title)
          return true;
      }
      return false;
    }

    void ScrobbleYoutube()
    {
      if (Youtube2MP.NowPlayingEntry.RelatedVideosUri != null)
      {
        YouTubeQuery query = new YouTubeQuery(Youtube2MP.NowPlayingEntry.RelatedVideosUri.Content);
        YouTubeFeed vidr = Youtube2MP.service.Query(query);
        if (vidr.Entries.Count > 0)
        {
          Random random = new Random();
          for (int i = 0; i < maxScrobbledEntry; i++)
          {
            int randomNumber = random.Next(0, vidr.Entries.Count - 1);
            PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
            if (!CheckTitle(vidr.Entries[randomNumber].Title.Text))
              AddItemToPlayList((YouTubeEntry) vidr.Entries[randomNumber], ref playList);
          }
        }
      }
    }

    void ScrobbleNeighbours()
    {
      if(!Youtube2MP.LastFmProfile.IsLoged)
        return;
      User user = new User(Youtube2MP._settings.LastFmUser, Youtube2MP.LastFmProfile.Session);
      User[] users = user.GetNeighbours();
      if(users.Length>0)
      {
        Random random = new Random();
        Random randomOther = new Random();
        Random randomOther1 = new Random();
        for (int i = 0; i < maxScrobbledEntry; i++)
        {
          int randomNumber = random.Next(0, users.Length - 1);
          TopArtist[] topArtists = users[randomNumber].GetTopArtists();
          int randomartist = randomOther.Next(0, topArtists.Length - 1);
          YouTubeEntry tubeEntry = GetArtistEntry(topArtists[randomartist].Item.ToString(), 10);
          PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
          if (tubeEntry != null && !CheckTitle(tubeEntry.Title.Text))
            AddItemToPlayList(tubeEntry, ref playList);
        }
      }
    }

    void ScrobbleRecent()
    {
      if (!Youtube2MP.LastFmProfile.IsLoged)
        return;
      User user = new User(Youtube2MP._settings.LastFmUser, Youtube2MP.LastFmProfile.Session);
      Track[] recent = user.GetRecentTracks(50);
      if (recent.Length > 0)
      {
        Random random = new Random();
        for (int i = 0; i < maxScrobbledEntry; i++)
        {
          int randomNumber = random.Next(0, recent.Length - 1);
          YouTubeEntry tubeEntry = GetEntry(recent[randomNumber].ToString());
          PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
          if (tubeEntry != null && !CheckTitle(tubeEntry.Title.Text))
            AddItemToPlayList(tubeEntry, ref playList);
        }
      }
    }

    YouTubeEntry GetEntry(string title)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      query.Query = title;
      query.NumberToRetrieve = 1;
      query.OrderBy = "relevance";

      if (Youtube2MP._settings.MusicFilter)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      YouTubeFeed videos = Youtube2MP.service.Query(query);
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        return youTubeEntry;
      }
      return null;
    }

    YouTubeEntry GetArtistEntry(string title,int length)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      query.Query = title;
      query.NumberToRetrieve = length;
      query.OrderBy = "relevance";

      if (Youtube2MP._settings.MusicFilter)
      {
        query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));
      }

      YouTubeFeed videos = Youtube2MP.service.Query(query);
      if (videos.Entries.Count == 0)
        return null;
      Random random = new Random();
      int number = random.Next(0, videos.Entries.Count-1);
      return (YouTubeEntry)videos.Entries[number];
    }


    public override void DeInit()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);
      scroblerBackgroundWorker.DoWork -= scroblerBackgroundWorker_DoWork;
      base.DeInit();
    }

    protected string SerializeName
    {
      get
      {
        return "youtubeplaylist";
      }
    }

    protected bool AllowView(View view)
    {
      if (view == View.List)
        return false;
      if (view == View.Albums)
        return false;
      if (view == View.FilmStrip)
        return false;
      return true;
    }


    public override void OnAction(Action action)
    {
      //Log.Debug("DEBUG - {0}", action.wID.ToString());
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP)
        MovePlayListItemUp();

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN)
        MovePlayListItemDown();

      else if (action.wID == Action.ActionType.ACTION_DELETE_SELECTED_ITEM)
        DeletePlayListItem();

    // Handle case where playlist has been stopped and we receive a player action.
      // This allows us to restart the playback proccess...
      else if (action.wID == Action.ActionType.ACTION_MUSIC_PLAY
       || action.wID == Action.ActionType.ACTION_NEXT_ITEM
       || action.wID == Action.ActionType.ACTION_PAUSE
       || action.wID == Action.ActionType.ACTION_PREV_ITEM
     )
      {
        if (playlistPlayer.CurrentPlaylistType != _playlistType)
        {
          playlistPlayer.CurrentPlaylistType = _playlistType;

          if (string.IsNullOrEmpty(g_Player.CurrentFile))
          {
            PlayList playList = playlistPlayer.GetPlaylist(_playlistType);

            if (playList != null && playList.Count > 0)
            {
              ClearScrobbleStartTrack();
              playlistPlayer.Play(0);
              UpdateButtonStates();
            }
          }
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", "Playlist/" + playlistname);
      _playlistType = PlayListType.PLAYLIST_MUSIC_VIDEO;

      currentView = View.PlayList;
      facadeView.CurrentLayout = GUIFacadeControl.Layout.Playlist;

      LoadDirectory(string.Empty);
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }
      if ((m_iLastControl == facadeView.GetID) && facadeView.Count <= 0)
      {
        m_iLastControl = btnNowPlaying.GetID;
        GUIControl.FocusControl(GetID, m_iLastControl);
      }
      if (facadeView.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }

      using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("youtubeplaylist", "repeat", true);
        ScrobblerOn = settings.GetValueAsBool("youtubeplaylist", "ScrobblerOn", true);
        currentScrobbleMode =(ScrobbleMode) settings.GetValueAsInt("youtubeplaylist", "ScrobblerMode", 0); 
      }

      if (btnRepeatPlaylist != null)
      {
        btnRepeatPlaylist.Selected = playlistPlayer.RepeatPlaylist;
      }
      if (ScrobblerOn)
        btnScrobble.Selected = true;

      SetScrobbleButonLabel();
      SelectCurrentPlayingSong();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iItemSelected = facadeView.SelectedListItemIndex;
      using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        settings.SetValueAsBool("youtubeplaylist", "repeat", playlistPlayer.RepeatPlaylist);
        settings.SetValueAsBool("youtubeplaylist", "ScrobblerOn", ScrobblerOn);
        settings.SetValue("youtubeplaylist", "ScrobblerMode", (int) currentScrobbleMode);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected void OnClickedBase(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (CurrentView)
          {
            case View.List:
              CurrentView = View.PlayList;
              if (!AllowView(CurrentView) || facadeView.PlayListLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.Playlist;
              break;

            case View.PlayList:
              CurrentView = View.Icons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.SmallIcons;
              break;

            case View.Icons:
              CurrentView = View.LargeIcons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
              break;

            case View.LargeIcons:
              CurrentView = View.Albums;
              if (!AllowView(CurrentView) || facadeView.AlbumListLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.AlbumView;
              break;

            case View.Albums:
              CurrentView = View.FilmStrip;
              if (!AllowView(CurrentView) || facadeView.FilmstripLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
              break;

            case View.FilmStrip:
              CurrentView = View.List;
              if (!AllowView(CurrentView) || facadeView.ListLayout == null)
                shouldContinue = true;
              else
                facadeView.CurrentLayout = GUIFacadeControl.Layout.List;
              break;
          }
        } while (shouldContinue);

        //SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;

        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          OnQueueItem(iItem);
        }
      }
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      OnClickedBase(controlId, control, actionType);
      if (control == btnSavedPlaylists)
      {
        OnShowSavedPlaylists();
      }

      if (control == btnScrobbleMode)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(Translation.AutomaticallyFillPlaylist); // Automatically fill playlist with

          dlg.Add(Translation.ScrobbleSimilarVideos); // similar tracks
          //dlg.Add(GUILocalizeStrings.Get(33017)); // random tracks
          if (Youtube2MP.LastFmProfile.IsLoged)
          {
            dlg.Add(Translation.ScrobbleNeighboursLike); // tracks your neighbours like
            //dlg.Add(GUILocalizeStrings.Get(33016)); // tracks your friends like
            dlg.Add(Translation.ScrobbleRecentlyPlayed); // tracks played recently
            //dlg.Add(GUILocalizeStrings.Get(33013)); // tracks suiting configured tag {0}
          }
          dlg.SelectedId = (int) currentScrobbleMode;
          dlg.DoModal(GetID);
          if (dlg.SelectedId < 0)
            return;

          if (dlg.SelectedLabelText == Translation.ScrobbleSimilarVideos)
          {
            currentScrobbleMode = ScrobbleMode.Youtube;
          }
          else if (dlg.SelectedLabelText == Translation.ScrobbleNeighboursLike)
          {
            currentScrobbleMode = ScrobbleMode.Neighbours;
          }
          else if (dlg.SelectedLabelText == Translation.ScrobbleRecentlyPlayed)
          {
            currentScrobbleMode = ScrobbleMode.Recent;
          }
          SetScrobbleButonLabel();
        }
        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnScrobbleMode)

      if (control == btnShuffle)
      {
        ShufflePlayList();
      }
      else if (control == btnSave)
      {
        SavePlayList();
      }
      else if (control == btnClear)
      {
        ClearPlayList();
      }
      else if (control == btnNowPlaying)
      {
        GUIWindowManager.ActivateWindow(29052);
      }

      else if (control == btnScrobble)
      {
        //if (_enableScrobbling)
        //{
        //get state of button
        if (btnScrobble.Selected)
        {
          ScrobblerOn = true;
        }
        else
          ScrobblerOn = false;

        if (facadeView.PlayListLayout != null)
          UpdateButtonStates();
      }
      else if ((btnRepeatPlaylist != null) && (control == btnRepeatPlaylist))
      {
        playlistPlayer.RepeatPlaylist = btnRepeatPlaylist.Selected;
      }
    }

    private void SetScrobbleButonLabel()
    {
      switch (currentScrobbleMode)
      {
          case ScrobbleMode.Youtube:
          btnScrobbleMode.Label = Translation.ScrobbleMode + Translation.ScrobbleSimilarVideos;
          break;
          case ScrobbleMode.Neighbours:
          btnScrobbleMode.Label = Translation.ScrobbleMode + Translation.ScrobbleNeighboursLike;
          break;
        case ScrobbleMode.Recent:
          btnScrobbleMode.Label = Translation.ScrobbleMode + Translation.ScrobbleRecentlyPlayed;
          break;
      }
    }

    private void OnShowSavedPlaylists()
    {
      if (Youtube2MP.service.Credentials == null)
      {
        Err_message(Translation.WrongUser);
        return;
      }
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(983); // Saved Playlists
      PlaylistsFeed userPlaylists;
      try
      {
        YouTubeQuery query = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(null));
        userPlaylists = Youtube2MP.service.GetPlaylists(query);

      }
      catch (GDataRequestException exception)
      {
        if (exception.InnerException != null)
          Err_message(exception.InnerException.Message);
        else
          Err_message(exception.Message);
        return;
      }
      dlg.Add(Translation.WatchLater);

      foreach (PlaylistsEntry entry in userPlaylists.Entries)
      {
        dlg.Add(entry.Title.Text);
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;
      PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
      playList.Clear();
      playlistname = dlg.SelectedLabelText;

      YouTubeQuery playlistQuery = null;
      if (dlg.SelectedLabelText == Translation.WatchLater)
      {
        playlistQuery = new YouTubeQuery("https://gdata.youtube.com/feeds/api/users/default/watch_later");
      }
      else
      {
        foreach (PlaylistsEntry entry in userPlaylists.Entries)
        {
          if (entry.Title.Text == dlg.SelectedLabelText)
          {
            playlistQuery = new YouTubeQuery(entry.Content.Src.Content);
          }
        }
      }
      if(playlistQuery==null)
        return;

      YouTubeFeed playlistFeed=null;
      int start = 1;
      do
      {
        playlistQuery.StartIndex = start;
        playlistQuery.NumberToRetrieve = 50;
        playlistFeed = Youtube2MP.service.Query(playlistQuery);
        foreach (YouTubeEntry playlistEntry in playlistFeed.Entries)
        {
          if (IsVideoUsable(playlistEntry))
            AddItemToPlayList(playlistEntry, ref playList);
        }
        start += 50;
      } while (playlistFeed.TotalResults > start - 1);

      GUIPropertyManager.SetProperty("#currentmodule", "Youtube.Fm/Playlist/" + playlistname);
      LoadDirectory(string.Empty);
    }

    public void AddItemToPlayList(YouTubeEntry vid, ref PlayList playList)
    {
      if (playList == null || vid == null)
        return;
      string PlayblackUrl = "";


      GUIListItem pItem = new GUIListItem(vid.Title.Text);
      pItem.MusicTag = vid;
      try
      {
        pItem.Duration = Convert.ToInt32(vid.Duration.Seconds);
      }
      catch (Exception)
      {
      }

      try
      {
        PlayblackUrl = vid.AlternateUri.ToString();
        if (vid.Media != null && vid.Media.Contents != null && vid.Media.Contents.Count > 0)
        {
          PlayblackUrl = string.Format("http://www.youtube.com/v/{0}", Youtube2MP.getIDSimple(vid.Id.AbsoluteUri));
        }
      }
      catch (Exception)
      {
        return;
      }

      VideoInfo qa = new VideoInfo();
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = PlayListItem.PlayListItemType.VideoStream;
      // Playlists.PlayListItem.PlayListItemType.Audio;
      qa.Entry = vid;
      playlistItem.FileName = PlayblackUrl;
      playlistItem.Description = pItem.Label;
      playlistItem.Duration = pItem.Duration;
      playlistItem.MusicTag = qa;
      playList.Add(playlistItem);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          {
            for (int i = 0; i < facadeView.Count; ++i)
            {
              GUIListItem item = facadeView[i];
              if (item != null && item.Selected)
              {
                item.Selected = false;
                break;
              }
            }

            UpdateButtonStates();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED:
          {
            if (m_iLastControl == facadeView.GetID && facadeView.Count <= 0)
            {
              if (GUIWindowManager.ActiveWindow == (int)GetID)
              {
                m_iLastControl = btnNowPlaying.GetID;
                GUIControl.FocusControl(GetID, m_iLastControl);
              }
            }

            SelectCurrentPlayingSong();
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected virtual void UpdateButtonStates()
    {
      if (GUIWindowManager.ActiveWindow == (int)GetID)
      {
        if (facadeView != null)
        {
          if (facadeView.Count > 0)
          {
            btnClear.Disabled = false;
            //            btnPlay.Disabled = false;
            btnSave.Disabled = false;

            if (ScrobblerOn)
              btnScrobble.Selected = true;
            else
              btnScrobble.Selected = false;
          }
          else
          {
            btnClear.Disabled = true;
            //            btnPlay.Disabled = true;
            btnSave.Disabled = true;
          }
        }
        else
        {
          btnClear.Disabled = true;
          //          btnPlay.Disabled = true;
          btnSave.Disabled = true;
        }
      }

      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);

      string strLine = string.Empty;
      View view = CurrentView;
      switch (view)
      {
        case View.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          strLine = GUILocalizeStrings.Get(529);
          break;
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case View.PlayList:
          strLine = GUILocalizeStrings.Get(101);
          break;
      }
      btnViewAs.Label = strLine;
    }

    protected void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null)
        return;
      if (item.IsFolder)
        return;

      string strPath = item.Path;
      playlistPlayer.CurrentPlaylistType = _playlistType;
      playlistPlayer.Reset();
      PlayListPlayer.SingletonPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_NONE;
      Youtube2MP.temp_player.CurrentPlaylistType = PlayListType.PLAYLIST_NONE;
      playlistPlayer.Play(iItem);
      SelectCurrentPlayingSong();
      UpdateButtonStates();
    }

    protected void OnQueueItem(int iItem)
    {
      RemovePlayListItem(iItem);
    }

    public override void Process()
    {
      if (!m_strCurrentFile.Equals(g_Player.CurrentFile))
      {
        m_strCurrentFile = g_Player.CurrentFile;
        GUIMessage msg;
        if (g_Player.Playing)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
        else
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
      }
    }

    #endregion

    void OnThreadMessage(GUIMessage message)
    {
      try
      {
        switch (message.Message)
        {
          case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
            ClearScrobbleStartTrack();
            break;

          //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
          case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
            if (playlistPlayer.CurrentPlaylistType == _playlistType)
            {
              scroblerBackgroundWorker.RunWorkerAsync();
            }
            break;
          // delaying internet lookups for smooth playback start
          case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:
            if (playlistPlayer.CurrentPlaylistType == _playlistType && ScrobblerOn ) // && playlistPlayer.CurrentSong != 0)
            {
              //if (Youtube2MP._settings.UseYouTubePlayer)
              //DoScrobbleLookups();
            }
            break;
        }
      }
      catch
      {
      }
    }


    protected void LoadDirectory(string strNewDirectory)
    {
      if (facadeView != null)
      {
        GUIWaitCursor.Show();
        try
        {
          //TimeSpan totalPlayingTime = new TimeSpan();
          GUIListItem SelectedItem = facadeView.SelectedListItem;
          if (SelectedItem != null)
          {
            if (SelectedItem.IsFolder && SelectedItem.Label != "..")
            {
              m_history.Set(SelectedItem.Label, m_strDirectory);
            }
          }
          m_strDirectory = strNewDirectory;
          GUIControl.ClearControl(GetID, facadeView.GetID);

          List<GUIListItem> itemlist = new List<GUIListItem>();

          PlayList playlist = playlistPlayer.GetPlaylist(_playlistType);
          /* copy playlist from general playlist*/
          int iCurrentSong = -1;
          if (playlistPlayer.CurrentPlaylistType == _playlistType)
            iCurrentSong = playlistPlayer.CurrentSong;

          string strFileName;
          for (int i = 0; i < playlist.Count; ++i)
          {
            PlayListItem item = playlist[i];
            strFileName = item.FileName;

            GUIListItem pItem = new GUIListItem(item.Description);
            pItem.Path = strFileName;
            pItem.MusicTag = item.MusicTag;
            pItem.IsFolder = false;
            //pItem.m_bIsShareOrDrive = false;

            MediaPortal.Util.Utils.SetDefaultIcons(pItem);
            //if (item.Played)
            //{
            //  pItem.Shaded = true;
            //}
            if(item.Type == PlayListItem.PlayListItemType.Unknown)
            {
              pItem.Shaded = true;
            }
            if (item.Duration > 0)
            {
              int nDuration = item.Duration;
              if (nDuration > 0)
              {
                string str = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
                pItem.Label2 = str;
              }
              else
                pItem.Label2 = string.Empty;
            }
            //pItem.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            itemlist.Add(pItem);
          }
          //OnRetrieveMusicInfo(ref itemlist);
          iCurrentSong = 0;
          strFileName = string.Empty;
          //	Search current playlist item
          if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 && g_Player.Playing
            && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
            || (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST && playlistPlayer.CurrentPlaylistType == _playlistType
            && g_Player.Playing))
          {
            iCurrentSong = playlistPlayer.CurrentSong;
            if (iCurrentSong >= 0)
            {
              playlist = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
              if (iCurrentSong < playlist.Count)
              {
                PlayListItem item = playlist[iCurrentSong];
                strFileName = item.FileName;
              }
            }
          }

          string strSelectedItem = m_history.Get(m_strDirectory);
          int iItem = 0;
          foreach (GUIListItem item in itemlist)
          {
            VideoInfo videoInfo = item.MusicTag as VideoInfo;

            if (videoInfo != null)
            {
              YouTubeEntry tag = videoInfo.Entry;
              string imageFile = GetLocalImageFileName(GetBestUrl(tag.Media.Thumbnails));
              if (File.Exists(imageFile))
              {
                item.ThumbnailImage = imageFile;
                item.IconImage = imageFile;
                item.IconImageBig = imageFile;
              }
              else
              {
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                DownloadImage(GetBestUrl(tag.Media.Thumbnails), item);
              }
            }
            item.OnItemSelected+=item_OnItemSelected;
            facadeView.Add(item);
            //	synchronize playlist with current directory
            if (strFileName.Length > 0 && item.Path == strFileName)
            {
              item.Selected = true;
            }
          }
          int iTotalItems = itemlist.Count;
          if (itemlist.Count > 0)
          {
            GUIListItem rootItem = itemlist[0];
            if (rootItem.Label == "..")
              iTotalItems--;
          }
          SetLabels();
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            if (item.Label == strSelectedItem)
            {
              GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
              break;
            }
            iItem++;
          }
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            if (item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
            {
              item.Selected = true;
              break;
            }
          }
          UpdateButtonStates();
          GUIWaitCursor.Hide();
          OnDownloadTimedEvent(null, null);
        }
        catch (Exception ex)
        {
          GUIWaitCursor.Hide();
          Log.Error("GUIYoutubePlaylist: An error occured while loading the list - {0}", ex.Message);
        }
      }
    }

    void ClearScrobbleStartTrack()
    {
      //if (_rememberStartTrack && _scrobbleStartTrack != null)
      //    _scrobbleStartTrack = null;
    }

    void ClearFileItems()
    {
      GUIControl.ClearControl(GetID, facadeView.GetID);
    }

    void ClearPlayList()
    {
      ClearFileItems();
      playlistPlayer.GetPlaylist(_playlistType).Clear();
      if (playlistPlayer.CurrentPlaylistType == _playlistType)
        playlistPlayer.Reset();
      ClearScrobbleStartTrack();
      LoadDirectory(string.Empty);
      UpdateButtonStates();
      GUIControl.FocusControl(GetID, btnNowPlaying.GetID);

    }

    void RemovePlayListItem(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];
      if (pItem == null)
        return;
      string strFileName = pItem.Path;

      playlistPlayer.Remove(_playlistType, strFileName);

      LoadDirectory(m_strDirectory);
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      SelectCurrentPlayingSong();
    }

    void ShufflePlayList()
    {
      ClearFileItems();
      PlayList playlist = playlistPlayer.GetPlaylist(_playlistType);

      if (playlist.Count <= 0)
        return;
      string strFileName = string.Empty;
      if (playlistPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == _playlistType)
        {
          PlayListItem item = playlist[playlistPlayer.CurrentSong];
          strFileName = item.FileName;
        }
      }
      playlist.Shuffle();
      if (playlistPlayer.CurrentPlaylistType == _playlistType)
        playlistPlayer.Reset();

      if (strFileName.Length > 0)
      {
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem item = playlist[i];
          if (item.FileName == strFileName)
            playlistPlayer.CurrentSong = i;
        }
      }
      LoadDirectory(m_strDirectory);
      SelectCurrentPlayingSong();
    }

    void SavePlayList()
    {
      if (Youtube2MP.service.Credentials == null)
      {
        Err_message(Translation.WrongUser);
        return;
      }

      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      // display an virtual keyboard
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = playlistname;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        // input confirmed -- execute the search
        playlistname = keyboard.Text;
        if (playlistname == Translation.WatchLater)
        {
          Err_message("Saving Watch Later not supported !");
          return;
        }
        PlaylistsFeed userPlaylists;
        YouTubeQuery query = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(null));
        try
        {
          userPlaylists = Youtube2MP.service.GetPlaylists(query);
        }
        catch (GDataRequestException exception)
        {
          if (exception.InnerException != null)
            Err_message(exception.InnerException.Message);
          else
            Err_message(exception.Message);
          return;
        }
        PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
        if (playlistname != Translation.WatchLater)
        {
          foreach (PlaylistsEntry entry in userPlaylists.Entries)
          {
            if (entry.Title.Text == playlistname)
            {
              entry.Delete();
            }
          }
        }
        else
        {
          YouTubeQuery playlistQuery = new YouTubeQuery("https://gdata.youtube.com/feeds/api/users/default/watch_later");
          YouTubeFeed playlistFeed = null;
          int start = 1;
          do
          {
            playlistQuery.StartIndex = start;
            playlistQuery.NumberToRetrieve = 50;
            playlistFeed = Youtube2MP.service.Query(playlistQuery);
            foreach (YouTubeEntry playlistEntry in playlistFeed.Entries)
            {
              playlistEntry.Delete();
            }
            start += 50;
          } while (playlistFeed.TotalResults > start - 1);
          //Youtube2MP.service.Delete(new Uri("https://gdata.youtube.com/feeds/api/users/default/watch_later"));
        }

        GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
        if (dlgProgress != null)
        {
          dlgProgress.Reset();
          dlgProgress.SetHeading(Translation.PlaylistSavingProgress);
          dlgProgress.SetLine(1, "");
          dlgProgress.SetLine(2, "");
          dlgProgress.SetPercentage(0);
          dlgProgress.Progress();
          dlgProgress.ShowProgressBar(true);
          dlgProgress.StartModal(GetID);
        }

        int notsaved = 0;
        
        Playlist pl = new Playlist();
        pl.Title = playlistname;
        pl.Summary = Translation.PlaylistSummary;
        pl =Youtube2MP.request.Insert(new Uri(YouTubeQuery.CreatePlaylistsUri(null)), pl);
        int i = 1;
        foreach (PlayListItem playitem in playList)
        {
          VideoInfo info = (VideoInfo)playitem.MusicTag;
          YouTubeEntry videoEntry = info.Entry;

          PlayListMember pm = new PlayListMember();

          // Insert <id> or <videoid> for video here
          pm.Id = videoEntry.VideoId;

          if (IsVideoUsable(videoEntry))
            try
            {
              if (playlistname != Translation.WatchLater)
              {
                Youtube2MP.request.AddToPlaylist(pl, pm);
              }
              else
              {
                Youtube2MP.service.Insert(new Uri("https://gdata.youtube.com/feeds/api/users/default/watch_later"),
                                          videoEntry);
              }
            }
            catch (Exception ex)
            {
              Thread.Sleep(2000);
              notsaved++;
              playitem.Type = PlayListItem.PlayListItemType.Unknown;
            }
          else
          {
            notsaved++;
            playitem.Type = PlayListItem.PlayListItemType.Unknown;
          }
          if (i % 10 == 0)
            Thread.Sleep(1000);
          if (dlgProgress != null)
          {
            double pr = ((double)i / (double)playList.Count) * 100;
            dlgProgress.SetLine(1, videoEntry.Title.Text);
            dlgProgress.SetLine(2,
                                i.ToString() + "/" + playList.Count.ToString() + "( skipped " + notsaved.ToString() + ")");
            dlgProgress.SetPercentage((int) pr);
            dlgProgress.Progress();
          }
          i++;
        }
        if (dlgProgress != null)
          dlgProgress.Close();

        if (notsaved > 0)
          Err_message(Translation.SomePlaylistItemNotSaved);

        LoadDirectory(string.Empty);
      }
    }

    void SelectCurrentPlayingSong()
    {
      if (GUIWindowManager.ActiveWindow != 29051)
        return;
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == _playlistType)
      {
        if (GUIWindowManager.ActiveWindow == GetID)
        {
          // delete prev. selected item
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            if (item != null && item.Selected)
            {
              item.Selected = false;
              break;
            }
          }

          // set current item selected
          int iSong = playlistPlayer.CurrentSong;
          if (iSong >= 0 && iSong <= facadeView.Count)
          {
            GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
            GUIListItem item = facadeView[iSong];
            if (item != null)
              item.Selected = true;
          }
        }
      }
    }


    private void MovePlayListItemUp()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = _playlistType;

      if (playlistPlayer.CurrentPlaylistType != _playlistType
          || facadeView.CurrentLayout != GUIFacadeControl.Layout.Playlist
          || facadeView.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;

      PlayList playList = playlistPlayer.GetPlaylist(_playlistType);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = facadeView.MoveItemUp(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
        playlistPlayer.CurrentSong = selectedIndex;

      facadeView.SelectedListItemIndex = selectedIndex;
      UpdateButtonStates();
    }

    private void MovePlayListItemDown()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = _playlistType;

      if (playlistPlayer.CurrentPlaylistType != _playlistType
          || facadeView.CurrentLayout != GUIFacadeControl.Layout.Playlist
          || facadeView.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;
      PlayList playList = playlistPlayer.GetPlaylist(_playlistType);

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = facadeView.MoveItemDown(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
        playlistPlayer.CurrentSong = selectedIndex;

      UpdateButtonStates();
    }

    private void DeletePlayListItem()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = _playlistType;

      if (playlistPlayer.CurrentPlaylistType != _playlistType
          || facadeView.CurrentLayout != GUIFacadeControl.Layout.Playlist
          || facadeView.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;

      string currentFile = g_Player.CurrentFile;
      GUIListItem item = facadeView[iItem];
      RemovePlayListItem(iItem);

      if (currentFile.Length > 0 && currentFile == item.Path)
      {
        string nextTrackPath = Youtube2MP.player.GetNext();

        if (nextTrackPath.Length == 0)
          g_Player.Stop();

        else
        {
          if (iItem == facadeView.Count)
            playlistPlayer.Play(iItem - 1);

          else
            playlistPlayer.PlayNext();
        }
      }

      if (facadeView.Count == 0)
        g_Player.Stop();

      else
        facadeView.PlayListLayout.SelectedListItemIndex = iItem;

      UpdateButtonStates();
    }

    private void PlaybackStartedThread(object aParam)
    {
      //if (Youtube2MP.NowPlayingSong == null)
      //    return;
      //QueueLastSong();
      //OnStateChangedEvent();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnStateChangedEvent()
    {
      //Log.Debug("Audioscrobbler plugin: found database track for: {0}", g_Player.CurrentFile);        
      SetStartTime();
      OnSongChangedEvent();

    }

    private void SetStartTime()
    {
    }

    private void OnSongChangedEvent()
    {
    }



    public void OnLengthTickEvent(object trash_, ElapsedEventArgs args_)
    {
    }

    /// <summary>
    /// Launch the actual submit
    /// </summary>
    private void QueueLastSong()
    {
    }

    private void OnSongLoadedThread()
    {
    }

    private void DoRefreshList()
    {
      if (facadeView != null)
      {
        // only focus the file while playlist is visible
        if (GUIWindowManager.ActiveWindow == GetID)
        {
          LoadDirectory(string.Empty);
          SelectCurrentPlayingSong();
        }
      }
    }


    protected virtual View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }


    protected void SetLabels()
    {
      if (facadeView != null)
      {
        try
        {

          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];

            //handler.SetLabel(item.AlbumInfoTag as Song, ref item);
          }
        }
        catch (Exception)
        {
          //Log.Error("GUIMusicPlaylist: exception occured - item without Albumtag? - {0} / {1}", ex.Message, ex.StackTrace);
        }
      }
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item == null || parent == null)
        return;

      VideoInfo vid = facadeView.SelectedListItem.MusicTag as VideoInfo;

      if (vid != null)
      {
        SetLabels(vid.Entry, "Curent");
      }
      else
      {
        ClearLabels("Curent"); ;
      }
    }
  }
}