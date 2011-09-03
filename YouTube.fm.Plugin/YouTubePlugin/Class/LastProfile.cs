using System;
using System.Collections.Generic;
using System.Reflection;
using Lastfm.Scrobbling;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Google.GData.YouTube;
using Lastfm.Services;

namespace YouTubePlugin.Class
{

  public class LastProfile
  {
    private string username;
    private string password;
    private Session session;
    private Lastfm.Scrobbling.Connection connection;
    private Lastfm.Scrobbling.ScrobbleManager manager;
    const string API_KEY = "60d35bf7777d870ec958a21872bacb24";
    const string API_SECRET = "099158e5216ad77239be5e0a2228cf04";

    public bool IsLoged { get; set; }


    public LastProfile()
    {
      session = new Session(API_KEY, API_SECRET);
      IsLoged = false;
    }

    public bool Login(string username, string password)
    {
      this.username = username;
      this.password = password;
      session.Authenticate(this.username, Lastfm.Utilities.md5(this.password));
      if (session.Authenticated)
      {
        connection = new Lastfm.Scrobbling.Connection("mpm", Assembly.GetEntryAssembly().GetName().Version.ToString(),
                                                      this.username, session);
        manager = new Lastfm.Scrobbling.ScrobbleManager(connection);
        IsLoged = true;
        return true;
      }
      return false;
    }

    public bool Handshake()
    {
      connection.Initialize();
      return session.Authenticated;
    }

    public void NowPlaying(YouTubeEntry song)
    {
      if (!IsLoged)
        return;
      try
      {
        string Artist = string.Empty;
        string Title = string.Empty;
        int length = 0;
        string _title = song.Title.Text;
        if (_title.Contains("-"))
        {
          Artist = _title.Split('-')[0].Trim();
          Title = _title.Split('-')[1].Trim();
        }
        if (song.Duration != null && song.Duration.Seconds != null)
        {
          length = Convert.ToInt32(song.Duration.Seconds, 10);
        }

        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && length > 0)
        {
          NowplayingTrack track1 = new NowplayingTrack(Artist, Title, new TimeSpan(0, 0, length));
          manager.ReportNowplaying(track1);
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public void Submit(YouTubeEntry song)
    {
      if (!IsLoged)
        return;
      try
      {
        string Artist = string.Empty;
        string Title = string.Empty;
        int length = 0;
        string _title = song.Title.Text;
        if (_title.Contains("-"))
        {
          Artist = _title.Split('-')[0].Trim();
          Title = _title.Split('-')[1].Trim();
        }
        if (song.Duration != null && song.Duration.Seconds != null)
        {
          length = Convert.ToInt32(song.Duration.Seconds, 10);
        }

        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && length > 0)
        {
          Entry entry = new Entry(Artist, Title, DateTime.Now, PlaybackSource.User, new TimeSpan(0, 0, length), ScrobbleMode.Played);
          manager.Queue(entry);
          //manager.Submit();
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public List<string> SimilarArtists(Song song)
    {
      return new List<string>();
    }
  }
}
