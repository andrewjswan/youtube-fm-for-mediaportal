﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin.Class.SiteItems
{
  class ArtistView:ISiteItem
  {
    public ArtistView()
    {
      Name = "Artists";
    }

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      //throw new NotImplementedException();
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      if (entry.GetValue("letter") == "")
      {
        foreach (string letter in ArtistManager.Instance.GetArtistsLetters())
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "true");
          newentry.Title = letter;
          GenericListItem listItem = new GenericListItem()
          {
            Title = letter,
            IsFolder = true,
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
      }
      if (entry.GetValue("letter") == "true")
      {
        foreach (ArtistItem artistItem in ArtistManager.Instance.GetArtists(entry.Title))
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", artistItem.Id);
          GenericListItem listItem = new GenericListItem()
          {
            Title = artistItem.Name,
            IsFolder = true,
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
      }
      return res;
    }

    
    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
                      {
                        IsFolder = true,
                        Title = "Aritsts",
                        Tag = new SiteItemEntry() { Provider = "Artists" }
                      });
      return res;
    }


  }
}
