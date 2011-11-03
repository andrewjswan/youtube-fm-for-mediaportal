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
    public ItemType ItemType { get; set; }

    public string ItemTypeName
    {
      get
      {
        switch (ItemType)
        {
          case ItemType.Item:
            return Translation.Items;
          case ItemType.Video:
            return Translation.Videos;
          case ItemType.Artist:
            return Translation.Artists;
        }
        return " ";
      }
    }

    public GenericListItemCollections()
    {
      Items = new List<GenericListItem>();
      ItemType = ItemType.Item;
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
