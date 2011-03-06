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


using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using Action = MediaPortal.GUI.Library.Action;
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
            Similar = 0,
            Neighbours = 1,
            Friends = 2,
            Tags = 3,
            Recent = 4,
            Random = 5,
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



        private static int CompareSongsByTimesPlayed(Song x, Song y)
        {
            // ...and y is not null, compare 
            int retval = 0;
            try
            {
                if (x.TimesPlayed == 0)
                    return 0;

                if (y.TimesPlayed == 0)
                    return 0;

                if (x.TimesPlayed == y.TimesPlayed)
                    return 0;
                else
                    if (x.TimesPlayed < y.TimesPlayed)
                        retval = 1;
                    else
                        retval = -1;

                if (retval != 0)
                {
                    return retval;
                }
                else
                {
                    return 0;
                }
            }

            catch (Exception)
            {
                return 0;
            }
        }

        // add the beginning artist again to avoid drifting away in style.
        const int REINSERT_AFTER_THIS_MANY_SONGS = 10;

        #region Variables
        private const int MIN_DURATION = 30;
        private const int MAX_DURATION = 86400; // 24h

        private const int INFINITE_TIME = Int32.MaxValue;
        private int _alertTime;
        private Timer SongLengthTimer;
        private TimeSpan _playingSecs = new TimeSpan(0, 0, 1);
        private int _lastPosition = 0;

        MusicDatabase mdb = null;
        DirectoryHistory m_history = new DirectoryHistory();
        string m_strDirectory = string.Empty;
        int m_iItemSelected = -1;
        int m_iLastControl = 0;
        int m_nTempPlayListWindow = 0;
        string m_strTempPlayListDirectory = string.Empty;
        string m_strCurrentFile = string.Empty;
        string _currentScrobbleUser = string.Empty;
        Song _scrobbleStartTrack;
        VirtualDirectory _virtualDirectory = new VirtualDirectory();
        //const int MaxNumPShuffleSongPredict = 12;
        int _totalScrobbledSongs = 0;
        int _maxScrobbledSongsPerArtist = 1;
        int _maxScrobbledArtistsForSongs = 4;
        int _preferCountForTracks = 2;
        private bool ScrobblerOn = false;
        private bool _enableScrobbling = false;
        private bool _useSimilarRandom = true;
        private bool _rememberStartTrack = true;
        private List<string> _scrobbleUsers = new List<string>(1);
        private AudioscrobblerUtils ascrobbler = null;
        private ScrobblerUtilsRequest _lastRequest;
        protected View currentView = View.List;
        protected string _currentPlaying = string.Empty;
        public bool _announceNowPlaying = true;
        public PlayListType _playlistType = PlayListType.PLAYLIST_MUSIC_VIDEO;


        #endregion

        protected delegate void ThreadRefreshList();

        private ScrobbleMode currentScrobbleMode = ScrobbleMode.Similar;
        private offlineMode currentOfflineMode = offlineMode.random;

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
        [SkinControlAttribute(29)]
        protected GUIButtonControl btnScrobbleUser = null;
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
            _virtualDirectory.SetExtensions(MediaPortal.Util.Utils.AudioExtensions);
        }

        private void LoadScrobbleUserSettings()
        {
            string currentUID = Convert.ToString(mdb.AddScrobbleUser(_currentScrobbleUser));
            ScrobblerOn = (mdb.AddScrobbleUserSettings(currentUID, "iScrobbleDefault", -1) == 1) ? true : false;
            _maxScrobbledArtistsForSongs = mdb.AddScrobbleUserSettings(currentUID, "iAddArtists", -1);
            _maxScrobbledSongsPerArtist = mdb.AddScrobbleUserSettings(currentUID, "iAddTracks", -1);
            _preferCountForTracks = mdb.AddScrobbleUserSettings(currentUID, "iPreferCount", -1);
            _rememberStartTrack = (mdb.AddScrobbleUserSettings(currentUID, "iRememberStartArtist", -1) == 1) ? true : false;
            _maxScrobbledArtistsForSongs = (_maxScrobbledArtistsForSongs > 0) ? _maxScrobbledArtistsForSongs : 3;
            _maxScrobbledSongsPerArtist = (_maxScrobbledSongsPerArtist > 0) ? _maxScrobbledSongsPerArtist : 1;
            int tmpRMode = mdb.AddScrobbleUserSettings(currentUID, "iOfflineMode", -1);

            switch (tmpRMode)
            {
                case 0:
                    currentOfflineMode = offlineMode.random;
                    break;
                case 1:
                    currentOfflineMode = offlineMode.timesplayed;
                    break;
                case 2:
                    currentOfflineMode = offlineMode.favorites;
                    break;
                default:
                    currentOfflineMode = offlineMode.random;
                    break;
            }
            Log.Info("YoutubePlayList: Scrobblesettings loaded for {0} - dynamic playlist inserts: {1}", _currentScrobbleUser, Convert.ToString(ScrobblerOn));
        }

        #region overrides
        public override bool Init()
        {
            mdb = MusicDatabase.Instance;
            m_strDirectory = System.IO.Directory.GetCurrentDirectory();

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                _enableScrobbling = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
                _currentScrobbleUser = xmlreader.GetValueAsString("audioscrobbler", "user", "Username");
                _useSimilarRandom = xmlreader.GetValueAsBool("audioscrobbler", "usesimilarrandom", true);
                _announceNowPlaying = xmlreader.GetValueAsBool("audioscrobbler", "EnableNowPlaying", true);

            }

            _scrobbleUsers = mdb.GetAllScrobbleUsers();
            // no users in database
            if (_scrobbleUsers.Count > 0 && _enableScrobbling)
                LoadScrobbleUserSettings();

            ascrobbler = AudioscrobblerUtils.Instance;
            //      ScrobbleLock = new object();
            //added by Sam
            GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
            Youtube2MP.player.PlayBegin += new YoutubePlaylistPlayer.EventHandler(player_PlayBegin);

            return Load(GUIGraphicsContext.Skin + @"\youtubeplaylist.xml");
        }

        void player_PlayBegin(PlayListItem en)
        {
            try
            {
                Thread stateThread = new Thread(new ParameterizedThreadStart(PlaybackStartedThread));
                stateThread.IsBackground = true;
                stateThread.Name = "Scrobbler event";
                stateThread.Start((object)en);

                Thread LoadThread = new Thread(new ThreadStart(OnSongLoadedThread));
                LoadThread.IsBackground = true;
                LoadThread.Name = "Scrobbler loader";
                LoadThread.Start();
            }
            catch (Exception ex)
            {
                Log.Error("YouTube.Fm plugin: Error creating threads on playback start - {0} {1}", ex.Message);
                Log.Error(ex);
            }
        }


        public override void DeInit()
        {
            GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);

            if (_lastRequest != null)
                ascrobbler.RemoveRequest(_lastRequest);

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

        private void InstantPlay()
        {
            string artist = "";
            string title = "";

            title = GUIPropertyManager.GetProperty("#selecteditem");

            if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(title))
            {
                artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                title = GUIPropertyManager.GetProperty("#Play.Current.Title");
            }

            if (string.IsNullOrEmpty(title))
            {
                Err_message("No artis or title defined !");
                return;

            }

            string searchString = string.Format("{0} {1}", artist, title);
            YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
            query = SetParamToYouTubeQuery(query, false);
            query.Query = searchString;
            query.OrderBy = "relevance";

            YouTubeFeed vidr = Youtube2MP.service.Query(query);

            if (vidr.Entries.Count > 0)
            {
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                List<YouTubeEntry> items = new List<YouTubeEntry>();
                if (dlg == null) return;
                dlg.Reset();
                foreach (YouTubeEntry entry in vidr.Entries)
                {
                    GUIListItem item = new GUIListItem();
                    item.Label = entry.Title.Text;
                    item.Label2 = "";
                    dlg.Add(item);
                    items.Add(entry);
                }
                dlg.DoModal(GetID);
                if (dlg.SelectedId == -1) return;
                DoPlay(items[dlg.SelectedId], true, null);
            }
            else
            {
                Err_message("No item was found !");
            }
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

            if (Youtube2MP._settings.UseYouTubePlayer)
            {
                _playlistType = PlayListType.PLAYLIST_VIDEO_TEMP;
            }
            else
            {
                _playlistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
            }

            currentView = View.PlayList;
            facadeView.CurrentLayout = GUIFacadeControl.Layout.Playlist;

            if (ScrobblerOn)
                btnScrobble.Selected = true;

            if (_scrobbleUsers.Count < 2)
                btnScrobbleUser.Visible = false;

            btnScrobbleUser.Label = GUILocalizeStrings.Get(33005) + _currentScrobbleUser;

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
                playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("musicfiles", "repeat", true);
            }

            if (btnRepeatPlaylist != null)
            {
                btnRepeatPlaylist.Selected = playlistPlayer.RepeatPlaylist;
            }

            SelectCurrentPlayingSong();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            m_iItemSelected = facadeView.SelectedListItemIndex;
            using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                settings.SetValueAsBool("musicfiles", "repeat", playlistPlayer.RepeatPlaylist);
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
            }//if (control == btnViewAs)

            //if (control == btnSortBy)
            //{
            //  OnShowSort();
            //}

            //if (control == btnViews)
            //{
            //  OnShowViews();
            //}

            //if (control == btnSavedPlaylists)
            //{
            //  OnShowSavedPlaylists(m_strPlayListPath);
            //}

            if (control == facadeView)
            {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
                OnMessage(msg);
                int iItem = (int)msg.Param1;
                //if (actionType == Action.ActionType.ACTION_SHOW_INFO)
                //{
                //  OnInfo(iItem);
                //}

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
            if (control == btnScrobbleUser)
            {
                // no users in database
                if (_scrobbleUsers.Count == 0)
                    return;
                //for (int i = 0; i < scrobbleusers.Count; i++)       
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                if (dlg != null)
                {
                    dlg.Reset();
                    dlg.SetHeading(GUILocalizeStrings.Get(497));//Menu
                    int selected = 0;
                    int count = 0;
                    foreach (string scrobbler in _scrobbleUsers)
                    {
                        dlg.Add(scrobbler);
                        if (scrobbler == _currentScrobbleUser)
                            selected = count;
                        count++;
                    }
                    dlg.SelectedLabel = selected;
                }
                dlg.DoModal(GetID);
                if (dlg.SelectedLabel < 0)
                    return;

                if (_currentScrobbleUser != dlg.SelectedLabelText)
                {
                    _currentScrobbleUser = dlg.SelectedLabelText;
                    btnScrobbleUser.Label = GUILocalizeStrings.Get(33005) + _currentScrobbleUser;

                    AudioscrobblerBase.DoChangeUser(_currentScrobbleUser, mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(_currentScrobbleUser)), ""));
                    LoadScrobbleUserSettings();
                    UpdateButtonStates();
                }

                GUIControl.FocusControl(GetID, controlId);
                return;
            }//if (control == btnScrobbleUser)


            if (control == btnScrobbleMode)
            {
                bool shouldContinue = false;
                do
                {
                    shouldContinue = false;
                    GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                    if (dlg != null)
                    {
                        dlg.Reset();
                        dlg.SetHeading(GUILocalizeStrings.Get(33010)); // Automatically fill playlist with

                        dlg.Add(GUILocalizeStrings.Get(33011)); // similar tracks
                        dlg.Add(GUILocalizeStrings.Get(33017)); // random tracks
                        if (_enableScrobbling)
                        {
                            dlg.Add(GUILocalizeStrings.Get(33012)); // tracks your neighbours like
                            dlg.Add(GUILocalizeStrings.Get(33016)); // tracks your friends like
                            //dlg.Add(GUILocalizeStrings.Get(33014)); // tracks played recently
                            //dlg.Add(GUILocalizeStrings.Get(33013)); // tracks suiting configured tag {0}
                        }

                        dlg.DoModal(GetID);
                        if (dlg.SelectedLabel < 0)
                            return;

                        switch (dlg.SelectedId)
                        {
                            case 1:
                                currentScrobbleMode = ScrobbleMode.Similar;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33001);
                                break;
                            case 2:
                                currentScrobbleMode = ScrobbleMode.Random;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33007);
                                break;
                            case 3:
                                currentScrobbleMode = ScrobbleMode.Neighbours;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33002);
                                break;
                            case 4:
                                currentScrobbleMode = ScrobbleMode.Friends;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33006);
                                break;
                            case 5:
                                currentScrobbleMode = ScrobbleMode.Recent;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33004);
                                break;
                            case 6:
                                currentScrobbleMode = ScrobbleMode.Tags;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33003);
                                break;
                            default:
                                currentScrobbleMode = ScrobbleMode.Random;
                                btnScrobbleMode.Label = GUILocalizeStrings.Get(33007);
                                break;
                        }

                        if (currentScrobbleMode == ScrobbleMode.Random)
                            if (currentOfflineMode == offlineMode.favorites)
                            {
                                MusicDatabase checkdb = MusicDatabase.Instance;
                                if (checkdb.GetTotalFavorites() <= _maxScrobbledArtistsForSongs * 2)
                                {
                                    shouldContinue = true;
                                    Log.Warn("Audioscrobbler playlist: Cannot activate offline mode: favorites because there are not enough tracks");
                                }
                            }
                    }

                } while (shouldContinue);

                CheckScrobbleInstantStart();
                GUIControl.FocusControl(GetID, controlId);
                return;
            }//if (control == btnScrobbleMode)

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
            //else if (control == btnPlay)
            //{
            //  playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
            //  playlistPlayer.Reset();
            //  playlistPlayer.Play(facadeView.SelectedListItemIndex);

            //  UpdateButtonStates();
            //}

            else if (control == btnScrobble)
            {
                //if (_enableScrobbling)
                //{
                //get state of button
                if (btnScrobble.Selected)
                {
                    ScrobblerOn = true;
                    CheckScrobbleInstantStart();
                }
                else
                    ScrobblerOn = false;

                if (facadeView.PlayListLayout != null)
                    //{
                    //  // Prevent the currently playing track from being scrolled off the top 
                    //  // or bottom of the screen when other items are re-ordered
                    //  facadeView.PlayListView.AllowLastVisibleListItemDown = !ScrobblerOn;
                    //  facadeView.PlayListView.AllowMoveFirstVisibleListItemUp = !ScrobblerOn;
                    //}
                    UpdateButtonStates();
            }
            else if ((btnRepeatPlaylist != null) && (control == btnRepeatPlaylist))
            {
                playlistPlayer.RepeatPlaylist = btnRepeatPlaylist.Selected;
            }
        }

        private void OnShowSavedPlaylists()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
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
            foreach (PlaylistsEntry entry in userPlaylists.Entries)
            {
                dlg.Add(entry.Title.Text);
            }
            dlg.DoModal(GetID);
            if (dlg.SelectedId == -1) return;
            PlayList playList = Youtube2MP.player.GetPlaylist(_playlistType);
            playList.Clear();
            foreach (PlaylistsEntry entry in userPlaylists.Entries)
            {
                if (entry.Title.Text == dlg.SelectedLabelText)
                {
                    PlaylistFeed playlistFeed;
                    int start = 1;
                    do
                    {
                        YouTubeQuery playlistQuery = new YouTubeQuery(entry.Content.Src.Content);
                        playlistQuery.StartIndex = start;
                        playlistQuery.NumberToRetrieve = 50;
                        playlistFeed = Youtube2MP.service.GetPlaylist(playlistQuery);
                        foreach (YouTubeEntry playlistEntry in playlistFeed.Entries)
                        {
                            AddItemToPlayList(playlistEntry, ref playList);
                        }
                        start += 50;
                    } while (playlistFeed.TotalResults > start - 1);
                }
            }
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

            //GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
            //if (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_GENRE)
            //{
            //  GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100005), handler.LocalizedCurrentView));
            //}
            //else
            //{
            //  GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
            //}

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
                        if (playlistPlayer.CurrentPlaylistType == _playlistType && playlistPlayer.CurrentSong != 0)
                        {
                            if (!Youtube2MP._settings.UseYouTubePlayer)
                                DoScrobbleLookups();
                        }
                        break;
                    // delaying internet lookups for smooth playback start
                    case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:
                        if (playlistPlayer.CurrentPlaylistType == _playlistType && ScrobblerOn && _enableScrobbling) // && playlistPlayer.CurrentSong != 0)
                        {
                            if (Youtube2MP._settings.UseYouTubePlayer)
                                DoScrobbleLookups();
                        }
                        break;
                }
            }
            catch
            {
            }
        }

        void OnRetrieveMusicInfo(ref List<GUIListItem> items)
        {
            if (items.Count <= 0)
                return;
            MusicDatabase dbs = MusicDatabase.Instance;
            Song song = new Song();
            foreach (GUIListItem item in items)
            {
                if (item.MusicTag == null)
                {
                    if (dbs.GetSongByFileName(item.Path, ref song))
                    {
                        MusicTag tag = new MusicTag();
                        tag = song.ToMusicTag();
                        item.MusicTag = tag;
                    }
                    //else
                    //  if (UseID3)
                    //  {
                    //    item.MusicTag = TagReader.TagReader.ReadTag(item.Path);
                    //  }
                }
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
                        if (item.Played)
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
                    OnRetrieveMusicInfo(ref itemlist);
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
                        YouTubeEntry tag = item.MusicTag as YouTubeEntry;
                        if (tag != null)
                        {
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
            if (_rememberStartTrack && _scrobbleStartTrack != null)
                _scrobbleStartTrack = null;
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
            string strNewFileName = string.Empty;
            VirtualKeyboard keyboard=(VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            // display an virtual keyboard
            if (null == keyboard) return;
            keyboard.Reset();
            keyboard.Text = strNewFileName;
            keyboard.DoModal(GetID);
            if (keyboard.IsConfirmed)
            {
                // input confirmed -- execute the search
                strNewFileName = keyboard.Text;
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
                foreach (PlaylistsEntry entry in userPlaylists.Entries)
                {
                    if (entry.Title.Text == strNewFileName)
                    {
                        entry.Delete();
                    }
                }

                //Playlist pl = new Playlist();
                //pl.Title = strNewFileName;
                //pl.Summary = "Created or modified in MediaPortal";
                //Playlist createdPlaylist = Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreatePlaylistsUri(null)), pl);

                //PlaylistsEntry newPlaylist = new PlaylistsEntry();
                //newPlaylist.Title.Text = strNewFileName;
                ////newPlaylist.Description = "Created or modified in MediaPortal";
                //newPlaylist.Summary.Text = "Created or modified in MediaPortal";
                //PlaylistsEntry createdPlaylist = (PlaylistsEntry)Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreatePlaylistsUri(null)), newPlaylist);
                //Playlist pl = new Playlist();

                //YouTubeQuery query1 = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(null));
                //PlaylistsFeed userPlaylists1 = Youtube2MP.service.GetPlaylists(query1);

                //foreach (PlaylistsEntry entry in userPlaylists1.Entries)
                //{
                //    if (entry.Title.Text == strNewFileName)
                //    {
                //        pl = (Playlist)entry;
                //    }
                //}

                //foreach (PlayListItem playitem in playList)
                //{
                //  VideoInfo info = (VideoInfo)playitem.MusicTag;
                //  YouTubeEntry videoEntry = info.Entry;
                //  PlayListMember pm = new PlayListMember();

                //  // Insert <id> or <videoid> for video here
                //  pm.Id = videoEntry.VideoId;
                //  Youtube2MP.request.AddToPlaylist(pl, pm);
                //}

                PlaylistsEntry newPlaylist = new PlaylistsEntry();
                newPlaylist.Title.Text = strNewFileName;
                //newPlaylist.Description = "Created or modified in MediaPortal";
                newPlaylist.Summary.Text = "Created or modified in MediaPortal";
                PlaylistsEntry createdPlaylist = (PlaylistsEntry)Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreatePlaylistsUri(null)), newPlaylist);

                foreach (PlayListItem playitem in playList)
                {
                    //playitem.MusicTag
                    //string videoEntryUrl = "http://gdata.youtube.com/feeds/api/videos/ADos_xW4_J0";
                    VideoInfo info = (VideoInfo)playitem.MusicTag;
                    YouTubeEntry videoEntry = info.Entry;
                    PlaylistEntry newPlaylistEntry = new PlaylistEntry();
                    newPlaylistEntry.Id = videoEntry.Id;
                    PlaylistEntry createdPlaylistEntry = (PlaylistEntry)Youtube2MP.service.Insert(new Uri(createdPlaylist.Content.Src.Content), newPlaylistEntry);
                }

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

        bool AddRandomSongToPlaylist(ref Song song, ref YouTubeFeed vidr)
        {
            //check duplication
            PlayList playlist = playlistPlayer.GetPlaylist(_playlistType);
            for (int i = 0; i < playlist.Count; i++)
            {
                PlayListItem item = playlist[i];
                if (item.FileName == song.FileName)
                    return false;
            }

            //add to playlist
            PlayListItem playlistItem = new PlayListItem();
            playlistItem.Type = PlayListItem.PlayListItemType.Video; //Playlists.PlayListItem.PlayListItemType.Audio;
            StringBuilder sb = new StringBuilder();

            playlistItem.FileName = song.FileName;
            //sb.Append(song.Track);
            //sb.Append(". ");
            sb.Append(song.Artist);
            if (!string.IsNullOrEmpty(song.Title))
            {
                sb.Append(" - ");
                sb.Append(song.Title);
            }
            playlistItem.Description = sb.ToString();
            playlistItem.Duration = song.Duration;

            MusicTag tag = new MusicTag();
            tag = song.ToMusicTag();
            foreach (YouTubeEntry entry in vidr.Entries)
            {
                if (Youtube2MP.PlaybackUrl(entry) == playlistItem.FileName)
                {
                    playlistItem.MusicTag = entry;
                    if (!Youtube2MP._settings.UseYouTubePlayer)
                    {
                        playlistItem.FileName = Youtube2MP.StreamPlaybackUrl(entry, new VideoInfo());
                    }
                }
            }

            playlistPlayer.GetPlaylist(_playlistType).Add(playlistItem);
            return true;
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

        void CheckScrobbleInstantStart()
        {
            if (ScrobblerOn)
            {
                PlayList playList = playlistPlayer.GetPlaylist(_playlistType);

                if (playList != null)
                {
                    // if scrobbling gets activated after 10 sec event nothing would happen without this
                    if (playList.Count == 1 && g_Player.CurrentPosition > 10)
                    {
                        DoScrobbleLookups();
                    }
                }
                if (playList.Count == 0)
                    if (currentScrobbleMode != ScrobbleMode.Similar)
                    {
                        DoScrobbleLookups();
                    }
            }
        }

        private void UpdateSimilarArtists(string _trackArtist)
        {
            if (_trackArtist == null)
                return;
            if (_trackArtist != string.Empty)
            {
                SimilarArtistRequest request2 = new SimilarArtistRequest(
                              _trackArtist,
                              _useSimilarRandom,
                              new SimilarArtistRequest.SimilarArtistRequestHandler(OnUpdateSimilarArtistsCompleted));
                _lastRequest = request2;
                ascrobbler.AddRequest(request2);
            }
        }

        private void UpdateNeighboursArtists(bool randomizeList)
        {
            NeighboursArtistsRequest request = new NeighboursArtistsRequest(
                            randomizeList,
                            new NeighboursArtistsRequest.NeighboursArtistsRequestHandler(OnUpdateNeighboursArtistsCompleted));
            _lastRequest = request;
            ascrobbler.AddRequest(request);
        }

        private void UpdateFriendsArtists(bool randomizeList)
        {
            FriendsArtistsRequest request = new FriendsArtistsRequest(
                            randomizeList,
                            new FriendsArtistsRequest.FriendsArtistsRequestHandler(OnUpdateFriendsArtistsCompleted));
            _lastRequest = request;
            ascrobbler.AddRequest(request);
        }

        private void UpdateRandomTracks()
        {
            RandomTracksRequest request = new RandomTracksRequest(
                            new RandomTracksRequest.RandomTracksRequestHandler(OnUpdateRandomTracksCompleted));
            _lastRequest = request;
            ascrobbler.AddRequest(request);
        }

        private void UpdateUnheardTracks()
        {
            UnheardTracksRequest request = new UnheardTracksRequest(
                            new UnheardTracksRequest.UnheardTracksRequestHandler(OnUpdateUnheardTracksCompleted));
            _lastRequest = request;
            ascrobbler.AddRequest(request);
        }

        private void UpdateFavoriteTracks()
        {
            FavoriteTracksRequest request = new FavoriteTracksRequest(
                            new FavoriteTracksRequest.FavoriteTracksRequestHandler(OnUpdateFavoriteTracksCompleted));
            _lastRequest = request;
            ascrobbler.AddRequest(request);
        }

        void DoScrobbleLookups()
        {
            PlayList currentPlaylist = playlistPlayer.GetPlaylist(_playlistType);

            MusicDatabase dbs = MusicDatabase.Instance;
            Song current10SekSong = new Song();
            List<Song> scrobbledArtists = new List<Song>();
            ascrobbler.RemoveRequest(_lastRequest);
            switch (currentScrobbleMode)
            {
                case ScrobbleMode.Similar:
                    if (_rememberStartTrack && _scrobbleStartTrack != null)
                        if (_totalScrobbledSongs > REINSERT_AFTER_THIS_MANY_SONGS)
                        {
                            _totalScrobbledSongs = 0;
                            Song tmpArtist = new Song();
                            tmpArtist = _scrobbleStartTrack;
                            scrobbledArtists.Add(tmpArtist);
                            break;
                        }
                    string strFile = string.Empty;
                    if (g_Player.Player.CurrentFile != null && g_Player.Player.CurrentFile != string.Empty)
                    {
                        strFile = g_Player.Player.CurrentFile;
                        playlistPlayer.CurrentPlaylistType = _playlistType;
                        if (!strFile.Contains("youtube."))
                        {
                            if (playlistPlayer.GetCurrentItem() != null)
                                strFile = playlistPlayer.GetCurrentItem().FileName;
                            else
                                strFile = string.Empty;
                        }
                        if (string.IsNullOrEmpty(strFile) || !strFile.Contains("youtube."))
                        {
                            return;
                        }
                        bool songFound = Youtube2MP.YoutubeEntry2Song(strFile, ref current10SekSong);
                        if (songFound)
                        {
                            if (_scrobbleStartTrack == null || _scrobbleStartTrack.Artist == string.Empty)
                                _scrobbleStartTrack = current10SekSong.Clone();

                            try
                            {
                                UpdateSimilarArtists(current10SekSong.Artist);
                                //scrobbledArtists = ascrobbler.getSimilarArtists(current10SekSong.ToURLArtistString(), _useSimilarRandom);
                                return;
                            }
                            catch (Exception ex)
                            {
                                Log.Error("YouTubePlaylist ScrobbleLookupThread: exception on lookup Similar - {0}", ex.Message);
                            }

                        }
                    }
                    break;

                case ScrobbleMode.Neighbours:
                    //lock (ScrobbleLock)
                    //{
                    try
                    {
                        UpdateNeighboursArtists(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("YouTubePlaylist ScrobbleLookupThread: exception on lookup Neighbourhood - {0}", ex.Message);
                    }
                    //}
                    break;

                case ScrobbleMode.Friends:
                    //lock (ScrobbleLock)
                    //{
                    try
                    {
                        UpdateFriendsArtists(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("YouTubePlaylist ScrobbleLookupThread: exception on lookup - Friends {0}", ex.Message);
                    }
                    //}
                    break;
                case ScrobbleMode.Random:
                    try
                    {
                        switch (currentOfflineMode)
                        {
                            case offlineMode.random:
                                UpdateRandomTracks();
                                break;
                            case offlineMode.timesplayed:
                                UpdateUnheardTracks();
                                break;
                            case offlineMode.favorites:
                                UpdateFavoriteTracks();
                                break;
                            default:
                                UpdateRandomTracks();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("YouTubePlaylist ScrobbleLookupThread: exception on lookup - Random {0}", ex.Message);
                    }
                    break;
            }

            OnScrobbleLookupsCompleted(scrobbledArtists);
        }


        public void OnUpdateSimilarArtistsCompleted(SimilarArtistRequest request2, List<Song> SimilarArtists)
        {
            if (request2.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(SimilarArtists);
            else
                Log.Warn("YouTubePlaylist: OnUpdateSimilarArtistsCompleted: unexpected response for request: {0}", request2.Type);
        }

        public void OnUpdateNeighboursArtistsCompleted(NeighboursArtistsRequest request, List<Song> NeighboursArtists)
        {
            if (request.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(NeighboursArtists);
            else
                Log.Warn("YouTubePlaylist: OnUpdateNeighboursArtistsCompleted: unexpected response for request: {0}", request.Type);
        }

        public void OnUpdateFriendsArtistsCompleted(FriendsArtistsRequest request, List<Song> FriendsArtists)
        {
            if (request.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(FriendsArtists);
            else
                Log.Warn("YouTubePlaylist: OnUpdateFriendsArtistsCompleted: unexpected response for request: {0}", request.Type);
        }

        public void OnUpdateRandomTracksCompleted(RandomTracksRequest request, List<Song> RandomTracks)
        {
            if (request.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(RandomTracks);
            else
                Log.Warn("YouTubePlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
        }

        public void OnUpdateUnheardTracksCompleted(UnheardTracksRequest request, List<Song> UnheardTracks)
        {
            if (request.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(UnheardTracks);
            else
                Log.Warn("YouTubePlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
        }

        public void OnUpdateFavoriteTracksCompleted(FavoriteTracksRequest request, List<Song> FavoriteTracks)
        {
            if (request.Equals(_lastRequest))
                OnScrobbleLookupsCompleted(FavoriteTracks);
            else
                Log.Warn("YouTubePlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
        }

        void OnScrobbleLookupsCompleted(List<Song> LookupArtists)
        {
            Log.Debug("YouTubePlaylist: OnScrobbleLookupsCompleted - processing {0} results", Convert.ToString(LookupArtists.Count));

            if (LookupArtists.Count < _maxScrobbledArtistsForSongs)
            {
                if (LookupArtists.Count > 0)
                    for (int i = 0; i < LookupArtists.Count; i++)
                        ScrobbleSimilarArtists(LookupArtists[i].Artist);
            }
            else // enough artists
            {
                int addedSimilarSongs = 0;
                int loops = 0;
                int previouspreferCount = _preferCountForTracks;
                // we WANT to get songs from _maxScrobbledArtistsForSongs
                while (addedSimilarSongs < _maxScrobbledArtistsForSongs)
                {
                    if (ScrobbleSimilarArtists(LookupArtists[loops].Artist))
                        addedSimilarSongs++;
                    loops++;
                    // okay okay seems like there aren't enough files to add
                    if (loops == LookupArtists.Count - 1)
                        // make sure we get a few songs at least...
                        //if (_preferCountForTracks != 2)
                        //{
                        //  _preferCountForTracks = 2;
                        //  Log.Info("ScrobbleLookupThread: could not find enough songs - temporarily accepting all songs");
                        //  loops = 0;
                        //}
                        //else
                        break;
                }
                _preferCountForTracks = previouspreferCount;
            }

            GUIGraphicsContext.form.Invoke(new ThreadRefreshList(DoRefreshList));
        }


        private void PlaybackStartedThread(object aParam)
        {
            if (Youtube2MP.NowPlayingSong == null)
                return;

            AudioscrobblerBase.CurrentPlayingSong.Clear();
            AudioscrobblerBase.CurrentPlayingSong = Youtube2MP.NowPlayingSong;
            QueueLastSong();
            OnStateChangedEvent();
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
            TimeSpan _playingSecs = new TimeSpan(0, 0, 1);
            try
            {
                _playingSecs = TimeSpan.FromSeconds(g_Player.CurrentPosition);
                AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed = DateTime.UtcNow - _playingSecs;
                _lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
            }
            catch (Exception)
            {
                AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed = DateTime.UtcNow;
                _lastPosition = 1;
            }
            Log.Info("Youtube.Fm plugin: Detected new track as: {0} - {1} started at: {2}", AudioscrobblerBase.CurrentPlayingSong.Artist, AudioscrobblerBase.CurrentPlayingSong.Title, AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed.ToLocalTime().ToLongTimeString());
        }

        private void OnSongChangedEvent()
        {
            try
            {
                //_alertTime = INFINITE_TIME;

                // Only submit if we have reasonable info about the song
                if (AudioscrobblerBase.CurrentPlayingSong.Artist == String.Empty || AudioscrobblerBase.CurrentPlayingSong.Title == String.Empty)
                {
                    Log.Info("Youtube.Fm plugin: {0}", "no tags found ignoring song");
                    return;
                }

                if (_announceNowPlaying)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        // try to wait for 6 seconds to give an maybe ongoing submit a chance to finish before the announce
                        // as otherwise the now playing track might not show up on the website
                        if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Init)
                            break;
                        Thread.Sleep(500);
                    }
                    AudioscrobblerBase.DoAnnounceNowPlaying();
                }

                _alertTime = GetAlertTime();

                if (_alertTime != INFINITE_TIME)
                {
                    AudioscrobblerBase.CurrentPlayingSong.AudioScrobblerStatus = SongStatus.Loaded;
                    //startStopSongLengthTimer(true, _alertTime - _playingSecs.Seconds);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Youtube.Fm plugin: Error in song change event - {0}", ex.Message);
            }
        }

        private int GetAlertTime()
        {
            if (AudioscrobblerBase.CurrentPlayingSong.Duration > MAX_DURATION)
            {
                Log.Info("Youtube.Fm plugin: Ignoring long song {0}", AudioscrobblerBase.CurrentPlayingSong.ToShortString());
                return INFINITE_TIME;
            }
            else if (AudioscrobblerBase.CurrentPlayingSong.Duration < MIN_DURATION)
            {
                Log.Info("Youtube.Fm plugin: Ignoring short song {0}", AudioscrobblerBase.CurrentPlayingSong.ToShortString());
                return INFINITE_TIME;
            }
            // If the duration is less then 480 secs, alert when the song
            // is half over, otherwise after 240 seconds.
            if (AudioscrobblerBase.CurrentPlayingSong.Duration < 480)
            {
                return AudioscrobblerBase.CurrentPlayingSong.Duration / 2;
            }
            else
            {
                return 240;
            }
        }

        private void startStopSongLengthTimer(bool startNow, int intervalLength)
        {
            try
            {
                if (SongLengthTimer != null)
                {
                    SongLengthTimer.Close();
                }
                else
                {
                    SongLengthTimer = new Timer();
                    SongLengthTimer.AutoReset = false;
                    SongLengthTimer.Interval = INFINITE_TIME;
                    SongLengthTimer.Elapsed += new ElapsedEventHandler(OnLengthTickEvent);
                }

                if (startNow)
                {
                    Log.Info("Youtube.Fm plugin: Starting song length timer with an interval of {0} seconds",
                             intervalLength.ToString());
                    SongLengthTimer.Interval = intervalLength * 1000;
                    SongLengthTimer.Start();
                }
                else
                {
                    SongLengthTimer.Stop();
                }
            }
            catch (Exception tex)
            {
                Log.Error("Youtube.Fm plugin: Issue with song length timer - start: {0} interval: {1} error: {2}",
                          startNow.ToString(), intervalLength.ToString(), tex.Message);
            }
        }

        public void OnLengthTickEvent(object trash_, ElapsedEventArgs args_)
        {
            if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded)
            {
                AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus = SongStatus.Cached;
                Log.Info("Youtube.Fm plugin: Cached song for submit: {0}",
                         AudioscrobblerBase.CurrentSubmitSong.ToShortString());
            }
            else
            {
                Log.Debug("Youtube.Fm plugin: NOT caching song: {0} because status is {1}",
                          AudioscrobblerBase.CurrentSubmitSong.ToShortString(),
                          AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus.ToString());
            }
        }

        /// <summary>
        /// Launch the actual submit
        /// </summary>
        private void QueueLastSong()
        {
            // Submit marked tracks and those which have an action attached
            if ((AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Cached) ||
                (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded && AudioscrobblerBase.CurrentSubmitSong.AudioscrobblerAction != SongAction.N))
            {
                AudioscrobblerBase.pushQueue(AudioscrobblerBase.CurrentSubmitSong);
                AudioscrobblerBase.CurrentSubmitSong.Clear();

                //if (SubmitQueued != null)
                //  SubmitQueued();
            }
        }

        private void OnSongLoadedThread()
        {
            int i = 0;
            try
            {
                for (i = 0; i < 15; i++)
                {
                    if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Init)
                        break;
                    Thread.Sleep(1000);
                }
                Log.Debug("YouTube.Fm plugin: Waited {0} seconds for reinit of submit track", i);

                for (i = 0; i < 15; i++)
                {
                    if (AudioscrobblerBase.CurrentPlayingSong.AudioScrobblerStatus == SongStatus.Loaded)
                        break;
                    Thread.Sleep(1000);
                }
                Log.Debug("YouTube.Fm plugin: Waited {0} seconds for lookup of current track", i);

                if (AudioscrobblerBase.CurrentPlayingSong.Artist != String.Empty)
                {
                    // Don't hand over the reference        
                    AudioscrobblerBase.CurrentSubmitSong = AudioscrobblerBase.CurrentPlayingSong.Clone();
                    Log.Info("YouTube.Fm plugin: Song loading thread sets submit song - {0}", AudioscrobblerBase.CurrentSubmitSong.ToLastFMMatchString(true));
                }
                else
                    Log.Debug("YouTube.Fm plugin: Song loading thread could not set the current for submit - {0}", AudioscrobblerBase.CurrentPlayingSong.ToLastFMMatchString(true));
            }
            catch (Exception ex)
            {
                Log.Error("YouTube.Fm plugin: Error in song load thread {0}", ex.Message);
            }
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

        private bool ScrobbleSimilarArtists(string Artist_)
        {

            MusicDatabase dbs = MusicDatabase.Instance;
            PlayList list = playlistPlayer.GetPlaylist(_playlistType);
            List<Song> songList = new List<Song>();
            int songsAdded = 0;
            int j = 0;
            YouTubeFeed vidr = null;
            Youtube2MP.GetSongsByArtist(Artist_, ref songList, ref vidr);

            //      Log.Debug("GUIMusicPlaylist: ScrobbleSimilarArtists found {0} songs allowed to add", Convert.ToString(songList.Count));

            // exit if not enough songs were found
            if (songList.Count < _maxScrobbledSongsPerArtist)
                return false;

            //// lookup how many times this artist's songs were played
            //avgPlayCount = dbs.GetAveragePlayCountForArtist(Artist_);

            //switch (_preferCountForTracks)
            //{
            //  case 0:
            //    // delete all heard songs
            //    for (int s = 0; s < songList.Count; s++)
            //      if (songList[s].TimesPlayed > 0)
            //      {
            //        songList.Remove(songList[s]);
            //        s--;
            //      }
            //    break;
            //  case 1:
            //    // delete all well known songs
            //    if (avgPlayCount < 0.5)
            //      goto case 0;
            //    else
            //      for (int s = 0; s < songList.Count; s++)
            //        // song was played more often than average
            //        if (songList[s].TimesPlayed > avgPlayCount)
            //          // give 1x played songs a chance...
            //          if (songList[s].TimesPlayed > 1)
            //            songList.Remove(songList[s]);
            //    break;
            //  case 2:
            //    break;
            //  case 3:
            //    // only well known songs
            //    for (int s = 0; s < songList.Count; s++)
            //      // delete all rarely heard songs
            //      if (songList[s].TimesPlayed < avgPlayCount)
            //      {
            //        songList.Remove(songList[s]);
            //        s--;
            //      }

            //    // get new average playcount of remaining files
            //    if (songList.Count > 0)
            //    {
            //      int avgOfKnownSongs = 0;
            //      foreach (Song favSong in songList)
            //      {
            //        avgOfKnownSongs += favSong.TimesPlayed;
            //      }
            //      avgOfKnownSongs /= songList.Count;
            //      avgOfKnownSongs = avgOfKnownSongs > 0 ? avgOfKnownSongs : 2;

            //      int songListCount = songList.Count;
            //      for (int s = 0; s < songListCount; s++)
            //        if (songList[s].TimesPlayed < avgOfKnownSongs)
            //        {
            //          songList.Remove(songList[s]);
            //          songListCount = songList.Count;
            //          s--;
            //        }
            //    }
            //    //songList.Sort(CompareSongsByTimesPlayed);
            //    break;
            //}

            // check if there are still enough songs
            if (songList.Count < _maxScrobbledSongsPerArtist)
                return false;

            PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();

            int randomPosition;

            while (songsAdded < _maxScrobbledSongsPerArtist)
            {
                if (_preferCountForTracks == 3)
                    randomPosition = rand.Next(0, songList.Count / 2);
                else
                    randomPosition = rand.Next(0, songList.Count - 1);

                Song refSong = new Song();

                refSong = songList[randomPosition];

                //        Log.Debug("GUIMusicPlaylist: ScrobbleSimilarArtists tries to add this song - {0}", refSong.ToShortString());

                if (AddRandomSongToPlaylist(ref refSong, ref vidr))
                {
                    songsAdded++;
                    _totalScrobbledSongs++;
                }

                j++;

                if (j > songList.Count * 5)
                    break;
            }
            // _maxScrobbledSongsPerArtist are inserted      
            return true;
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
                        MusicTag tag = null;
                        bool dirtyTag = false;
                        if (item.MusicTag != null)
                        {
                            tag = (MusicTag)item.MusicTag;

                            if (tag.Title == ("unknown") || tag.Title.IndexOf("unknown") > 0 || tag.Title == string.Empty)
                                dirtyTag = true;
                        }
                        else
                            dirtyTag = true;

                        if (tag != null && !dirtyTag)
                        {
                            int playCount = tag.TimesPlayed;
                            string duration = MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
                            item.Label = string.Format("{0} - {1}", tag.Artist, tag.Title);
                            item.Label2 = duration;
                        }
                    }

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

    }
}