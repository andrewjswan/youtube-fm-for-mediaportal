using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
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
        res.Title = "Artists";
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
        res.Title = "Artists/Letter/" + entry.Title;
        foreach (ArtistItem artistItem in ArtistManager.Instance.GetArtists(entry.Title))
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", artistItem.Id);
          newentry.SetValue("name", artistItem.Name);
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = artistItem.Name,
                                         LogoUrl =
                                           string.IsNullOrEmpty(artistItem.Img_url.Trim()) ? "@" : artistItem.Img_url,
                                         IsFolder = true,
                                         Tag = newentry
                                       };
          res.Items.Add(listItem);
        }
      }
      if (entry.GetValue("letter") == "false")
      {
        //res = ArtistManager.Instance.Grabber.GetArtistVideosIds(entry.GetValue("id"));
        GenericListItemCollections resart = ArtistManager.Instance.Grabber.GetArtistVideosIds(entry.GetValue("id"));
        string search = "";
        foreach (GenericListItem genericListItem in resart.Items)
        {
          YouTubeEntry tubeEntry = genericListItem.Tag as YouTubeEntry;
          if (!Youtube2MP.GetVideoId(tubeEntry).Contains("-"))
            search += Youtube2MP.GetVideoId(tubeEntry) + "|";
        }
        res.Title = "Artists/" + entry.GetValue("name");
        YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
        query.Query = search;
        query.NumberToRetrieve = 50;

        YouTubeFeed videos = Youtube2MP.service.Query(query);

        foreach (GenericListItem genericListItem in resart.Items)
        {
          YouTubeEntry tubeEntry = genericListItem.Tag as YouTubeEntry;
          YouTubeEntry searchEntry = GetVideFromFeed(Youtube2MP.GetVideoId(tubeEntry), videos);
          if (searchEntry != null)
          {
            searchEntry.Title.Text = tubeEntry.Title.Text;
            res.Items.Add(Youtube2MP.YouTubeEntry2ListItem(searchEntry));
          }
          else
          {
            res.Items.Add(genericListItem);
          }
        }
        res.FolderType = 1;
      }
      return res;
    }


    public GenericListItemCollections HomeGetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Items.Add(new GenericListItem()
                      {
                        IsFolder = true,
                        Title = "Artists",
                        Tag = new SiteItemEntry() { Provider = "Artists" }
                      });
      return res;
    }


    YouTubeEntry GetVideFromFeed(string videoId,YouTubeFeed videos)
    {
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        if (Youtube2MP.GetVideoId(youTubeEntry) == videoId)
          return youTubeEntry;
      }
      return null;
    }


  }
}
