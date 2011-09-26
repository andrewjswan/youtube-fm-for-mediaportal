﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lastfm.Services;

namespace YouTubePlugin.Class.SiteItems
{
  class LastFmUser : ISiteItem
  {
    public LastFmUser()
    {
      Name = "LastFmUser";
      ConfigControl = new LastFmUserControl();  
    }

    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((LastFmUserControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      User user = new User(Youtube2MP._settings.LastFmUser, Youtube2MP.LastFmProfile.Session);
      TopTrack[] tracks = user.GetTopTracks();
      foreach (TopTrack topTrack in tracks)
      {
        SiteItemEntry newentry = new SiteItemEntry();
        VideoItem videoItem = new VideoItem();
        newentry.Provider = videoItem.Name;
        newentry.Title = topTrack.Item.ToString();
        res.Items.Add(new GenericListItem()
        {
          IsFolder = false,
          Title = newentry.Title,
          Tag = newentry
        });
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
      {
        IsFolder = true,
        Title = entry.Title,
        Tag = entry
      });
      return res;
    }

    #endregion
  }
}