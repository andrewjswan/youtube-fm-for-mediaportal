using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.GData.YouTube;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Util;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Database;
using Action = System.Action;

namespace YouTubePlugin
{
  public class YouTubeGUIVideoFullscreen : GUIVideoFullscreen
  {
    public override string GetModuleName()
    {
      return  "Youtube.Fm Fullscreen";
    }

    public override int GetID { get { return 29054; } set { } }

    public override bool Load(string _skinFileName)
    {
      return base.Load(GUIGraphicsContext.Skin + @"\youtubeFullScreen.xml");
    }

    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_VOLUME_UP ||
          action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_VOLUME_DOWN ||
          action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_VOLUME_MUTE)
      {
        // MediaPortal core sends this message to the Fullscreenwindow, we need to do it ourselves to make the Volume OSD show up
        base.OnAction(new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_VOLUME, 0,
                                                         0));
        return;
      }
      else if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU)
      {
        OnShowContextMenu();
        return;
      }
      else
      {
        MediaPortal.GUI.Library.Action translatedAction = new MediaPortal.GUI.Library.Action();
        if (ActionTranslator.GetAction((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO, action.m_key,
                                       ref translatedAction))
        {
          if (translatedAction.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_OSD)
          {
            GUIPropertyManager.SetProperty("#Youtube.fm.FullScreen.ShowTitle", "false");
            base.OnAction(translatedAction);
            if (GUIWindowManager.VisibleOsd == GUIWindow.Window.WINDOW_OSD)
            {
              GUIWindowManager.VisibleOsd = (GUIWindow.Window) 29055;
            }
            return;
          }
          if (translatedAction.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_ASPECT_RATIO)
          {
            base.OnAction(translatedAction);
            return;
          }
        }
      }
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

    public override bool OnMessage(GUIMessage message)
    {
      bool result = base.OnMessage(message);

      if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
      {
        YouTubeGUIOSD osd = (YouTubeGUIOSD)GUIWindowManager.GetWindow(29055);
        typeof(GUIVideoFullscreen).InvokeMember("_osdWindow", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField, null, this, new object[] { osd });
      }

      return result;
    }

    protected override void OnShowContextMenu()
    {
      if (Youtube2MP.NowPlayingEntry == null)
      {
        base.OnShowContextMenu();
        return;
      }
      YouTubeEntry videoEntry = Youtube2MP.NowPlayingEntry;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(Translation.ContextMenu); // menu
      dlg.AddLocalizedString(941);
      dlg.Add(Translation.Info);
      if (Youtube2MP.service.Credentials != null)
      {
        dlg.Add(Translation.AddFavorites);
      }
      dlg.Add(Translation.DownloadVideo);
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      if (dlg.SelectedLabelText == Translation.Info)
      {
        YoutubeGuiInfoEx scr = (YoutubeGuiInfoEx)GUIWindowManager.GetWindow(29053);
        scr.YouTubeEntry = videoEntry;
        GUIWindowManager.ActivateWindow(29053);
      }
      if (dlg.SelectedLabelText == Translation.AddFavorites)
      {
        try
        {
          Youtube2MP.service.Insert(new Uri(YouTubeQuery.CreateFavoritesUri(null)), videoEntry);
        }
        catch (Exception)
        {
          Youtube2MP.Err_message(Translation.WrongRequestWrongUser);
        }
      }
      
      if (dlg.SelectedLabelText == Translation.DownloadVideo)
      {
        LocalFileStruct fil = Youtube2MP._settings.LocalFile.Get(Youtube2MP.GetVideoId(videoEntry));
        if (fil != null && File.Exists(fil.LocalFile))
        {
          Youtube2MP.Err_message(Translation.ItemAlreadyDownloaded);
        }
        else
        {
          if (Youtube2MP.VideoDownloader.IsBusy)
          {
           Youtube2MP.Err_message(Translation.AnotherDonwnloadProgress);
          }
          else
          {
            VideoInfo inf = Youtube2MP.SelectQuality(videoEntry);
            string streamurl = Youtube2MP.StreamPlaybackUrl(videoEntry, inf);
            Youtube2MP.VideoDownloader.AsyncDownload(streamurl,
                                          Youtube2MP._settings.DownloadFolder + "\\" +
                                          Utils.MakeFileName(Utils.GetFilename(videoEntry.Title.Text + "{" +
                                                                               Youtube2MP.GetVideoId(videoEntry) + "}")) +
                                          Path.GetExtension(streamurl) + ".___");
            GUIPropertyManager.SetProperty("#Youtube.fm.IsDownloading", "true");
            GUIPropertyManager.SetProperty("#Youtube.fm.Download.Progress", "0");
            GUIPropertyManager.SetProperty("#Youtube.fm.Download.Item", videoEntry.Title.Text);
            DatabaseProvider.InstanInstance.Save(videoEntry);
            Youtube2MP.VideoDownloader.Entry = videoEntry;
          }
        }
      }

      if (dlg.SelectedId == 941)
      {
        ShowAspectRatioMenu();
      }
    }

    private void ShowAspectRatioMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio
      dlg.AddLocalizedString(942); // Stretch
      dlg.AddLocalizedString(943); // Normal
      dlg.AddLocalizedString(944); // Original
      dlg.AddLocalizedString(945); // Letterbox
      dlg.AddLocalizedString(946); // Smart stretch
      dlg.AddLocalizedString(947); // Zoom
      dlg.AddLocalizedString(1190); //14:9

      // set the focus to currently used mode
      dlg.SelectedLabel = dlg.IndexOfItem(MediaPortal.Util.Utils.GetAspectRatioLocalizedString(GUIGraphicsContext.ARType));
      // show dialog and wait for result
//      _IsDialogVisible = true;
      dlg.DoModal(GetID);
//      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      //_timeStatusShowTime = (DateTime.Now.Ticks / 10000);

      //string strStatus = "";

      GUIGraphicsContext.ARType = MediaPortal.Util.Utils.GetAspectRatioByLangID(dlg.SelectedId);
      //strStatus = GUILocalizeStrings.Get(dlg.SelectedId);

      //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0,
      //                                null);
      //msg.Label = strStatus;
      //OnMessage(msg);
    }
  }
}
