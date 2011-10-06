using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public class SearchHistory : ISiteItem
  {
    public SearchHistory()
    {
      Name = "Search History";
      ConfigControl = new SearchHistoryControl();
    }

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((SearchHistoryControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      int count = 0;
      int ii = 0;
      int.TryParse(entry.GetValue("items"), out ii);

      for (int i = Youtube2MP._settings.SearchHistory.Count; i > 0; i--)
      {
        count++;
        SiteItemEntry newentry = new SiteItemEntry();
        SearchVideo searchVideo = new SearchVideo();
        newentry.Provider = searchVideo.Name;
        newentry.SetValue("term", Youtube2MP._settings.SearchHistory[i - 1]);
        GenericListItem listItem = new GenericListItem()
                                     {
                                       Title = "Search result for :" + Youtube2MP._settings.SearchHistory[i - 1],
                                       IsFolder = true,
                                       //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
                                       Tag = newentry
                                     };
        res.Items.Add(listItem);
        if (count > ii)
          break;
      }
      return res;
    }

    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      if (entry.GetValue("flat") == "true")
      {
        int count = 0;
        int ii = 0;
        int.TryParse(entry.GetValue("items"), out ii);

        for (int i = Youtube2MP._settings.SearchHistory.Count; i > 0; i--)
        {
          count++;
          SiteItemEntry newentry = new SiteItemEntry();
          SearchVideo searchVideo = new SearchVideo();
          newentry.Provider = searchVideo.Name;
          newentry.SetValue("term", Youtube2MP._settings.SearchHistory[i - 1]);
          GenericListItem listItem = new GenericListItem()
          {
            Title = "Search result for :" + Youtube2MP._settings.SearchHistory[i - 1],
            IsFolder = true,
            //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
            Tag = newentry
          };
          res.Items.Add(listItem);
          if (count > ii)
            break;
        }
      }
      else
      {
        GenericListItem listItem = new GenericListItem()
        {
          Title = entry.Title,
          IsFolder = true,
          //LogoUrl = YoutubeGUIBase.GetBestUrl(youTubeEntry.Media.Thumbnails),
          Tag = entry
        };
        res.Items.Add(listItem);
      }
      return res;
    }

  }
}
