using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using YouTubePlugin.Class.Artist;

/*
 * Original code come from mvCentral plugin code
 * http://code.google.com/p/mvcentral/
 */
namespace YouTubePlugin.DataProvider
{
  public class AllMusic
  {
    public bool GetDetails(ArtistItem artistItem)
    {
      if (string.IsNullOrEmpty(artistItem.Name))
        return false;
      string strArtistHTML;
      string strAlbumHTML;
      string strArtistURL;
      string artist = artistItem.Name;
      if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
      {
        var artistInfo = new MusicArtistInfo();
        if (artistInfo.Parse(strArtistHTML))
        {
          artistInfo.Artist = artist;

          artistItem.Bio = artistInfo.AMGBiography;
          if (!string.IsNullOrEmpty(artistInfo.ImageURL))
            artistItem.Img_url = artistInfo.ImageURL;
          //setMusicVideoArtist(ref mv1, artistInfo, strArtistHTML);
          //GetArtistArt((DBArtistInfo)mv);
          return true;
        }
      }
      return false;
    }

    #region Provider variables

    private const string BaseURL = "http://www.allmusic.com/search/artist/";

    private const string SongRegExpPattern = @"<td\s*class=""cell""><a\s*href=""(?<songURL>.*?)"">(?<songName>.*)</a></td>";
    private static readonly Regex SongURLRegEx = new Regex(SongRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private const string AlbumRegExpPattern = @"<td\s*class=""cell""><a\s*href=""(?<albumURL>.*?)"">(?<albumName>.*)</a></td>";
    private static readonly Regex AlbumURLRegEx = new Regex(AlbumRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private const string ArtistRegExpPattern = @"<td><a href=""(?<artistURL>.*?)"">(?<artist>.*?)</a></td>\s*<td>(?<genres>.*?)</td>\s*<td>(?<years>.*?)</td>\s*</tr>";
    private static readonly Regex ArtistURLRegEx = new Regex(ArtistRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BracketRegEx = new Regex(@"\s*[\(\[\{].*?[\]\)\}]\s*", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);

    private Match _match = null;
    private string _strFormed = "";
    private static bool _strippedPrefixes;
    private static bool _logMissing;
    private static bool _useAlternative = true;
    private static bool _useProxy;
    private static string _proxyHost;
    private static int _proxyPort;

    List<string> albumURLList = new List<string>();
    List<string> songURLList = new List<string>();
    #endregion


    #region URL and HTTP Handling

    /// <summary>
    /// Used to get the artist URL from allmusic.com based on artist name
    /// </summary>
    /// <param name="strArtist">artist name</param>
    /// <param name="strArtistURL">the URL of the artist</param>
    /// <returns>True if an artist page can be found else false</returns>
    /// <summary>
    /// Used to get the artist URL from allmusic.com based on artist name
    /// </summary>
    /// <param name="strArtist">artist name</param>
    /// <param name="strArtistURL">the URL of the artist</param>
    /// <returns>True if an artist page can be found else false</returns>
    private bool GetArtistURL(String strArtist, out String strArtistURL, out List<string> strArtistURLs)
    {
      strArtistURL = string.Empty;
      strArtistURLs = new List<string>();
      try
      {
        var strEncodedArtist = EncodeString(strArtist);
        var strURL = BaseURL + strEncodedArtist + "/filter:pop";

        var x = (HttpWebRequest)WebRequest.Create(strURL);
        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          x.Abort();
          if ((int)y.StatusCode != 302)
          {
            y.Close();

            if (!_useAlternative)
            {
              return false;
            }
            var altTry = GetArtistURLAlternative(strArtist, out strArtistURL, out strArtistURLs);
            if (altTry)
            {
              return true;
            }
            return altTry;
          }
          strArtistURL = y.GetResponseHeader("Location");

          y.Close();

        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }

      return true;
    }

    /// <summary>
    /// If unable to find an artist URL based on name straight away (ie. we are sent to search page)
    /// Then attempt to find artist within search results
    /// </summary>
    /// <param name="strArtist">Name of artist we are searching for</param>
    /// <param name="strArtistURL">URL of artist</param>
    /// <returns>True if artist page found</returns>
    private bool GetArtistURLAlternative(String strArtist, out String strArtistURL, out List<string> strArtistURLs)
    {
      strArtistURL = string.Empty;
      strArtistURLs = new List<string>();

      try
      {
        var strEncodedArtist = EncodeString(strArtist);
        var strURL = BaseURL + strEncodedArtist + "/filter:pop";

        var x = (HttpWebRequest)WebRequest.Create(strURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            string artistHTML;

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              artistHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();

            var matches = ArtistURLRegEx.Matches(artistHTML);
            var numberOfMatchesWithYears = 0;
            //some cases like Björk where there is only a single entry matching artist in list but stil returns list rather then redirecting to artist page
            if (matches.Count == 1)
            {
              strArtistURL = matches[0].Groups["artistURL"].ToString();
              //logger.Debug("GetArtistURLAlternative: Single match on artist screen: strArtistURL: {0}", strArtistURL);
              return true;
            }

            var strPotentialURL = string.Empty;
            var strCleanArtist = EncodeString(CleanArtist(strArtist));

            // else there are either 0 or multiple matches so lets see how many have years populated
            foreach (Match m in matches)
            {
              var strCleanMatch = EncodeString(CleanArtist(m.Groups["artist"].ToString()));
              //logger.Debug("GetArtistURLAlternative: Cleaned/Encoded matched Artist: |{0}| compare to match |{1}|", strCleanMatch, strCleanArtist);

              if (strCleanArtist != strCleanMatch)
                continue;

              //logger.Debug("GetArtistURLAlternative: Years: {0}", m.Groups["years"].ToString().Trim());
              //if (string.IsNullOrEmpty(m.Groups["years"].ToString().Trim()))
              //  continue;

              strPotentialURL = m.Groups["artistURL"].ToString();
              numberOfMatchesWithYears++;

              // give up if more than one match with years active
              if (numberOfMatchesWithYears > 1)
              {
                strPotentialURL = matches[0].Groups["artistURL"].ToString();
                //strArtistURLs.Clear();
                for (int i = 1; i < 5; i++)
                {
                  string artURL = matches[i].Groups["artistURL"].ToString();
                  strArtistURLs.Add(artURL);
                }
                break;
              }
            }

            // No valid match found (Not sure about this check...)
            if (numberOfMatchesWithYears == 0)
            {
              return false;
            }

            // only one match with years active so return URL for that artist.
            strArtistURL = strPotentialURL; // matches[matchIndex].Groups["artistURL"].ToString();

            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }
    }

    /// <summary>
    /// Attempts to get the HTML from the artist page
    /// </summary>
    /// <param name="strArtist">Artist we are looking for</param>
    /// <param name="artistHTML">HTML of artist page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetArtistHTML(string strArtist, out String artistHTML, out String artistURL)
    {
      artistHTML = string.Empty;
      artistURL = string.Empty;

      try
      {
        String strRedirect;
        List<string> strArtistURLs = null;
        if (!GetArtistURL(strArtist, out strRedirect, out strArtistURLs))
        {
          return false;
        }
        artistURL = strRedirect;

        var x = (HttpWebRequest)WebRequest.Create(strRedirect);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;
        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              x.Abort();
              y.Close();
              return false;
            }
            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              artistHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Error retrieving artist data for: |{0}|", strArtist);
        Log.Error(ex);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Attempt to make string searching more helpful.   Removes all accents and puts in lower case
    /// Then escapes characters for use in URI
    /// </summary>
    /// <param name="strUnclean">String to be encoded</param>
    /// <returns>An encoded, cleansed string</returns>
    private string EncodeString(string strUnclean)
    {
      strUnclean = Regex.Replace(strUnclean, " {2,}", " ");
      var stFormD = strUnclean.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();

      foreach (var t in from t in stFormD let uc = CharUnicodeInfo.GetUnicodeCategory(t) where uc != UnicodeCategory.NonSpacingMark select t)
      {
        sb.Append(t);
      }
      var strClean = Uri.EscapeDataString(sb.ToString().Normalize(NormalizationForm.FormC)).ToLower();

      return strClean;
    }
    /// <summary>
    /// Improve changes of matching artist by replacing & and + with "and"
    /// on both side of comparison
    /// Also remove "The"
    /// </summary>
    /// <param name="strArtist">artist we are searching for</param>
    /// <returns>Cleaned artist string</returns>
    private static string CleanArtist(string strArtist)
    {
      var strCleanArtist = strArtist.ToLower();
      strCleanArtist = strCleanArtist.Replace("&", "and");
      strCleanArtist = strCleanArtist.Replace("+", "and");
      strCleanArtist = Regex.Replace(strCleanArtist, "^the ", "", RegexOptions.IgnoreCase);

      return strCleanArtist;
    }

    #endregion

  }
}
