using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.Class.Database;

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
        {
          res.Title = "Artists";
          SiteItemEntry newentry1 = new SiteItemEntry();
          newentry1.Provider = this.Name;
          newentry1.SetValue("letter", "true");
          newentry1.SetValue("special", "1");
          newentry1.Title = Translation.PlayedArtists;
          GenericListItem listItem1 = new GenericListItem()
                                        {
                                          Title = Translation.PlayedArtists,
                                          IsFolder = true,
                                          Tag = newentry1
                                        };
          res.Items.Add(listItem1);
        }

        {
          res.Title = "Artists";
          SiteItemEntry newentry1 = new SiteItemEntry();
          newentry1.Provider = this.Name;
          newentry1.SetValue("letter", "true");
          newentry1.SetValue("special", "2");
          newentry1.Title = Translation.ByTags;
          GenericListItem listItem1 = new GenericListItem()
          {
            Title = Translation.ByTags,
            IsFolder = true,
            Tag = newentry1
          };
          res.Items.Add(listItem1);
        }

        foreach (string letter in ArtistManager.Instance.GetArtistsLetters())
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "true");
          newentry.SetValue("special", "false");
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
      if (entry.GetValue("letter") == "true" && entry.GetValue("special") == "false")
      {
        res.Title = "Artists/Letter/" + entry.Title;
        foreach (ArtistItem artistItem in ArtistManager.Instance.GetArtists(entry.Title))
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", artistItem.Id);
          newentry.SetValue("name", artistItem.Name);
          res.ItemType = ItemType.Artist;
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = artistItem.Name,
                                         LogoUrl =
                                           string.IsNullOrEmpty(artistItem.Img_url.Trim()) ? "@" : artistItem.Img_url,
                                         IsFolder = true,
                                         DefaultImage = "defaultArtistBig.png",
                                         Tag = newentry
                                       };
          res.Items.Add(listItem);
        }
      }
      if (entry.GetValue("special") == "1")
      {
        res.Title = "Artists/" + Translation.PlayedArtists;
        foreach (
          ArtistItem artistItem in
            ArtistManager.Instance.GetArtistsByIds(DatabaseProvider.InstanInstance.GetPlayedArtistIds(1)))
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", artistItem.Id);
          newentry.SetValue("name", artistItem.Name);
          res.ItemType = ItemType.Artist;
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = artistItem.Name,
                                         LogoUrl =
                                           string.IsNullOrEmpty(artistItem.Img_url.Trim()) ? "@" : artistItem.Img_url,
                                         IsFolder = true,
                                         DefaultImage = "defaultArtistBig.png",
                                         Tag = newentry
                                       };
          res.Items.Add(listItem);
        }
      }

      if (entry.GetValue("special") == "2")
      {
        res.Title = "Artists/" + Translation.ByTags;
        foreach (string[] strings in ArtistManager.Instance.GetTags())
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "true");
          newentry.SetValue("special", "3");
          newentry.SetValue("tag",strings[0]);
          res.ItemType = ItemType.Item;
          GenericListItem listItem = new GenericListItem()
          {
            Title = strings[0],
            IsFolder = true,
            Title2 = strings[1],
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
      }

      if (entry.GetValue("special") == "3")
      {
        res.Title = "Artists/" + Translation.ByTags + "/" + entry.Title;
        foreach (ArtistItem artistItem in ArtistManager.Instance.GetArtistsByTag(entry.GetValue("tag")))
        {
          SiteItemEntry newentry = new SiteItemEntry();
          newentry.Provider = this.Name;
          newentry.SetValue("letter", "false");
          newentry.SetValue("id", artistItem.Id);
          newentry.SetValue("name", artistItem.Name);
          res.ItemType = ItemType.Artist;
          GenericListItem listItem = new GenericListItem()
          {
            Title = artistItem.Name,
            LogoUrl =
              string.IsNullOrEmpty(artistItem.Img_url.Trim()) ? "@" : artistItem.Img_url,
            IsFolder = true,
            DefaultImage = "defaultArtistBig.png",
            Tag = newentry
          };
          res.Items.Add(listItem);
        }
      }


      if (entry.GetValue("letter") == "false")
      {
        //res = ArtistManager.Instance.Grabber.GetArtistVideosIds(entry.GetValue("id"));
        res.ItemType = ItemType.Video;
        string user = ArtistManager.Instance.Grabber.GetArtistUser(entry.GetValue("id"));
        GenericListItemCollections resart = ArtistManager.Instance.Grabber.GetArtistVideosIds(entry.GetValue("id"));
        YouTubeFeed videos = null;
        if (!string.IsNullOrEmpty(user))
        {
          YouTubeQuery query =
            new YouTubeQuery(string.Format("http://gdata.youtube.com/feeds/api/users/{0}/uploads", user));
          query.NumberToRetrieve = 50;
          videos = Youtube2MP.service.Query(query);
        }

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
        res.Title = "Artists/" + ArtistManager.Instance.GetArtistsById(entry.GetValue("id")).Name;
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
      if (videos == null)
        return null;
      foreach (YouTubeEntry youTubeEntry in videos.Entries)
      {
        if (Youtube2MP.GetVideoId(youTubeEntry) == videoId)
          return youTubeEntry;
      }
      return null;
    }


  }
}
