using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;

namespace YouTubePlugin.Class.Artist
{
  public class ArtistManager
  {
    private SQLiteClient m_db;

    private static ArtistManager _instance;
    public static ArtistManager Instance
    {
      get
      {
        if(_instance==null)
          _instance = new ArtistManager();
        return _instance;
      }
      set { _instance = value; }
    }

    public void InitDatabase()
    {
      bool dbExists;
      try
      {
        // Open database
        dbExists = System.IO.File.Exists(Config.GetFile(Config.Dir.Database, "YouTubeFm_V01.db3"));
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "YouTubeFm_V01.db3"));

        MediaPortal.Database.DatabaseUtility.SetPragmas(m_db);

        if (!dbExists)
        {
          m_db.Execute("CREATE TABLE ARTISTS(ID integer primary key autoincrement,ARTIST_ID text,ARTIST_NAME text,ARTIST_IMG text, ARTIST_BIO text,ARTIST_USER text)\n");
        }
      }
      catch (SQLiteException ex)
      {
        //Log.Instance.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }



    public  ArtistGrabber Grabber = new ArtistGrabber();

    public  void AddArtist(ArtistItem artistItem)
    {
      if(string.IsNullOrEmpty(artistItem.Id))
        return;
      string lsSQL = string.Format("select distinct ARTIST_ID from ARTISTS WHERE ARTIST_ID=\"{0}\"", artistItem.Id);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      if (loResultSet.Rows.Count > 0)
        return;
      lsSQL = string.Format("insert into ARTISTS (ARTIST_ID,ARTIST_NAME) VALUES (\"{0}\",\"{1}\")", artistItem.Id,
                            artistItem.Name);
      m_db.Execute(lsSQL);
      artistItem.Db_id = m_db.LastInsertID();
    }

    public List<string> GetArtistsLetters()
    {
      List<string> res = new List<string>();
      string lsSQL = string.Format("select distinct substr(ARTIST_NAME,1,1) AS LETTER from ARTISTS order by ARTIST_NAME");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(DatabaseUtility.Get(loResultSet, iRow, "LETTER"));

      }
      return res;
    }


    public List<ArtistItem> GetArtists(string letter)
    {
      List<ArtistItem> res=new List<ArtistItem>();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like \"{0}%\" order by ARTIST_NAME",letter);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new ArtistItem()
                  {
                    Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"),
                    Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME"),
                  });

      }
      return res;
    }
  }
}
