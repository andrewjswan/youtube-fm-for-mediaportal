
using System;
using System.Collections.Generic;
using System.Text;
using Google.GData.Client;
using Google.GData.YouTube;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

using MediaPortal.Video.Database;
using SearchPlugin.Classes;
using SearchPlugin.Providers;
using SQLite.NET;

namespace YouTubePlugin
{
    public class SearchProvider : BaseProvider
    {
        public override event SearchDoneEventHandler SearchDone;

        public SearchProvider()
        {
            SearchResult = new SearchResultCollection();
            DisplayName = "YouTube.Fm";
            Version = new Version(0, 1);
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void Search(object term)
        {
            YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
            //query.VQ = searchString;
            query.Query = term.ToString();
            query.OrderBy = "relevance";
            query.Categories.Add(new QueryCategory("Music", QueryCategoryOperator.AND));

            YouTubeFeed vidr =Youtube2MP.service.Query(query);

            SearchResult.Items.Clear();
            foreach (YouTubeEntry entry in vidr.Entries)
            {
                SearchResultItem item = new SearchResultItem();
                item.Id = entry.VideoId ;
                item.Label = entry.Title.Text;
                item.Provider = this;
                item.MetaData.Add("entry", entry);
                SearchResult.Items.Add(item);
            }
            IsBusy = false;
            if (SearchDone != null)
                SearchDone(this);
        }

        public override void Play(SearchResultItem resultItem)
        {
            YouTubeEntry entry = resultItem.MetaData["entry"] as YouTubeEntry;
            YoutubeGUIBase.SetLabels(entry, "NowPlaying");
            Youtube2MP.NowPlayingEntry = entry;
            VideoInfo info = new VideoInfo();
            g_Player.PlayVideoStream(Youtube2MP.StreamPlaybackUrl(entry, info));
            if (g_Player.Playing)
            {
                if (Youtube2MP._settings.ShowNowPlaying)
                {
                    GUIWindowManager.ActivateWindow(29052);
                }
                else
                {
                    g_Player.ShowFullScreenWindow();
                }
            }

            if (!g_Player.Playing)
            {
                GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                if (dlgOK != null)
                {
                    dlgOK.SetHeading(25660);
                    dlgOK.SetLine(1, "Unable to playback the item ! ");
                    dlgOK.SetLine(2, "");
                    dlgOK.DoModal(GUIWindowManager.ActiveWindow);
                }
            }
        }

        public override void ShowDetail(SearchResultItem resultItem)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            Youtube2MP.service = new YouTubeService("My YouTube Videos For MediaPortal",
                                                    "ytapi-DukaIstvan-MyYouTubeVideosF-d1ogtvf7-0",
                                                    "AI39si621gfdjmMcOzulF3QlYFX_vWCqdXFn_Y5LzIgHolPoSetAUHxDPx8u4YXZVkU7CmeiObnzavrsjL5GswY_GGEmen9kdg");
            Youtube2MP._settings = new Settings();
            Youtube2MP._settings.Load();
            Version = new Version(0, 0, 1);
            IsBusy = false;
        }
    }
}
