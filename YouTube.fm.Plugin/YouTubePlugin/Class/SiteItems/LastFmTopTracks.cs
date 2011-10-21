using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lastfm.Services;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin.Class.SiteItems
{
  public class LastFmTopTracks : ISiteItem
  {
    public LastFmTopTracks()
    {
      Name = "LastFmTopTracks";
      ConfigControl = new LastFmTopTracksControl();
    }

    #region Implementation of ISiteItem

    public Control ConfigControl { get; set; }
    public void Configure(SiteItemEntry entry)
    {
      ((LastFmTopTracksControl)ConfigControl).SetEntry(entry);
    }

    public string Name { get; set; }
    public GenericListItemCollections GetList(SiteItemEntry entry)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      res.Title = entry.Title;
      Country country = new Country(entry.GetValue("country"), Youtube2MP.LastFmProfile.Session);
      TopTrack[] tracks = country.GetTopTracks();
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
          Tag = newentry,
          LogoUrl = ArtistManager.Instance.GetArtistsImgUrl(topTrack.Item.Artist.Name),
          DefaultImage = "defaultArtistBig.png"
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
