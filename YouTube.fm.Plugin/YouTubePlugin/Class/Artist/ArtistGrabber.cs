using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using HttpUtility = Google.GData.Client.HttpUtility;
using MediaGroup = Google.GData.YouTube.MediaGroup;

namespace YouTubePlugin.Class.Artist
{
  public class ArtistGrabber
  {
    public ArtistItem GetFromVideoSite(string site)
    {
      ArtistItem res=new ArtistItem();

      try
      {
        Regex regexObj = new Regex(@"/artist\?a=(?<id>.*?)&amp;.*?Artist: <span class=.link-like.>(?<name>.*?)</span>", RegexOptions.Singleline);
        Match matchResult = regexObj.Match(site);
        while (matchResult.Success)
        {
          res.Id = matchResult.Groups["id"].Value;
          res.Name = HttpUtility.HtmlDecode(matchResult.Groups["name"].Value);
          //res.Img_url = HttpUtility.HtmlDecode(matchResult.Groups["img_url"].Value);
          matchResult = matchResult.NextMatch();
        }
        GetArtistUser(res.Id);
      }
      catch (ArgumentException ex)
      {
        // Syntax error in the regular expression
      }
      return res;
    }

    public ArtistItem GetFromVideoId(string vidId)
    {
      string url = string.Format("http://www.youtube.com/watch?v={0}", vidId);
      string site = "";
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      if (ArtistManager.Instance.SitesCache.GetByUrl(url) == null)
      {
        try
        {
          site = client.DownloadString(url);
          ArtistManager.Instance.SitesCache.Add(new SiteContent() {SIte = site, Url = url});
        }
        catch (Exception exception)
        {
          site = "";
        }
      }
      else
      {
        site = ArtistManager.Instance.SitesCache.GetByUrl(url).SIte;
      }
      return GetFromVideoSite(site);
    }

    private string DownloadArtistInfo(string artist_id)
    {
      string site = "";
      WebClient client = new WebClient();
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      string url = string.Format("http://www.youtube.com/artist?a={0}", artist_id);
      if (ArtistManager.Instance.SitesCache.GetByUrl(url) == null)
      {
        client.Encoding = System.Text.Encoding.UTF8;
        site = client.DownloadString(url);
        ArtistManager.Instance.SitesCache.Add(new SiteContent() { SIte = site, ArtistId = artist_id, Url = url });
      }
      else
      {
        site = ArtistManager.Instance.SitesCache.GetByUrl(url).SIte;
      }
      return site;
    }

    public string GetArtistUser(string artist_id)
    {
      try
      {
        string site = DownloadArtistInfo(artist_id);
        Regex regexObj = new Regex("class=\"channel-details\">.*?<a href=\"/user/.*?\">(?<user>.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        string user = regexObj.Match(site).Groups["user"].Value;
        ArtistItem artistItem = ArtistManager.Instance.GetArtistsById(artist_id);
        artistItem.User = user;
        ArtistManager.Instance.Save(artistItem);
        return user;
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }

    public GenericListItemCollections GetArtistVideosIds(string artist_id)
    {
      string site = DownloadArtistInfo(artist_id);

      GenericListItemCollections res = new GenericListItemCollections();
      try
      {
        GetSimilarArtistsSite(site);
        //------------------------------
        //string img = Regex.Match(site, "<img class=\"artist-image\" src=\"(?<url>.*?)\" />", RegexOptions.Singleline).Groups["url"].Value;
        ArtistItem artistItem = ArtistManager.Instance.GetArtistsById(artist_id);
        //artistItem.Img_url = img;
        //ArtistManager.Instance.Save(artistItem);
        //----------------------------
        //Regex regexObj = new Regex("album-row.*?data-video-ids=\"(?<vid_id>.*?)\".*?<span class=\"clip\"><img src=\"(?<thumb>.*?)\".*?album-track-name\">(?<title>.*?)</span>", RegexOptions.Singleline);
        Regex regexObj = new Regex("album-row.*?data-video-ids=\"(?<vid_id>.*?)\".*?<span class=\"clip\"><img src=\"(?<thumb>.*?)\".*?album-track-duration\">(?<duration>.*?)</span>.*?album-track-name\">(?<title>.*?)</span>", RegexOptions.Singleline);
        Match matchResult = regexObj.Match(site);
        while (matchResult.Success)
        {
          YouTubeEntry youTubeEntry=new YouTubeEntry();
          
          youTubeEntry.AlternateUri = new AtomUri("http://www.youtube.com/watch?v=" + matchResult.Groups["vid_id"].Value);
          youTubeEntry.Title = new AtomTextConstruct();
          youTubeEntry.Title.Text = artistItem.Name + " - " + HttpUtility.HtmlDecode(matchResult.Groups["title"].Value);
          youTubeEntry.Media = new MediaGroup();
          youTubeEntry.Media.Description = new MediaDescription("");
          youTubeEntry.Id = new AtomId(youTubeEntry.AlternateUri.Content);
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = youTubeEntry.Title.Text,
                                         IsFolder = false,
                                         LogoUrl = "http:" + matchResult.Groups["thumb"].Value.Replace("default.jpg", "hqdefault.jpg"),
                                         Tag = youTubeEntry,
                                         Title2 = matchResult.Groups["duration"].Value,
                                         ParentTag = artistItem
                                       };
          res.Items.Add(listItem);
          //resultList.Add(matchResult.Groups["groupname"].Value);
          matchResult = matchResult.NextMatch();
        }
      }
      catch (Exception ex)
      {
        // Syntax error in the regular expression
      }
      res.ItemType = ItemType.Video;
      return res;
    }

    public List<ArtistItem> GetSimilarArtists(string artistId)
    {
      return GetSimilarArtistsSite(DownloadArtistInfo(artistId));
    }

    public List<ArtistItem> GetSimilarArtistsSite(string site)
    {
      List<ArtistItem> res = new List<ArtistItem>();

      try
      {
        Regex regexObj =
          new Regex("similar-artist\"><a href=\"/artist.a=(?<id>.*?)&amp;feature=artist\">(?<name>.*?)</a>",
                    RegexOptions.Singleline);
        Match matchResult = regexObj.Match(site);
        while (matchResult.Success)
        {
          ArtistItem item = new ArtistItem() { Id = matchResult.Groups["id"].Value, Name = HttpUtility.HtmlDecode(matchResult.Groups["name"].Value) };
          ArtistManager.Instance.AddArtist(item);
          res.Add(item);
          matchResult = matchResult.NextMatch();
        }
      }
      catch (ArgumentException ex)
      {
        // Syntax error in the regular expression

      }
      return res;
    }
  }
}
