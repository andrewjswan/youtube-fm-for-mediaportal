using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using Action = System.Action;

namespace YouTubePlugin
{
  public class YouTubeGUIOSD : GUIVideoOSD
  {
    public override int GetID { get { return 29055; } set { } }

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
        YouTubeGUIVideoFullscreen videoWindow = (YouTubeGUIVideoFullscreen)GUIWindowManager.GetWindow(29054);
        videoWindow.OnAction(new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_OSD, 0, 0));
        videoWindow.OnAction(action);
      }
      else
      {
        base.OnAction(action);
      }
    }
  }
}
