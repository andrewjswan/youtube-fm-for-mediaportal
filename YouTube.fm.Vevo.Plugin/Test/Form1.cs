using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Windows.Forms;
using YouTubePlugin.DataProvider;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using YouTubePlugin;



namespace Test
{
  public partial class Form1 : Form
  {
    YouTubeService service = new YouTubeService("My YouTube Videos For MediaPortal", "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");
    YouTubeRequest request = new YouTubeRequest(new YouTubeRequestSettings("My YouTube Videos For MediaPortal", "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0", "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg"));

    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Uri ur = new Uri("http://gdata.youtube.com/feeds/api/videos/fSgGV1llVHM&f=gdata_playlists&c=ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0&d=U1YkMvELc_arPNsH4kYosmD9LlbsOl3qUImVMV6ramM");
      YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
      //order results by the number of views (most viewed first)
      query.OrderBy = "viewCount";
      
      //exclude restricted content from the search
      query.SafeSearch = YouTubeQuery.SafeSearchValues.None;
        string ss = YouTubeQuery.TopRatedVideo;
        //http://gdata.youtube.com/feeds/api/standardfeeds/top_rated
      //search for puppies!
      query.Query = textBox1.Text;
      query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));

      YouTubeFeed videoFeed = service.Query(query);
      YouTubeEntry en = (YouTubeEntry)videoFeed.Entries[0];
      string s = en.Summary.Text;
      string s1 = en.Media.Description.Value;
      Google.GData.YouTube.MediaGroup gr = en.Media;

      Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + en.VideoId);
      Video video = request.Retrieve<Video>(videoEntryUrl);
      Feed<Comment> comments = request.GetComments(video);
        string cm = "";
        foreach (Comment c in comments.Entries)
        {
          cm +=  c.Content + "\n------------------------------------------\n";
        }

      VideoInfo info = new VideoInfo();
      info.Get("yUHNUjEs7rQ");
      //Video v = request.Retrieve<Video>(videoEntryUrl);
     


      //Feed<Comment> comments = request.GetComments(v);

      //string cm = "";
      //foreach (Comment c in comments.Entries)
      //{
      //  cm += c.Author + c.Content + "------------------------------------------";
      //}


    }

    //static void printVideoFeed(YouTubeFeed feed)
    //{
    //  foreach (YouTubeEntry entry in feed.Entries)
    //  {
    //    printVideoEntry(entry);
    //  }
    //}

    //static void printVideoEntry(Video video)
    //{
    //  Console.WriteLine("Title: " + video.Title);
    //  Console.WriteLine(video.Description);
    //  Console.WriteLine("Keywords: " + video.Keywords);
    //  Console.WriteLine("Uploaded by: " + video.Uploader);
    //  if (video.YouTubeEntry.Location != null)
    //  {
    //    Console.WriteLine("Latitude: " + video.YouTubeEntry.Location.Latitude);
    //    Console.WriteLine("Longitude: " + video.YouTubeEntry.Location.Longitude);
    //  }
    //  if (video.Media != null && video.Media.Rating != null)
    //  {
    //    Console.WriteLine("Restricted in: " + video.Media.Rating.Country);
    //  }

    //  if (video.IsDraft)
    //  {
    //    Console.WriteLine("Video is not live.");
    //    string stateName = video.Status.Name;
    //    if (stateName == "processing")
    //    {
    //      Console.WriteLine("Video is still being processed.");
    //    }
    //    else if (stateName == "rejected")
    //    {
    //      Console.Write("Video has been rejected because: ");
    //      Console.WriteLine(video.Status.Value);
    //      Console.Write("For help visit: ");
    //      Console.WriteLine(video.Status.Help);
    //    }
    //    else if (stateName == "failed")
    //    {
    //      Console.Write("Video failed uploading because:");
    //      Console.WriteLine(video.Status.Value);

    //      Console.Write("For help visit: ");
    //      Console.WriteLine(video.Status.Help);
    //    }

    //    if (video.ReadOnly == false)
    //    {
    //      Console.WriteLine("Video is editable by the current user.");
    //    }

    //    if (video.RatingAverage != -1)
    //    {
    //      Console.WriteLine("Average rating: " + video.RatingAverage);
    //    }
    //    if (video.ViewCount != -1)
    //    {
    //      Console.WriteLine("View count: " + video.ViewCount);
    //    }

    //    Console.WriteLine("Thumbnails:");
    //    foreach (MediaThumbnail thumbnail in video.Thumbnails)
    //    {
    //      Console.WriteLine("\tThumbnail URL: " + thumbnail.Url);
    //      Console.WriteLine("\tThumbnail time index: " + thumbnail.Time);
    //    }

    //    Console.WriteLine("Media:");
    //    foreach (Google.GData.YouTube.MediaContent mediaContent in video.Contents)
    //    {
    //      Console.WriteLine("\tMedia Location: " + mediaContent.Url);
    //      Console.WriteLine("\tMedia Type: " + mediaContent.Format);
    //      Console.WriteLine("\tDuration: " + mediaContent.Duration);
    //    }
    //  }
    //}



    private string getIDSimple(string googleID)
    {
      string id = "";
      if (!googleID.Contains("video_id"))
      {
        int lastSlash = googleID.LastIndexOf("/");
        if (googleID.Contains("&"))
          id = googleID.Substring(lastSlash + 1, googleID.IndexOf('&') - lastSlash - 1);
        else
          id = googleID.Substring(lastSlash + 1);
      }
      else
      {
        Uri erl = new Uri(googleID);
        string[] param = erl.Query.Substring(1).Split('&');
        foreach (string s in param)
        {
          if (s.Split('=')[0] == "video_id")
          {
            id = s.Split('=')[1];
          }
        }
      }
      return id;
    }

    private void button2_Click(object sender, EventArgs e)
    {
    //    LastProfile profile = new LastProfile("", "");
    //    bool res = profile.Handshake();
    //    //profile.NowPlaying(new YouTubeEntry());
    //    profile.Submit(new YouTubeEntry());
    }

    private void button3_Click(object sender, EventArgs e)
    {
        HTBFanArt fanart = new HTBFanArt();
        fanart.Search(textBox3.Text);

    }
  }

}