﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;

namespace YouTubePlugin.Class.SiteItems
{
  public class FavoritesVideos:ISiteItem
  {
    public FavoritesVideos()
    {
      Name = "User favorites videos";
      ConfigControl = new UserVideosControl();
    }

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((UserVideosControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.FolderType = 1;
      string user = entry.GetValue("user");
      user = string.IsNullOrEmpty(user) ? "default" : user;
      YouTubeQuery query =
        new YouTubeQuery(string.Format("http://gdata.youtube.com/feeds/api/users/{0}/favorites",user));
      query.NumberToRetrieve = Youtube2MP.ITEM_IN_LIST;
      query.StartIndex = entry.StartItem;
      if (entry.StartItem > 1)
        res.Paged = true;
      YouTubeFeed videos = Youtube2MP.service.Query(query);
      res.Title = videos.Title.Text;
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        res.Items.Add(Youtube2MP.YouTubeEntry2ListItem(youTubeEntry));
      }
      res.Add(Youtube2MP.GetPager(entry, videos));
      res.ItemType = ItemType.Video;
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry itemEntry)
    {
      GenericListItemCollections res = new GenericListItemCollections();

      GenericListItem listItem = new GenericListItem()
      {
        Title = itemEntry.Title,
        IsFolder = true,
        //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
        Tag = itemEntry
      };
      res.Items.Add(listItem);
      return res;
    }
  }
}
