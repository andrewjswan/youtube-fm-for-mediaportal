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
            "CREATE TABLE VIDEOS(ID integer primary key autoincrement,VIDEO_ID text, ARTIST_ID text, TITLE text, IMG_URL text, LENGTH integer,STATE integer, rating integer, hd integer)\n");
          m_db.Execute(
            "CREATE TABLE PLAY_HISTORY(ID integer primary key autoincrement,VIDEO_ID text, datePlayed timestamp, loved integer)\n");

          CreateArtistTable(m_db);

          DatabaseUtility.AddIndex(m_db, "idx_video_id", "CREATE INDEX idx_video_id ON VIDEOS(VIDEO_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_ARTIST_ID", "CREATE INDEX idx_ARTIST_ID ON VIDEOS(ARTIST_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_his_video_id", "CREATE INDEX idx_his_video_id ON PLAY_HISTORY(VIDEO_ID)");
          DatabaseUtility.AddIndex(m_db, "idx_his_date", "CREATE INDEX idx_his_date ON PLAY_HISTORY(datePlayed DESC)");
        }
        else
        {
          if(!DatabaseUtility.TableColumnExists(m_db,"VIDEOS","hd"))
          {
            m_db.Execute("ALTER TABLE VIDEOS ADD hd integer");
          }
          if (!DatabaseUtility.TableExists(m_db, "ARTISTS"))
          {
            CreateArtistTable(m_db);
          }
        }
      }
      catch (SQLiteException ex)
      {
        Log.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private void CreateArtistTable(SQLiteClient db)
    {
      db.Execute("CREATE TABLE ARTISTS(ID integer primary key autoincrement,ARTIST_ID text, ARTIST_NAME text, ARTIST_IMG text, ARTIST_BIO text, ARTIST_USER text, ARTIST_TAG text, ARTIST_GENRE text, ARTIST_IMG_URL text, ARTIST_PLAYED integer)\n");
      DatabaseUtility.AddIndex(m_db, "idx_artists_name", "CREATE INDEX idx_artists_name ON ARTISTS(ARTIST_NAME)");
      DatabaseUtility.AddIndex(m_db, "idx_artists_ARTIST_ID", "CREATE INDEX idx_ARTIST_ID ON ARTISTS(ARTIST_ID)");
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
          bool ishd =
            entry.ExtensionElements.Any(
              extensionElementFactory =>
              extensionElementFactory.XmlPrefix == "yt" && extensionElementFactory.XmlName == "hd");

          lsSQL =
            string.Format(
              "insert into VIDEOS (VIDEO_ID,TITLE ,LENGTH,IMG_URL, hd ) VALUES (\"{0}\",\"{1}\",{2},\"{3}\",{4})",
              Youtube2MP.GetVideoId(entry),
              DatabaseUtility.RemoveInvalidChars(entry.Title.Text.Replace('"', '`')),
              duration,
              YoutubeGUIBase.GetBestUrl(entry.Media.Thumbnails),
              ishd ? 1 : 0);
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
      ArtistItem artistItem = new ArtistItem();
      string lsSQL = string.Format("select distinct ARTIST_ID from VIDEOS WHERE VIDEO_ID=\"{0}\" ", Youtube2MP.GetVideoId(entry));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        artistItem= ArtistManager.Instance.GetArtistsById(DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"));
        break;
      }
      ArtistItem artistItemLocal = GetArtistsByName(artistItem.Name);
      if (artistItemLocal != null)
      {
        artistItem.Bio = artistItemLocal.Bio;
        artistItem.Img_url = artistItem.Img_url;
      }
      return artistItem;
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

    public void Save(ArtistItem artistItem)
    {
      //if (string.IsNullOrEmpty(artistItem.Id))
      //  return;
      //if (Youtube2MP.LastFmProfile.Session != null)
      //{
      //  Lastfm.Services.Artist artist = new Lastfm.Services.Artist(artistItem.Name, Youtube2MP.LastFmProfile.Session);
      //  artistItem.Img_url = artist.GetImageURL(ImageSize.Large);
      //}
      string lsSQL =
        string.Format(
          "UPDATE ARTISTS SET ARTIST_NAME ='{1}' ,ARTIST_IMG='{2}', ARTIST_USER='{3}', ARTIST_TAG='{4}', ARTIST_BIO='{5}'  WHERE ARTIST_NAME like '{0}' ",
          DatabaseUtility.RemoveInvalidChars(artistItem.Name), DatabaseUtility.RemoveInvalidChars(artistItem.Name),
          artistItem.Img_url, artistItem.User, DatabaseUtility.RemoveInvalidChars(artistItem.Tags), DatabaseUtility.RemoveInvalidChars(artistItem.Bio));
      m_db.Execute(lsSQL);
    }

    public void AddArtist(ArtistItem artistItem)
    {
      try
      {
        if (string.IsNullOrEmpty(artistItem.Name))
          return;

        string lsSQL = string.Format("select distinct ARTIST_ID,ARTIST_IMG from ARTISTS WHERE ARTIST_NAME like '{0}'", artistItem.Name);
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        if (loResultSet.Rows.Count > 0)
        {
          Save(artistItem);
          return;
        }
        lsSQL =
          string.Format(
            "insert into ARTISTS (ARTIST_ID,ARTIST_NAME ,ARTIST_IMG, ARTIST_USER, ARTIST_TAG, ARTIST_BIO) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
            artistItem.Id,
            DatabaseUtility.RemoveInvalidChars(artistItem.Name), artistItem.Img_url, artistItem.User,
            DatabaseUtility.RemoveInvalidChars(artistItem.Tags), DatabaseUtility.RemoveInvalidChars(artistItem.Bio));
        m_db.Execute(lsSQL);
        artistItem.Db_id = m_db.LastInsertID();
      }
      catch (Exception exception)
      {
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

    public int GetWatchCount(string id)
    {
      try
      {
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

    public bool IsHd(string id)
    {
      try
      {
        string lsSQL = string.Format("select hd from VIDEOS WHERE VIDEO_ID=\"{0}\"", id);
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        if (loResultSet.Rows.Count > 0)
        {
          return DatabaseUtility.GetAsInt(loResultSet, 0, "hd") == 1;
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return false;
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
        string lastid = "";
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          YouTubeEntry youTubeEntry = new YouTubeEntry();
          if (lastid == DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID"))
            continue;
          lastid = DatabaseUtility.Get(loResultSet, iRow, "VIDEO_ID");
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

    public ArtistItem GetArtistsByName(string name)
    {
      if (string.IsNullOrEmpty(name))
        return null;
      ArtistItem res = new ArtistItem();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like \"{0}\" order by ARTIST_NAME",
                                   DatabaseUtility.RemoveInvalidChars(name.Replace('"', '`')));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID");
        res.Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'");
        res.Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG");
        res.User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER");
        res.Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG");
        res.Bio = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_BIO");
        return res;
      }
      return null;
    }
  }
}
