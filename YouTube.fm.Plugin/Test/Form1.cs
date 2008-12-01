using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;


namespace Test
{
  public partial class Form1 : Form
  {
    YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal", "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");

    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Uri ur = new Uri("http://gdata.youtube.com/feeds/api/videos/fSgGV1llVHM&f=gdata_playlists&c=ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0&d=U1YkMvELc_arPNsH4kYosmD9LlbsOl3qUImVMV6ramM");
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.CreateFavoritesUri(null));
      service.setUserCredentials("dukusi@gmail.com", "mikimiki");
      //order results by the number of views (most viewed first)
      query.OrderBy = "viewCount";
      
      //exclude restricted content from the search
      query.Racy = "exclude";

      //search for puppies!
//      query.VQ = "puppy";
      query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));

      YouTubeFeed videoFeed = service.Query(query);

      query = new YouTubeQuery(YouTubeQuery.CreatePlaylistsUri(null));
      PlaylistsFeed userPlaylists = service.GetPlaylists(query);



      foreach (PlaylistsEntry entry in userPlaylists.Entries)
      {
        if (entry.Title.Text == "test 2")
        {
          YouTubeQuery playlistQuery = new YouTubeQuery(entry.FeedLink.Href);
          PlaylistFeed playlistFeed = service.GetPlaylist(playlistQuery);

          foreach (YouTubeEntry playlistEntry in playlistFeed.Entries)
          {
            
            //AddItemToPlayList((YouTubeEntry)playlistEntry, ref playList);
          }
        }
      }
    }

    private string getIDSimple(string googleID)
    {
      int lastSlash = googleID.LastIndexOf("/");
      return googleID.Substring(lastSlash + 1);
    }
  }

}