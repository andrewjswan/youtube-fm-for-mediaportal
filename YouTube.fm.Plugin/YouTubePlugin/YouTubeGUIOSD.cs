using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Player;
using MediaPortal.Player.PostProcessing;
using Action = System.Action;

namespace YouTubePlugin
{
  public class YouTubeGUIOSD : GUIVideoOSD
  {
    public override int GetID
    {
      get { return 29055; }
      set { }
    }

    public override string GetModuleName()
    {
      return "YouTube.Fm OSD";
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\youtubeOSD.xml");
      return bResult;
    }

    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU)
      {
        YouTubeGUIVideoFullscreen videoWindow = (YouTubeGUIVideoFullscreen) GUIWindowManager.GetWindow(29054);
        videoWindow.OnAction(
          new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_OSD, 0, 0));
        videoWindow.OnAction(action);
      }
      else
      {
        base.OnAction(action);
      }
    }
    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
      {
        // following line should stay. Problems with OSD not
        // appearing are already fixed elsewhere
        GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());

        AllocResources();
        //// if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
        //ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
        //m_bSubMenuOn = false;
        //m_iActiveMenuButtonID = 0;
        //m_iActiveMenu = 0;
        //m_bNeedRefresh = false;
        //Reset();
        FocusControl(GetID, 213, 0); // set focus to play button by default when window is shown
        QueueAnimation(AnimationType.WindowOpen);
        //for (int i = (int)Controls.Panel1; i < (int)Controls.Panel2; i++)
        //{
        //  ShowControl(GetID, i);
        //}
        if (g_Player.Paused)
        {
          ToggleButton(213, true);
          // make sure play button is down (so it shows the pause symbol)
        }
        else
        {
          ToggleButton(213, false); // make sure play button is up (so it shows the play symbol)
        }
        //m_delayInterval = MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayInterval;
        //if (m_delayInterval > 0)
        //  m_subtitleDelay = MediaPortal.Player.Subtitles.SubEngine.GetInstance().Delay / m_delayInterval;
        //m_delayIntervalAudio = PostProcessingEngine.GetInstance().AudioDelayInterval;
        //if (m_delayIntervalAudio > 0)
        //  m_audioDelay = PostProcessingEngine.GetInstance().AudioDelay / m_delayIntervalAudio;
        return true;
      }
      return base.OnMessage(message);
    }

    private void FocusControl(int dwSenderId, int dwControlID, int dwParam)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, dwSenderId, dwControlID, dwParam,
                                      0, null);
      OnMessage(msg);
    }

    private void ToggleButton(int iButtonID, bool bSelected)
    {
      GUIControl pControl = (GUIControl)GetControl(iButtonID);

      if (pControl != null)
      {
        if (bSelected) // do we want the button to appear down?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
        else // or appear up?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
      }
    }
  }
}
