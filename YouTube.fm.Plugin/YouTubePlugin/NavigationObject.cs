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

    public NavigationObject(GUIListControl control, string tit, int pos, View curview)
    {
      Items = new List<GUIListItem>();
      GetItems(control, tit, pos, curview);
    }

    public void GetItems(GUIListControl control, string tit, int pos, View curview)
    {
      Title = tit;
      Position = pos;
      CurrentView = curview;
      Items = control.ListItems.GetRange(0, control.ListItems.Count);
    }

    private View curentView;

    public View CurrentView
    {
      get { return curentView; }
      set { curentView = value; }
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
