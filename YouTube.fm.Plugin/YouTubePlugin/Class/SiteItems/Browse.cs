using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public class Browse : ISiteItem
  {
    private Settings _settings = new Settings();
    public Browse()
    {
      Name = "Browse";
    }
    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      if (string.IsNullOrEmpty(entry.GetValue("level")))
      {
        foreach (KeyValuePair<string, string> keyValuePair in _settings.Regions)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("region", keyValuePair.Key);
          newentry.SetValue("level", "1");
          newentry.Title = keyValuePair.Key;
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = keyValuePair.Key,
                                         IsFolder = true,
                                         Tag = newentry
                                       };
          res.Items.Add(listItem);
        }
      }
      else if (entry.GetValue("level") == "1")
      {
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("region", entry.GetValue("region"));
          newentry.SetValue("level", "2");
          newentry.SetValue("hd", "false");
          newentry.Title = "All";
          GenericListItem listItem = new GenericListItem()
          {
            Title = "All",
            IsFolder = true,
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("region", entry.GetValue("region"));
          newentry.SetValue("level", "2");
          newentry.SetValue("hd", "true");
          newentry.Title = "HD";
          GenericListItem listItem = new GenericListItem()
          {
            Title = "HD",
            IsFolder = true,
            Tag = newentry
          };
          res.Items.Add(listItem);
        }

      }
      else if (entry.GetValue("level") == "2")
      {
        for (int i = 0; i < _settings.Cats.Count; i++)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = i > 4 ? "Standard feed" : this.Name;
          newentry.SetValue("region", entry.GetValue("region"));
          newentry.SetValue("hd", entry.GetValue("hd"));
          newentry.SetValue("level", "3");
          newentry.SetValue("feedint", i.ToString());
          newentry.Title = _settings.Cats[i];
          GenericListItem listItem = new GenericListItem()
          {
            Title = _settings.Cats[i],
            IsFolder = true,
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
      }
      else if (entry.GetValue("level") == "3")
      {
        for (int i = 0; i < _settings.TimeList.Count; i++)
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = "Standard feed";
          newentry.SetValue("region", entry.GetValue("region"));
          newentry.SetValue("hd", entry.GetValue("hd"));
          newentry.SetValue("level", "4");
          newentry.SetValue("feedint", entry.GetValue("feedint"));
          newentry.SetValue("time", _settings.TimeList[i]);
          newentry.Title = _settings.TimeList[i];
          GenericListItem listItem = new GenericListItem()
          {
            Title = _settings.TimeList[i],
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
      entry.Title = Name;
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      GenericListItem listItem = new GenericListItem()
      {
        Title = entry.Title,
        IsFolder = true,
        //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
        Tag = entry
      };
      res.Items.Add(listItem);
      return res;
    }

    #endregion
  }
}
