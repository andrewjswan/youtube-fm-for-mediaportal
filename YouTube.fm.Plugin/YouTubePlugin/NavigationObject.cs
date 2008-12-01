using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace YouTubePlugin
{
  public class NavigationObject
  {
    private List<GUIListItem> items;

    public List<GUIListItem> Items
    {
      get { return items; }
      set { items = value; }
    }

    private string  title;

    public string  Title
    {
      get { return title; }
      set { title = value; }
    }

    public NavigationObject()
    {
      Items = new List<GUIListItem>();
      Title = string.Empty;
    }

    private int position;

    public int Position
    {
      get { return position; }
      set { position = value; }
    }

    public NavigationObject(GUIListControl control, string tit, int pos)
    {
      Items = new List<GUIListItem>();
      GetItems(control, tit, pos);
    }

    public void GetItems(GUIListControl control, string tit,int pos)
    {
      Title = tit;
      Position = pos;
      Items = control.ListItems.GetRange(0, control.ListItems.Count);
    }

    public void SetItems(GUIFacadeControl control)
    {
      foreach (GUIListItem item in Items)
      {
        control.Add(item);
      }
    }
  }
}
