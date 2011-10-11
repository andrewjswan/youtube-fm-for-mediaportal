using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.GData.YouTube;
using MediaPortal.GUI.Library;
using YouTubePlugin.Class.Artist;

namespace YouTubePlugin
{
  public class YouTubeGuiInfoBase : YoutubeGUIBase
  {
    #region skin connection
    [SkinControl(50)]
    protected GUIThumbnailPanel listControl = null;
    [SkinControlAttribute(166)]
    protected GUIListControl listsimilar = null;
    [SkinControlAttribute(95)]
    protected GUIImage imgFanArt = null;
    #endregion

    protected List<GUIListItem> relatated = new List<GUIListItem>();
    protected List<GUIListItem> similar = new List<GUIListItem>();

    protected static readonly object locker = new object();

    protected void addVideos(YouTubeFeed videos, YouTubeQuery qu)
    {
      foreach (YouTubeEntry entry in videos.Entries)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = entry.Title.Text; //ae.Entry.Author.Name + " - " + ae.Entry.Title.Content;
        item.Label2 = "";
        item.IsFolder = false;

        try
        {
          item.Duration = Convert.ToInt32(entry.Duration.Seconds, 10);
          if (entry.Rating != null)
            item.Rating = (float)entry.Rating.Average;
        }
        catch
        {

        }

        string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
        if (File.Exists(imageFile))
        {
          item.ThumbnailImage = imageFile;
          item.IconImage = imageFile; item.IconImageBig = imageFile;
        }
        else
        {
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += item_OnRetrieveArt;
          DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
          //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
        }
        item.MusicTag = entry;
        relatated.Add(item);
      }
      OnDownloadTimedEvent(null, null);
    }

    void item_OnRetrieveArt(GUIListItem item)
    {
      YouTubeEntry entry = item.MusicTag as YouTubeEntry;
      string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
      if (File.Exists(imageFile))
      {
        item.ThumbnailImage = imageFile;
        item.IconImage = "defaultVideoBig.png";
        item.IconImageBig = imageFile;
      }
    }

    protected void FillRelatedList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listControl == null)
        return;
      GUIControl.ClearControl(GetID, listControl.GetID);
      if (relatated == null || relatated.Count < 1) return;
      foreach (GUIListItem item in relatated)
      {
        listControl.Add(item);
      }
      listControl.SelectedListItemIndex = 0;
    }

    protected void FillSimilarList()
    {
      //if (GUIWindowManager.ActiveWindow != GetID)
      //  return;
      if (listsimilar == null)
        return;
      GUIControl.ClearControl(GetID, listsimilar.GetID);
      if (similar == null || similar.Count < 1) return;
      foreach (GUIListItem item in similar)
      {
        listsimilar.Add(item);
      }
      listsimilar.SelectedListItemIndex = 0;
    }

    protected void LoadRelatated(YouTubeEntry entry)
    {
      //Youtube2MP.getIDSimple(Youtube2MP.NowPlayingEntry.Id.AbsoluteUri));
      //GUIControl.ClearControl(GetID, listControl.GetID);
      string relatatedUrl = string.Format("http://gdata.youtube.com/feeds/api/videos/{0}/related",
                                         Youtube2MP.GetVideoId(entry));
      relatated.Clear();
      YouTubeQuery query = new YouTubeQuery(relatatedUrl);
      YouTubeFeed vidr = Youtube2MP.service.Query(query);
      if (vidr.Entries.Count > 0)
      {
        addVideos(vidr, query);
      }
      if (listControl != null)
      {
        FillRelatedList();
      }
    }


    protected ArtistItem LoadSimilarArtists(YouTubeEntry entry)
    {
      //if (listsimilar != null)
      //{
        similar.Clear();
        string vidId = Youtube2MP.GetVideoId(entry);
        ArtistItem artistItem = ArtistManager.Instance.SitesCache.GetByVideoId(vidId) != null
                                  ? ArtistManager.Instance.Grabber.GetFromVideoSite(
                                    ArtistManager.Instance.SitesCache.GetByVideoId(vidId).SIte)
                                  : ArtistManager.Instance.Grabber.GetFromVideoId(vidId);

        if (string.IsNullOrEmpty(artistItem.Id) && Youtube2MP.NowPlayingEntry.Title.Text.Contains("-"))
        {
          artistItem =
            ArtistManager.Instance.GetArtistsByName(Youtube2MP.NowPlayingEntry.Title.Text.Split('-')[0].TrimEnd());
        }

        if (!string.IsNullOrEmpty(artistItem.Id))
        {
          List<ArtistItem> items = ArtistManager.Instance.Grabber.GetSimilarArtists(artistItem.Id);
          //GUIControl.ClearControl(GetID, listsimilar.GetID);
          foreach (ArtistItem aitem in items)
          {
            GUIListItem item = new GUIListItem();
            // and add station name & bitrate
            item.Label = aitem.Name;
            item.Label2 = "";
            item.IsFolder = true;

            string imageFile = GetLocalImageFileName(aitem.Img_url);
            if (File.Exists(imageFile))
            {
              item.ThumbnailImage = imageFile;
              item.IconImage = "defaultVideoBig.png";
              item.IconImageBig = imageFile;
            }
            else
            {
              MediaPortal.Util.Utils.SetDefaultIcons(item);
              DownloadImage(aitem.Img_url, item);
              //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
            }
            item.MusicTag = aitem;
            similar.Add(item);
            //listsimilar.Add(item);
          }
          OnDownloadTimedEvent(null, null);
        }
        if (listsimilar != null)
        {
          FillSimilarList();
        }
      //}
      return artistItem;
    }

    protected void ClearLists()
    {
      relatated.Clear();
      similar.Clear();

      if (GUIWindowManager.ActiveWindow != GetID)
        return;

      lock (locker)
      {
        if (listControl != null)
        {
          GUIControl.ClearControl(GetID, listControl.GetID);
        }
        if (listsimilar != null)
        {
          GUIControl.ClearControl(GetID, listsimilar.GetID);
        }
      }
    }


  }
}
