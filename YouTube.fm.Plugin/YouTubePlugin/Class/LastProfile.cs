using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading;
using System.Windows.Forms;

using MediaPortal.Music.Database;

using Google.GData.YouTube;
using Lastfm.Services;

namespace YouTubePlugin
{

    public class LastProfile
    { 
        private string username;
        private string password;
        private Session session;
        private Lastfm.Scrobbling.Connection connection;
        private Lastfm.Scrobbling.ScrobbleManager manager;

        public LastProfile(string username, string password)
        {
            this.username = username;
            this.password = password;
            session = new Session("60d35bf7777d870ec958a21872bacb24", "099158e5216ad77239be5e0a2228cf04");
            session.Authenticate(this.username, Lastfm.Utilities.md5(this.password));
            connection = new Lastfm.Scrobbling.Connection("mpm", Assembly.GetEntryAssembly().GetName().Version.ToString(), this.username, session);
            manager = new Lastfm.Scrobbling.ScrobbleManager(connection);
           }

        public bool Handshake()
        {
            connection.Initialize();
            return session.Authenticated;
        }

        public void NowPlaying(YouTubeEntry song)
        {
            try
            {
                Lastfm.Scrobbling.NowplayingTrack track = new Lastfm.Scrobbling.NowplayingTrack("madonna", "test");
                track.Album = "valami";
                track.Duration = new TimeSpan(0, 3, 22);
                manager.ReportNowplaying(track);

               
            }
            catch
            {
            }
        }

        public void Submit(YouTubeEntry song)
        {
            try
            {
                Lastfm.Scrobbling.Entry track = new Lastfm.Scrobbling.Entry("madonna", "test", DateTime.Now, Lastfm.Scrobbling.PlaybackSource.User, new TimeSpan(0, 2, 32), Lastfm.Scrobbling.ScrobbleMode.Played);
                manager.Queue(track);
                //manager.Submit();
            }
            catch
            {
            }
        }

        public List<string> SimilarArtists(Song song)
        {
            return new List<string>();
        }
    }
}
