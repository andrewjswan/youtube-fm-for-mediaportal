using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
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
        base.OnAction(new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_VOLUME, 0, 0));
        return;
      }
      else
      {
        MediaPortal.GUI.Library.Action translatedAction = new MediaPortal.GUI.Library.Action();
        if (ActionTranslator.GetAction((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO, action.m_key, ref translatedAction))
        {
          if (translatedAction.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_OSD)
          {
            base.OnAction(translatedAction);
            if (GUIWindowManager.VisibleOsd == GUIWindow.Window.WINDOW_OSD)
            {
              GUIWindowManager.VisibleOsd = (GUIWindow.Window)29055;
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
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool result = base.OnMessage(message);

      if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
      {
        GUIVideoOSD osd = (GUIVideoOSD)GUIWindowManager.GetWindow(29055);
        typeof(GUIVideoFullscreen).InvokeMember("_osdWindow", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField, null, this, new object[] { osd });
      }

      return result;
    }
  }
}
