using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Google.GData.YouTube;
using YouTubePlugin.Class.Database;

namespace YouTubePlugin.Class.SiteItems
{
  public class Disco : ISiteItem
  {
    private static GenericListItemCollections items = new GenericListItemCollections();
    public Disco()
    {
      Name = "Disco";
      ConfigControl = new DiscoControl();
    }
    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((DiscoControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      if (items.Items.Count > 0)
        return items;
      Dictionary<string, GenericListItem> list1 = new Dictionary<string, GenericListItem>();

      GenericListItemCollections res = new GenericListItemCollections();
      string query = YouTubeQuery.MostViewedVideo;
      res.Title = entry.Title;
      if (!string.IsNullOrEmpty(entry.GetValue("region")))
      {
        string reg = Youtube2MP._settings.Regions[entry.GetValue("region")];
        if (!string.IsNullOrEmpty(reg))
          query = query.Replace("standardfeeds", "standardfeeds/" + reg);
      }
      else
      {
        if (Youtube2MP._settings.Regions.ContainsValue(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper()))
        {
          query = query.Replace("standardfeeds", "standardfeeds/" + Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper());
        }
      }
      query += "_Music";
      YouTubeQuery tubeQuery = new YouTubeQuery(query);
      tubeQuery.NumberToRetrieve = 50;
      tubeQuery.SafeSearch = YouTubeQuery.SafeSearchValues.None;
     
      YouTubeFeed videos = Youtube2MP.service.Query(tubeQuery);
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        list1.Add(Youtube2MP.GetVideoId(youTubeEntry), Youtube2MP.YouTubeEntry2ListItem(youTubeEntry));
      }
      GenericListItemCollections popular = DatabaseProvider.InstanInstance.GetTopPlayed(2);
      foreach (GenericListItem genericListItem in popular.Items)
      {
        YouTubeEntry youTubeEntry = genericListItem.Tag as YouTubeEntry;
        if (youTubeEntry != null && !list1.ContainsKey(Youtube2MP.GetVideoId(youTubeEntry)))
        {
          list1.Add(Youtube2MP.GetVideoId(youTubeEntry), genericListItem);
        }
      }
      Random random = new Random();
      foreach (GenericListItem genericListItem in list1.Values)
      {
        if (res.Items.Count == 0)
        {
          res.Items.Add(genericListItem);
        }
        else
        {
          res.Items.Insert(random.Next(res.Items.Count - 1), genericListItem);
        }
      }
      items = res;
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();

      GenericListItem listItem = new GenericListItem()
      {
        Title = Name,
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
