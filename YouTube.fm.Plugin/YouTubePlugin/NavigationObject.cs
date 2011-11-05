using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace YouTubePlugin
{
  public class NavigationObject
  {
    public List<GUIListItem> Items { get; set; }

    public string Title { get; set; }
    public string ItemType { get; set; }

    public NavigationObject()
    {
      Items = new List<GUIListItem>();
      Title = string.Empty;
    }

    public int Position { get; set; }

    public NavigationObject(GUIListControl control, string tit,string itemtype, int pos, View curview)
    {
      Items = new List<GUIListItem>();
      GetItems(control, tit, itemtype, pos, curview);
    }

    public void GetItems(GUIListControl control, string tit, string itemtype, int pos, View curview)
    {
      Title = tit;
      Position = pos;
      CurrentView = curview;
      Items = control.ListItems.GetRange(0, control.ListItems.Count);
    }

    public View CurrentView { get; set; }


    public void SetItems(GUIFacadeControl control)
    {
      foreach (GUIListItem item in Items)
      {
        control.Add(item);
      }
    }
  }
}
