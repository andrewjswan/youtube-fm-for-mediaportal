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

using YouTubePlugin.DataProvider;

namespace YouTubePlugin
{

  public class YouTubeGUIInfo : YoutubeGUIBase
  {
    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIThumbnailPanel listControl = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnPlay = null;
    [SkinControlAttribute(95)]
    protected GUIImage imgFanArt = null;
    #endregion

#region variabiles
    List<GUIListItem> relatated = new List<GUIListItem>();
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
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(updateStationLogoTimer_Elapsed);
      updateStationLogoTimer.Enabled = false;
      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      updateStationLogoTimer.Interval = 1 * 1000;
      return Load(GUIGraphicsContext.Skin + @"\youtubeinfo.xml");
    }

    void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      updateStationLogoTimer.Enabled = true; 
    }

    
    void updateStationLogoTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      imgFanArt.Visible = false;
      updateStationLogoTimer.Enabled = false;
      Log.Error(Youtube2MP.NowPlayingSong.Artist);
      if (Youtube2MP.NowPlayingSong != null)
      {
        HTBFanArt fanart = new HTBFanArt();
        string file = GetFanArtImage(Youtube2MP.NowPlayingSong.Artist);
        if (!File.Exists(file))
        {
          fanart.Search(Youtube2MP.NowPlayingSong.Artist);
          if (fanart.ImageUrls.Count > 0)
          {
            Client.DownloadFile(fanart.ImageUrls[0].Url, file);
            Log.Error(fanart.ImageUrls[0].Url);
            GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
            imgFanArt.Visible = true;
          }
          else
          {
            //imgFanArt.Visible = false;
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", file);
          imgFanArt.Visible = true;
        }
        LoadRelatated();
      }
      else
      {
        //imgFanArt.Visible = false;
      }
    
    }

    private void LoadRelatated()
    {
      if (Youtube2MP.NowPlayingEntry.RelatedVideosUri != null)
      {
        GUIControl.ClearControl(GetID, listControl.GetID);
        relatated.Clear();
        YouTubeQuery query = new YouTubeQuery(Youtube2MP.NowPlayingEntry.RelatedVideosUri.Content);
        YouTubeFeed vidr = Youtube2MP.service.Query(query);
        if (vidr.Entries.Count > 0)
        {
          addVideos(vidr, query);
        }
      }
    }

    protected override void OnPageLoad()
    {
      foreach (GUIListItem item in relatated)
      {
        listControl.Add(item);
      }
      //updateStationLogoTimer.Enabled = true;
     
      //if (Youtube2MP.NowPlayingEntry != null)
      //{
      //  if (File.Exists(GetFanArtImage(Youtube2MP.NowPlayingSong.Artist)))
      //  {
      //    GUIPropertyManager.SetProperty("#Youtube.fm.NowPlaying.Video.FanArt", GetFanArtImage(Youtube2MP.NowPlayingSong.Artist));
      //    imgFanArt.Visible = true;
      //    updateStationLogoTimer.Enabled = false;
      //  }
      //  else
      //  {
      //    imgFanArt.Visible = false;
      //    updateStationLogoTimer.Enabled = true;
      //  }


      //}
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
          DoPlay(listControl.SelectedListItem.MusicTag as YouTubeEntry, false);
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
        relatated.Add(item);
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
