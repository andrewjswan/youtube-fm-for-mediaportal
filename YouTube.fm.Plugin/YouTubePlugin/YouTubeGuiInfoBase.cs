using System;
using System.Collections.Generic;
using System.IO;
using Google.GData.YouTube;
using Google.YouTube;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.Class.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace YouTubePlugin
{
  public class YouTubeGuiInfoBase : YoutubeGUIBase
  {
    #region skin connection
    [SkinControl(50)]
    protected GUIFacadeControl listControl = null;
    [SkinControlAttribute(166)]
    protected GUIListControl listsimilar = null;
    [SkinControlAttribute(95)]
    protected GUIImage imgFanArt = null;

    #endregion

    protected List<GUIListItem> relatated = new List<GUIListItem>();
    protected List<GUIListItem> similar = new List<GUIListItem>();

    protected static readonly object locker = new object();
    protected static readonly object similarlocker = new object();

    protected void addVideos(YouTubeFeed videos, YouTubeQuery qu)
    {
      downloaQueue.Clear();
      foreach (YouTubeEntry entry in videos.Entries)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = entry.Title.Text; //ae.Entry.Author.Name + " - " + ae.Entry.Title.Content;
        item.Label2 = "";
        item.IsFolder = false;

        try
        {
          item.Duration = Convert.ToInt32(entry.Duration.Seconds, 10);
          if (entry.Rating != null)
            item.Rating = (float)entry.Rating.Average;
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
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += item_OnRetrieveArt;
          DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
          //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
        }
        item.MusicTag = entry;
        relatated.Add(item);
      }
      //OnDownloadTimedEvent(null, null);
    }

    void item_OnRetrieveArt(GUIListItem item)
    {
      YouTubeEntry entry = item.MusicTag as YouTubeEntry;
      string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
      if (File.Exists(imageFile))
      {
        item.ThumbnailImage = imageFile;
        item.IconImage = imageFile;
        item.IconImageBig = imageFile;
      }
    }

    protected void FillRelatedList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listControl == null)
        return;
      GUIControl.ClearControl(GetID, listControl.GetID);
      if (relatated == null || relatated.Count < 1) return;
      foreach (GUIListItem item in relatated)
      {
        listControl.Add(item);
        item.RefreshCoverArt();
      }
      listControl.SelectedListItemIndex = 0;
      OnDownloadTimedEvent(null, null);
      GUIPropertyManager.SetProperty("#itemcount", listControl.Count.ToString());
    }

    protected void FillSimilarList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listsimilar == null)
        return;
      GUIControl.ClearControl(GetID, listsimilar.GetID);
      if (similar == null || similar.Count < 1) return;
      foreach (GUIListItem item in similar)
      {
        listsimilar.Add(item);
      }
      listsimilar.SelectedListItemIndex = 0;
    }

    protected void LoadRelatated(YouTubeEntry entry)
    {
      //Youtube2MP.getIDSimple(Youtube2MP.NowPlayingEntry.Id.AbsoluteUri));
      //GUIControl.ClearControl(GetID, listControl.GetID);
      string relatatedUrl = string.Format("http://gdata.youtube.com/feeds/api/videos/{0}/related",
                                         Youtube2MP.GetVideoId(entry));
      relatated.Clear();
      YouTubeQuery query = new YouTubeQuery(relatatedUrl);
      YouTubeFeed vidr = Youtube2MP.service.Query(query);
      // time to time this query return nothing, maybe the quata limit ..
      if (vidr.Entries.Count == 0)
      {
        vidr = Youtube2MP.service.Query(query);
      }
      if (vidr.Entries.Count > 0)
      {
        addVideos(vidr, query);
      }
      if (listControl != null)
      {
        FillRelatedList();
      }
    }


    protected ArtistItem LoadSimilarArtists(YouTubeEntry entry)
    {
      //if (listsimilar != null)
      //{
      similar.Clear();
      ArtistItem artistItem = GetArtist(entry);

      if (!string.IsNullOrEmpty(artistItem.Id))
      {
        List<ArtistItem> items = ArtistManager.Instance.Grabber.GetSimilarArtists(artistItem.Id);
        foreach (ArtistItem aitem in items)
        {
          GUIListItem item = new GUIListItem();
          // and add station name & bitrate
          item.Label = aitem.Name;
          item.Label2 = "";
          item.IsFolder = true;

          string imageFile = GetLocalImageFileName(aitem.Img_url);
          if (File.Exists(imageFile))
          {
            item.ThumbnailImage = imageFile;
            item.IconImage = imageFile;
            item.IconImageBig = imageFile;
          }
          else
          {
            MediaPortal.Util.Utils.SetDefaultIcons(item);
            DownloadImage(aitem.Img_url, item);
          }
          item.MusicTag = aitem;
          similar.Add(item);
        }
        OnDownloadTimedEvent(null, null);
      }
      if (listsimilar != null)
      {
        FillSimilarList();
      }
      //}
      return artistItem;
    }

    protected void ClearLists()
    {
      relatated.Clear();
      similar.Clear();

      if (GUIWindowManager.ActiveWindow != GetID)
        return;

      lock (locker)
      {
        if (listControl != null)
        {
          GUIControl.ClearControl(GetID, listControl.GetID);
        }
        if (listsimilar != null)
        {
          GUIControl.ClearControl(GetID, listsimilar.GetID);
        }
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem selectedItem = listControl.SelectedListItem;
      YouTubeEntry videoEntry = selectedItem.MusicTag as YouTubeEntry;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(498); // menu
      if (Youtube2MP.player.CurrentSong > -1 || Youtube2MP.temp_player.CurrentSong > -1)
        dlg.Add(Translation.PlayNext);
      dlg.Add(Translation.ShowPreviousWindow);
      dlg.Add(Translation.Fullscreen);
      if (videoEntry != null)
      {
        dlg.Add(Translation.AddPlaylist);
        dlg.Add(Translation.AddAllPlaylist);
        dlg.Add(Translation.Info);
        if (Youtube2MP.service.Credentials != null)
        {
          dlg.Add(Translation.AddFavorites);
          dlg.Add(Translation.AddWatchLater);
        }
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      if (dlg.SelectedLabelText == Translation.ShowPreviousWindow)
      {
        GUIWindowManager.ShowPreviousWindow();
      }
      else if (dlg.SelectedLabelText == Translation.Fullscreen)
      {
        g_Player.ShowFullScreenWindow();
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
            (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
              dlgProgress.SetLine(2, i.ToString() + "/" + listControl.Count.ToString());
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
          Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreateFavoritesUri(null)), videoEntry);
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
      else if (dlg.SelectedLabelText == Translation.Info)
      {
        YoutubeGuiInfoEx scr = (YoutubeGuiInfoEx)GUIWindowManager.GetWindow(29053);
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
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if(listControl!=null){
        switch (SkinUtil.GetDefine(_windowXmlFileName, "#viewmode"))
        {
          case "":
          case "largeicons":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
            }
            break;
          case "album":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.AlbumView;
            }
            break;
          case "filmstrip":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
            }
            break;
          case "list":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.List;
            }
            break;
          case "playlist":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.Playlist;
            }
            break;
          case "smallicons":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.SmallIcons;
            }
            break;
          case "coverflow":
            {
              listControl.CurrentLayout = GUIFacadeControl.Layout.CoverFlow;
            }
            break;
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listControl && actionType == Action.ActionType.ACTION_SELECT_ITEM && listControl.SelectedListItem != null)
      {
        DoPlay(listControl.SelectedListItem.MusicTag as YouTubeEntry, true, null);
      }
      else if (control == listsimilar && actionType == Action.ActionType.ACTION_SELECT_ITEM && listsimilar.SelectedListItem != null)
      {
        ArtistItem artistItem = listsimilar.SelectedListItem.MusicTag as ArtistItem;
        MessageGUI.Item = artistItem;
        GUIWindowManager.ActivateWindow(29050);
      }
      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_NEXT_ITEM || action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_NEXT_CHAPTER)
      {
        if (Youtube2MP.player.CurrentSong > -1)
        {
          Youtube2MP.player.PlayNext();
          return;
        }
        if (Youtube2MP.temp_player.CurrentSong > -1)
        {
          Youtube2MP.temp_player.PlayNext();
          return;
        }
      }

      if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PREV_ITEM || action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PREV_CHAPTER)
      {
        if (Youtube2MP.player.CurrentSong > -1)
        {
          Youtube2MP.player.PlayPrevious();
          return;
        }
        if (Youtube2MP.temp_player.CurrentSong > -1)
        {
          Youtube2MP.temp_player.PlayPrevious();
          return;
        }
      }
      base.OnAction(action);
    }


  }
}
