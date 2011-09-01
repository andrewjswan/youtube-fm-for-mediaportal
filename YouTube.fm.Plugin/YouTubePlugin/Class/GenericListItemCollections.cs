using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.Class
{
  public class GenericListItemCollections
  {
    public List<GenericListItem> Items { get; set; }
    public string Title { get; set; }
    public int FolderType { get; set; }

    public GenericListItemCollections()
    {
      Items = new List<GenericListItem>();
    }

    public void Add(GenericListItemCollections collections)
    {
      foreach (GenericListItem item in collections.Items)
      {
        Items.Add(item);
      }
    }


  }
}
