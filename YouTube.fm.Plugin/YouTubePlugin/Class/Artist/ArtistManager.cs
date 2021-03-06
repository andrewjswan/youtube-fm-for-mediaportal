﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Google.GData.YouTube;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;
using Lastfm.Services;
using YouTubePlugin.Class.Database;
using YouTubePlugin.DataProvider;

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
            "insert into TAGS (ARTIST_ID, ARTIST_TAG) VALUES ('{0}','{1}')",
            artistItem.Id, DatabaseUtility.RemoveInvalidChars(tag));
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
          "insert into ARTISTS (ARTIST_ID,ARTIST_NAME,ARTIST_IMG, ARTIST_USER, ARTIST_TAG) VALUES ('{0}','{1}','{2}','{3}','{4}')",
          artistItem.Id,
          DatabaseUtility.RemoveInvalidChars(artistItem.Name), artistItem.Img_url, artistItem.User,
          DatabaseUtility.RemoveInvalidChars(artistItem.Tags));
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

    public List<string> GetArtistsLetters(string letter)
    {
      List<string> res = new List<string>();
      string lsSQL =
        string.Format(
          "select distinct upper(substr(ARTIST_NAME,1,{0})) AS LETTER from ARTISTS WHERE ARTIST_NAME like '{1}%' order by upper(ARTIST_NAME)",
          letter.Length + 1, letter);
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      string oldvalue = "";
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        string l = DatabaseUtility.Get(loResultSet, iRow, "LETTER");
        if (l.Length == 1)
        {
          l += " ";
        }
        if (l != oldvalue)
          res.Add(l);
        oldvalue = l;
      }
      return res;
    }

    public ArtistItem GetArtistsByName(string name)
    {

      ArtistItem res = new ArtistItem();
      res = DatabaseProvider.InstanInstance.GetArtistsByName(name);
      if (res != null)
        return res;
      res = new ArtistItem();
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like '{0}' order by ARTIST_NAME",
                                   DatabaseUtility.RemoveInvalidChars(name));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      for (int iRow = 0; iRow < loResultSet.Rows.Count; iRow++)
      {
        res.Id = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_ID");
        res.Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME");
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
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME"),
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
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like '{0}%' order by ARTIST_NAME",
                                   DatabaseUtility.RemoveInvalidChars(letter));
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
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME"),
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
          Name = DatabaseUtility.Get(loResultSet, iRow, "ARTIST_NAME"),
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
        string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_NAME like '{0}' order by ARTIST_NAME", DatabaseUtility.RemoveInvalidChars(letter));
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
      string lsSQL = string.Format("select * from ARTISTS WHERE ARTIST_ID = '{0}' order by ARTIST_NAME", id);
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
          "UPDATE ARTISTS SET ARTIST_NAME ='{1}' ,ARTIST_IMG='{2}', ARTIST_USER='{3}', ARTIST_TAG='{4}', ARTIST_BIO='{5}'  WHERE ARTIST_ID='{0}' ",
          artistItem.Id, DatabaseUtility.RemoveInvalidChars(artistItem.Name),
          artistItem.Img_url, artistItem.User, DatabaseUtility.RemoveInvalidChars(artistItem.Tags),
          DatabaseUtility.RemoveInvalidChars(artistItem.Bio));
      m_db.Execute(lsSQL);
    }

    public bool SetSkinProperties(YouTubeEntry youTubeEntry, string prefix, bool grab, bool download)
    {
      if (youTubeEntry == null)
        return true;
      ArtistItem artistItem = DatabaseProvider.InstanInstance.GetArtist(youTubeEntry);
      if (artistItem == null && grab)
      {
        string vidId = Youtube2MP.GetVideoId(youTubeEntry);
        artistItem = SitesCache.GetByVideoId(vidId) != null
                       ? Grabber.GetFromVideoSite(SitesCache.GetByVideoId(vidId).SIte)
                       : Grabber.GetFromVideoId(vidId);
      }
      if (artistItem == null)
      {
        string art = GetArtistName(youTubeEntry.Title.Text);
        artistItem = GetArtistsByName(art);
      }
      if ((artistItem == null || string.IsNullOrEmpty(artistItem.Bio) || string.IsNullOrEmpty(artistItem.Img_url)) && grab)
      {
        if (artistItem == null || string.IsNullOrEmpty((artistItem.Name)))
          artistItem = new ArtistItem() {Name = GetArtistName(youTubeEntry.Title.Text)};

        try
        {
          Lastfm.Services.Artist artist = new Lastfm.Services.Artist(artistItem.Name, Youtube2MP.LastFmProfile.Session);
          if (string.IsNullOrEmpty(artistItem.Img_url))
          {
            artistItem.Img_url = artist.GetImageURL(ImageSize.Huge);
          }
          if (string.IsNullOrEmpty(artistItem.Bio))
          {
            ArtistBio artistBio = artist.Bio;
            artistBio.Lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
            string contents = Regex.Replace(HttpUtility.HtmlDecode(artistBio.getContent()), "<.*?>",
                                            string.Empty);
            if (string.IsNullOrEmpty(contents))
            {
              artistBio.Lang = string.Empty;
              contents = Regex.Replace(HttpUtility.HtmlDecode(artistBio.getContent()), "<.*?>",
                                       string.Empty);
            }
            artistItem.Bio = contents;
          }
          if (string.IsNullOrEmpty(artistItem.Tags))
          {
            int i = 0;
            string tags = "";
            TopTag[] topTags = artist.GetTopTags();
            foreach (TopTag tag in topTags)
            {
              tags += tag.Item.Name + "|";
              if (i < 5)
              {
                if (!string.IsNullOrEmpty(artistItem.Id))
                  SaveTag(artistItem, tag.Item.Name);
              }
              i++;
            }
            artistItem.Tags = tags;
          }
          DatabaseProvider.InstanInstance.AddArtist(artistItem);
        }
        catch (Exception exception)
        {
          Log.Debug(exception.Message);
        }
      }
      if (artistItem != null)
      {
        if (download && !File.Exists(artistItem.LocalImage))
        {
          Youtube2MP.DownloadFile(artistItem.Img_url, artistItem.LocalImage);
        }
        SetSkinProperties(artistItem, prefix);
      }
      return false;
    }

    public void SetSkinProperties(ArtistItem artistItem,string prefix)
    {
      GUIPropertyManager.SetProperty("#Youtube.fm." + prefix + ".Artist.Image", " ");
      GUIPropertyManager.SetProperty("#Youtube.fm." + prefix + ".Artist.Name", Property(artistItem.Name));
      GUIPropertyManager.SetProperty("#Youtube.fm." + prefix + ".Artist.Bio", Property(artistItem.Bio));
      GUIPropertyManager.SetProperty("#Youtube.fm." + prefix + ".Artist.Tags", Property(artistItem.Tags));
      GUIPropertyManager.SetProperty("#Youtube.fm." + prefix + ".Artist.Image",
                                     Property(artistItem.LocalImage));
    }

    private string Property(string s)
    {
      if (string.IsNullOrEmpty(s))
        return " ";
      return s;
    }

    public string GetArtistName(YouTubeEntry entry)
    {
      return GetArtistName(entry.Title.Text);
    }

    public string GetArtistName(string title)
    {
      string name = "";
      if (title.Contains(" - "))
      {
        name = title.Substring(0, title.IndexOf(" - ", System.StringComparison.Ordinal));
      }
      else if (title.Contains("-"))
      {
        name = title.Substring(0, title.IndexOf("-", StringComparison.Ordinal));
      }
      else if (title.Contains(":"))
      {
        return title.Substring(0, title.IndexOf(":", System.StringComparison.Ordinal));
      }
      return name.Trim();
    }
  }
}
