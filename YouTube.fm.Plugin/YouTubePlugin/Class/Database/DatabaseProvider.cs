using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;
using YouTubePlugin.Class.Artist;
using MediaGroup = Google.GData.YouTube.MediaGroup;

namespace YouTubePlugin.Class.Database
{
  public class DatabaseProvider
  {
    private SQLiteClient m_db;
    private static DatabaseProvider _instanInstance;

    public static DatabaseProvider InstanInstance
    {
      get
      {
        if (_instanInstance == null)
          _instanInstance = new DatabaseProvider();
        return _instanInstance;
      }
      set { _instanInstance = value; }
    }

    public void InitDatabase()
    {
      bool dbExists;
      try
      {
        // Open database
        dbExists = System.IO.File.Exists(Config.GetFile(Config.Dir.Database, "YouTubeFm_Data_V01.db3"));
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "YouTubeFm_Data_V01.db3"));

        DatabaseUtility.SetPragmas(m_db);

        if (!dbExists)
        {
          m_db.Execute(
            "CREATE TABLE VIDEOS(ID integer primary key autoincrement,VIDEO_ID text, ARTIST_ID text, TITLE text, IMG_URL text, LENGTH integer,STATE integer, rating integer)\n");
          m_db.Execute(
            "CREATE TABLE PLAY_HISTORY(ID integer primary key autoincrement,VIDEO_ID text, datePlayed timestamp, loved integer)\n");
          DatabaseUtility.AddIndex(m_db, "idx_video_id", "CREATE INDEX idx_video_id ON VIDEOS(VIDEO_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_ARTIST_ID", "CREATE INDEX idx_ARTIST_ID ON VIDEOS(ARTIST_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_his_video_id", "CREATE INDEX idx_his_video_id ON PLAY_HISTORY(VIDEO_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_his_date", "CREATE INDEX idx_his_date ON PLAY_HISTORY(datePlayed DESC)");
        }
      }
      catch (SQLiteException ex)
      {
        Log.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void SavePlayData(YouTubeEntry entry, DateTime dateTime)
    {
      try
      {
        Save(entry);
        string lsSQL = string.Format("insert into PLAY_HISTORY (VIDEO_ID,datePlayed ) VALUES (\"{0}\",\"{1}\")",
                                     Youtube2MP.GetVideoId(entry), dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        m_db.Execute(lsSQL);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public void Save(YouTubeEntry entry)
    {
      try
      {
        string id = Youtube2MP.GetVideoId(entry);
        string lsSQL = string.Format("select distinct VIDEO_ID from VIDEOS WHERE VIDEO_ID=\"{0}\"", id);
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        if (loResultSet.Rows.Count == 0)
        {
          int duration = 0;
          if (entry.Duration != null && entry.Duration.Seconds != null)
            duration = Convert.ToInt32(entry.Duration.Seconds);
          lsSQL =
            string.Format("insert into VIDEOS (VIDEO_ID,TITLE ,LENGTH,IMG_URL ) VALUES (\"{0}\",\"{1}\",{2},\"{3}\")",
                          Youtube2MP.GetVideoId(entry),
                          DatabaseUtility.RemoveInvalidChars(entry.Title.Text.Replace('"', '`')),
                          duration,
                          YoutubeGUIBase.GetBestUrl(entry.Media.Thumbnails));
          m_db.Execute(lsSQL);
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public ArtistItem GetArtist(YouTubeEntry entry)
    {
      string lsSQL = string.Format("select distinct ARTIST_ID from VIDEOS WHERE VIDEO_ID=\"{0}\" ", Youtube2MP.GetVideoId(entry));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        return ArtistManager.Instance.GetArtistsById(DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"));
      }
      return null;
    }

    public void Save(YouTubeEntry entry, ArtistItem artistItem)
    {
      try
      {
        Save(entry);
        string lsSQL = string.Format("UPDATE VIDEOS SET ARTIST_ID =\"{1}\" WHERE VIDEO_ID=\"{0}\" ", Youtube2MP.GetVideoId(entry),
                                     artistItem.Id);
        m_db.Execute(lsSQL);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public int GetWatchCount(YouTubeEntry entry)
    {
      try
      {
        string id = Youtube2MP.GetVideoId(entry);
        string lsSQL = string.Format("select count(*) AS CNT from PLAY_HISTORY WHERE VIDEO_ID=\"{0}\"", id);
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        if (loResultSet.Rows.Count > 0)
        {
          return DatabaseUtility.GetAsInt(loResultSet, 0, "CNT");
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return 0;
    }

    public GenericListItemCollections GetTopPlayed()
    {
      GenericListItemCollections res = new GenericListItemCollections();
      try
      {
        string lsSQL =
          string.Format(
            "SELECT VIDEOS.VIDEO_ID AS VIDEO_ID, ARTIST_ID, TITLE, IMG_URL, count(PLAY_HISTORY.VIDEO_ID) as num_play FROM VIDEOS, PLAY_HISTORY WHERE VIDEOS.VIDEO_ID=PLAY_HISTORY.VIDEO_ID group by VIDEOS.VIDEO_ID, ARTIST_ID, TITLE, IMG_URL order by count(PLAY_HISTORY.VIDEO_ID) desc");
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          YouTubeEntry youTubeEntry = new YouTubeEntry();

          youTubeEntry.AlternateUri = new AtomUri("http://www.youtube.com/watch?v=" + DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID"));
          youTubeEntry.Title = new AtomTextConstruct();
          youTubeEntry.Title.Text = DatabaseUtility.Get(loResultSet, iRow, "TITLE");
          youTubeEntry.Media = new MediaGroup();
          youTubeEntry.Media.Description = new MediaDescription("");
          youTubeEntry.Id = new AtomId(youTubeEntry.AlternateUri.Content);
          GenericListItem listItem = new GenericListItem()
          {
            Title = youTubeEntry.Title.Text,
            IsFolder = false,
            LogoUrl = DatabaseUtility.Get(loResultSet, iRow, "IMG_URL"),
            Tag = youTubeEntry,
            Title2 = DatabaseUtility.Get(loResultSet, iRow, "num_play"),
            //ParentTag = artistItem
          };
          res.Items.Add(listItem);
        };

      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return res;
    }

    public GenericListItemCollections GetTopPlayed(int numPlay)
    {
      GenericListItemCollections res = new GenericListItemCollections();
      try
      {
        string lsSQL =
          string.Format(
            "select * from (SELECT VIDEOS.VIDEO_ID AS VIDEO_ID, ARTIST_ID, TITLE, IMG_URL, count(PLAY_HISTORY.VIDEO_ID) as num_play FROM VIDEOS, PLAY_HISTORY WHERE VIDEOS.VIDEO_ID=PLAY_HISTORY.VIDEO_ID group by VIDEOS.VIDEO_ID, ARTIST_ID, TITLE, IMG_URL order by count(PLAY_HISTORY.VIDEO_ID) desc)where num_play>" +
            numPlay.ToString());
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          YouTubeEntry youTubeEntry = new YouTubeEntry();

          youTubeEntry.AlternateUri = new AtomUri("http://www.youtube.com/watch?v=" + DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID"));
          youTubeEntry.Title = new AtomTextConstruct();
          youTubeEntry.Title.Text = DatabaseUtility.Get(loResultSet, iRow, "TITLE");
          youTubeEntry.Media = new MediaGroup();
          youTubeEntry.Media.Description = new MediaDescription("");
          youTubeEntry.Id = new AtomId(youTubeEntry.AlternateUri.Content);
          GenericListItem listItem = new GenericListItem()
          {
            Title = youTubeEntry.Title.Text,
            IsFolder = false,
            LogoUrl = DatabaseUtility.Get(loResultSet, iRow, "IMG_URL"),
            Tag = youTubeEntry,
            Title2 = DatabaseUtility.Get(loResultSet, iRow, "num_play"),
            //ParentTag = artistItem
          };
          res.Items.Add(listItem);
        };

      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return res;
    }

    public GenericListItemCollections GetRandom()
    {
      GenericListItemCollections res = new GenericListItemCollections();
      try
      {
        string lsSQL =
          string.Format(
            "SELECT VIDEOS.VIDEO_ID AS VIDEO_ID, ARTIST_ID, TITLE, IMG_URL, count(PLAY_HISTORY.VIDEO_ID) as num_play FROM VIDEOS, PLAY_HISTORY WHERE VIDEOS.VIDEO_ID=PLAY_HISTORY.VIDEO_ID group by VIDEOS.VIDEO_ID, ARTIST_ID, TITLE, IMG_URL order by RANDOM()");
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          YouTubeEntry youTubeEntry = new YouTubeEntry();

          youTubeEntry.AlternateUri = new AtomUri("http://www.youtube.com/watch?v=" + DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID"));
          youTubeEntry.Title = new AtomTextConstruct();
          youTubeEntry.Title.Text = DatabaseUtility.Get(loResultSet, iRow, "TITLE");
          youTubeEntry.Media = new MediaGroup();
          youTubeEntry.Media.Description = new MediaDescription("");
          youTubeEntry.Id = new AtomId(youTubeEntry.AlternateUri.Content);
          GenericListItem listItem = new GenericListItem()
          {
            Title = youTubeEntry.Title.Text,
            IsFolder = false,
            LogoUrl = DatabaseUtility.Get(loResultSet, iRow, "IMG_URL"),
            Tag = youTubeEntry,
            Title2 = DatabaseUtility.Get(loResultSet, iRow, "num_play"),
            //ParentTag = artistItem
          };
          res.Items.Add(listItem);
        };
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return res;
    }

    public string GetPlayedArtistIds(int numPlay)
    {
      List<string> ids = new List<string>();
      string lsSQL =
        string.Format(
          "select * from (SELECT VIDEOS.VIDEO_ID AS VIDEO_ID, ARTIST_ID, TITLE, IMG_URL, count(PLAY_HISTORY.VIDEO_ID) as num_play FROM VIDEOS, PLAY_HISTORY WHERE VIDEOS.VIDEO_ID=PLAY_HISTORY.VIDEO_ID group by VIDEOS.VIDEO_ID, ARTIST_ID, TITLE, IMG_URL order by count(PLAY_HISTORY.VIDEO_ID) desc)where num_play>" +
          numPlay.ToString());
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        string id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID");
        if (!string.IsNullOrEmpty(id) && !ids.Contains(id))
          ids.Add(id);
      }
      string ret = "";
      for (int i = 0; i < ids.Count; i++)
      {
        ret += "'"+ids[i]+"'";
        if (i != ids.Count - 1)
          ret += ",";
      }
      return ret;
    }

    public GenericListItemCollections GetRecentlyPlayed()
    {
      GenericListItemCollections res = new GenericListItemCollections();
      try
      {
        string lsSQL =
          string.Format(
            "SELECT VIDEOS.VIDEO_ID AS VIDEO_ID, ARTIST_ID, TITLE, IMG_URL FROM VIDEOS, PLAY_HISTORY WHERE VIDEOS.VIDEO_ID=PLAY_HISTORY.VIDEO_ID order by datePlayed DESC");
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          YouTubeEntry youTubeEntry = new YouTubeEntry();

          youTubeEntry.AlternateUri =
            new AtomUri("http://www.youtube.com/watch?v=" + DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID"));
          youTubeEntry.Title = new AtomTextConstruct();
          youTubeEntry.Title.Text = DatabaseUtility.Get(loResultSet, iRow, "TITLE");
          youTubeEntry.Media = new MediaGroup();
          youTubeEntry.Media.Description = new MediaDescription("");
          youTubeEntry.Id = new AtomId(youTubeEntry.AlternateUri.Content);
          GenericListItem listItem = new GenericListItem()
                                       {
                                         Title = youTubeEntry.Title.Text,
                                         IsFolder = false,
                                         LogoUrl = DatabaseUtility.Get(loResultSet, iRow, "IMG_URL"),
                                         Tag = youTubeEntry,
                                         //ParentTag = artistItem
                                       };
          res.Items.Add(listItem);
        }
        ;

      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return res;
    }

  }
}
