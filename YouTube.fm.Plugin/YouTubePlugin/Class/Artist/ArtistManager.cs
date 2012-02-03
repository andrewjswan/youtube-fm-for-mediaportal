using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;
using Lastfm.Services;

namespace YouTubePlugin.Class.Artist
{
  public class ArtistManager
  {

    public ArtistManager()
    {
      SitesCache = new SitesCache();
    }

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

    public SitesCache SitesCache { get; set; }

    public void InitDatabase()
    {
      bool dbExists;
      try
      {
        // Open database
        dbExists = System.IO.File.Exists(Config.GetFile(Config.Dir.Database, "YouTubeFm_V01.db3"));
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "YouTubeFm_V01.db3"));

        DatabaseUtility.SetPragmas(m_db);

        if (!dbExists)
        {
          m_db.Execute("CREATE TABLE ARTISTS(ID integer primary key autoincrement,ARTIST_ID text,ARTIST_NAME text,ARTIST_IMG text, ARTIST_BIO text, ARTIST_USER text, ARTIST_TAG text)\n");
          m_db.Execute("CREATE TABLE TAGS(ID integer primary key autoincrement,ARTIST_ID text, ARTIST_TAG text)\n");
        }
      }
      catch (SQLiteException ex)
      {
        //Log.Instance.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }



    public  ArtistGrabber Grabber = new ArtistGrabber();

    public List<string[]> GetTags()
    {
      List<string[]> res = new List<string[]>();
      string lsSQL = string.Format(" select * from (" +
                                   "select  lower(artist_tag) as tag, count(ARTIST_TAG) as cnt from TAGS group by ARTIST_TAG order by count(ARTIST_TAG) desc" +
                                   ") where cnt>50 order by tag");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new string[]
                  {
                    DatabaseUtility.Get(loResultSet, iRow, "tag"), DatabaseUtility.Get(loResultSet, iRow, "cnt")
                  });
      }
      return res;
    }

    public void SaveTag(ArtistItem artistItem, string tag)
    {
      try
      {
        if (string.IsNullOrEmpty(artistItem.Id) || string.IsNullOrEmpty(tag))
          return;
        string lsSQL =
          string.Format(
            "insert into TAGS (ARTIST_ID, ARTIST_TAG) VALUES (\"{0}\",\"{1}\")",
            artistItem.Id, tag);
        m_db.Execute(lsSQL);

      }
      catch (Exception exception)
      {
      }
    }

    public  void AddArtist(ArtistItem artistItem)
    {
      try
      {
      if(string.IsNullOrEmpty(artistItem.Id))
        return;

      string lsSQL = string.Format("select distinct ARTIST_ID,ARTIST_IMG from ARTISTS WHERE ARTIST_ID=\"{0}\"", artistItem.Id);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      if (loResultSet.Rows.Count > 0)
      {
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          artistItem.Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG");
          artistItem.Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG");
        }
        return;
      }
      lsSQL =
        string.Format(
          "insert into ARTISTS (ARTIST_ID,ARTIST_NAME,ARTIST_IMG, ARTIST_USER, ARTIST_TAG) VALUES (\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\")",
          artistItem.Id,
          DatabaseUtility.RemoveInvalidChars(artistItem.Name.Replace('"', '`')), artistItem.Img_url, artistItem.User,
          artistItem.Tags);
      m_db.Execute(lsSQL);
      artistItem.Db_id = m_db.LastInsertID();
      }
      catch (Exception)
      {
      }
    }

    public List<string> GetArtistsLetters()
    {
      List<string> res = new List<string>();
      string lsSQL = string.Format("select distinct upper(substr(ARTIST_NAME,1,1)) AS LETTER from ARTISTS order by ARTIST_NAME");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(DatabaseUtility.Get(loResultSet, iRow, "LETTER"));

      }
      return res;
    }

    public ArtistItem GetArtistsByName(string name)
    {
      ArtistItem res = new ArtistItem();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like \"{0}%\" order by ARTIST_NAME",
                                   DatabaseUtility.RemoveInvalidChars(name.Replace('"', '`')));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID");
        res.Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'");
        res.Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG");
        res.User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER");
        res.Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG");
        break;
      }
      return res;
    }

    public List<ArtistItem> GetArtistsByTag(string tag)
    {
      List<ArtistItem> res = new List<ArtistItem>();
      string lsSQL =
        string.Format(
          "select distinct * from ARTISTS,TAGS where ARTISTS.ARTIST_ID=TAGS.ARTIST_ID and TAGS.ARTIST_TAG like '{0}' order by ARTIST_NAME",
          tag);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new ArtistItem()
        {
          Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"),
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'").Replace("`", "\""),
          Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG"),
          User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER"),
          Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG")
        });
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
                    Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'").Replace("`", "\""),
                    Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG"),
                    User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER"),
                    Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG")
                  });
      }
      return res;
    }

    public List<ArtistItem> GetVevoArtists()
    {
      List<ArtistItem> res = new List<ArtistItem>();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_USER like '%VEVO%' order by ARTIST_NAME");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new ArtistItem
                  {
          Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"),
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'").Replace("`", "\""),
          Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG"),
          User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER"),
          Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG")
        });
      }
      return res;
    }

    public List<ArtistItem> GetArtistsByIds(string letter)
    {
      List<ArtistItem> res = new List<ArtistItem>();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_ID in ({0}) order by ARTIST_NAME", letter);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new ArtistItem
        {
          Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"),
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME").Replace("''", "'").Replace("`", "\""),
          Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG"),
          User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER"),
          Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG")
        });
      }
      return res;
    }

    public string GetArtistsImgUrl(string letter)
    {
      try
      {
        if (string.IsNullOrEmpty(letter))
          return string.Empty;
        string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like \"{0}\" order by ARTIST_NAME", DatabaseUtility.RemoveInvalidChars(letter.Replace('"', '`')));
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
        {
          return DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG");
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      return string.Empty;
    }

    public List<ArtistItem> GetArtists()
    {
      List<ArtistItem> res = new List<ArtistItem>();
      string lsSQL = string.Format("select * from ARTISTS  order by ARTIST_NAME");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Add(new ArtistItem()
                  {
                    Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID"),
                    Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME"),
                    Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG"),
                    User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER"),
                    Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG")
                  });
      }
      return res;
    }

    public ArtistItem GetArtistsById(string id)
    {
      ArtistItem res = new ArtistItem();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_ID = \"{0}\" order by ARTIST_NAME", id);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID");
        res.Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME");
        res.Img_url = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_IMG");
        res.User = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_USER");
        res.Tags = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_TAG");
      };
     
      return res;
    }

    public void Save(ArtistItem artistItem)
    {
      if (string.IsNullOrEmpty(artistItem.Id))
        return;
      //if (Youtube2MP.LastFmProfile.Session != null)
      //{
      //  Lastfm.Services.Artist artist = new Lastfm.Services.Artist(artistItem.Name, Youtube2MP.LastFmProfile.Session);
      //  artistItem.Img_url = artist.GetImageURL(ImageSize.Large);
      //}
      string lsSQL =
        string.Format(
          "UPDATE ARTISTS SET ARTIST_NAME =\"{1}\" ,ARTIST_IMG=\"{2}\", ARTIST_USER=\"{3}\", ARTIST_TAG=\"{4}\", ARTIST_BIO=\"{5}\"  WHERE ARTIST_ID=\"{0}\" ",
          artistItem.Id, DatabaseUtility.RemoveInvalidChars(artistItem.Name.Replace('"', '`')),
          artistItem.Img_url, artistItem.User, artistItem.Tags, artistItem.Bio);
      m_db.Execute(lsSQL);
    }


  }
}
