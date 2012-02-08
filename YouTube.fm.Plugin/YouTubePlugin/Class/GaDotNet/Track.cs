using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using GaDotNet.Common;
using GaDotNet.Common.Data;
using GaDotNet.Common.Helpers;
using MediaPortal.GUI.Library;

namespace YouTubePlugin.Class.GaDotNet
{
  public static class Track
  {
    public static void TrackStartup()
    {
      try
      {
        GooglePageView pageView = new GooglePageView("youtube.fm",
                                   "youtube.fm",
                                   "/default.html");
        TrackingRequest request = new RequestFactory().BuildRequest(pageView);
        request.AnalyticsAccountCode = "";
        GoogleTracking.FireTrackingEvent(request);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public static void TrackPlay()
    {
      try
      {
        GooglePageView pageView = new GooglePageView("youtube.fm",
                                   "youtube.fm",
                                   "/play.html");
        TrackingRequest request = new RequestFactory().BuildRequest(pageView);
        request.AnalyticsAccountCode = "UA-29014752-2";
        GoogleTracking.FireTrackingEvent(request);
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }
  }
}
