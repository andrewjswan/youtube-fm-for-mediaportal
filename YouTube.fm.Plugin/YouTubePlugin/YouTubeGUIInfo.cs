using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Localisation;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Playlists;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;

namespace YouTubePlugin
{

  public class YouTubeGUIInfo : YoutubeGUIBase
  {
    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIThumbnailPanel listControl = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnPlay = null;

    #endregion

    public override int GetID
    {
      get
      {
        return 29052;
      }

      set
      {
      }
    }

    public YouTubeGUIInfo()
    {

    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\youtubeinfo.xml");
    }

    protected override void OnPageLoad()
    {
      if (Youtube2MP.NowPlayingEntry != null)
      {
        if (Youtube2MP.NowPlayingEntry.RelatedVideosUri != null)
        {
          YouTubeQuery query = new YouTubeQuery(Youtube2MP.NowPlayingEntry.RelatedVideosUri.Content);
          YouTubeFeed vidr = Youtube2MP.service.Query(query);
          if (vidr.Entries.Count > 0)
          {
            addVideos(vidr,  query);
          }
        }
      }
      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
    }


    void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      //      throw new Exception("The method or operation is not implemented.");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listControl)
      {
        // execute only for enter keys
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          DoPlay(listControl.SelectedListItem.MusicTag as YouTubeEntry);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    void addVideos(YouTubeFeed videos, YouTubeQuery qu)
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
          item.Rating = (float)entry.Rating.Average;
        }
        catch
        {

        }

        string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
        if (File.Exists(imageFile))
        {
          item.ThumbnailImage = imageFile;
          item.IconImage = "defaultVideoBig.png";
          item.IconImageBig = imageFile;
        }
        else
        {
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(item_OnRetrieveArt);
          //DownloadImage(GetBestUrl(entry.Media.Thumbnails), item);
        }
        item.MusicTag = entry;
        listControl.Add(item);
      }
    }

    void item_OnRetrieveArt(GUIListItem item)
    {
      YouTubeEntry entry = item.MusicTag as YouTubeEntry;
      string imageFile = GetLocalImageFileName(GetBestUrl(entry.Media.Thumbnails));
      if (!File.Exists(imageFile))
      {
        WebClient client = new WebClient();
        client.DownloadFile(new Uri(GetBestUrl(entry.Media.Thumbnails)), imageFile);
      }
      item.ThumbnailImage = imageFile;
      item.IconImage = "defaultVideoBig.png";
      item.IconImageBig = imageFile;
    }

  }
}
