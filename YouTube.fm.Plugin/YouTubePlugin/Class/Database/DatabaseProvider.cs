using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.YouTube;
using MediaPortal.Configuration;
using MediaPortal.Database;
using SQLite.NET;
using YouTubePlugin.Class.Artist;

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
            "CREATE TABLE VIDEOS(ID integer primary key autoincrement,VIDEO_ID text, ARTIST_ID text, TITLE text, IMG_URL text, LENGTH integer,STATE integer)\n");
          m_db.Execute(
            "CREATE TABLE PLAY_HISTORY(ID integer primary key autoincrement,VIDEO_ID text, datePlayed timestamp)\n");
        }
      }
      catch (SQLiteException ex)
      {
        //Log.Instance.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void SavePlayData(YouTubeEntry entry, DateTime dateTime)
    {
      string id = Youtube2MP.GetVideoId(entry);
      Save(entry);
      string lsSQL = string.Format("insert into PLAY_HISTORY (VIDEO_ID,datePlayed ) VALUES (\"{0}\",\"{1}\")",
                                   Youtube2MP.GetVideoId(entry), dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
      m_db.Execute(lsSQL);
    }

    public void Save(YouTubeEntry entry)
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

    public void Save(YouTubeEntry entry, ArtistItem artistItem)
    {
      Save(entry);
      string lsSQL = string.Format("UPDATE VIDEOS SET ARTIST_ID =\"{1}\" WHERE VIDEO_ID=\"{0}\" ", Youtube2MP.GetVideoId(entry),
                                   artistItem.Id);
      m_db.Execute(lsSQL);
    }

  }
}
