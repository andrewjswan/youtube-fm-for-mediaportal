using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Globalization;
using AxShockwaveFlashObjects;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Utils;
using System.Xml;

namespace YouTubePlayer
{
	public class YouTubePlayer : IExternalPlayer
	{
		//public static bool _playerIsPaused;
		string _currentFile = String.Empty;
		//bool _started;
		//bool _ended;
		bool _notifyPlaying = false;
		double _duration;
		double _currentPosition;
		DateTime _updateTimer;
		bool _needUpdate = true;
		//private string[] _supportedExtension = new string[5];

		bool _isFullScreen = false;
		int _positionX = 10, _positionY = 10, _videoWidth = 100, _videoHeight = 100;
		//protected ILog _log;

		public static FlashControl FlvControl = null;

		public enum PlayState
		{
			Init,
			Playing,
			Paused,
			Ended
		}
		PlayState _playState = PlayState.Playing;
    public YouTubePlayer()
		{
			//ServiceProvider services = GlobalServiceProvider.Instance;
			//_log = services.Get<ILog>();
		}

		public override string Description()
		{
      return "YouTube Video Flash Player";
		}

		public override string PlayerName
		{
			get { return "YouTube Video Player"; }
		}

		public override string AuthorName
		{
			get { return "Dukus, based on work of Gregmac45"; }
		}

		public override string VersionNumber
		{
			get { return "0.1"; }
		}

    public override void ShowPlugin()
    {
      FlashplayerConfig dlg = new FlashplayerConfig();
      dlg.ShowDialog();
    }

		public override string[] GetAllSupportedExtensions()
		{
			String[] laSupported = new String[1];
			laSupported[0] = ".flv";
      laSupported[1] = "flv";
			return laSupported;
		}
    //public override
		/*
        public string[] GetAllSupportedStreams()
        {
        	String[] laSupported = new String[2];
            laSupported[0] = "http";
            laSupported[0] = "RTMP";
            return laSupported;
         
        }
		 */
		public override bool SupportsFile(string filename)
		{
      //string ext = null;
      //int dot = filename.LastIndexOf(".");    // couldn't find the dot to get the extension
      //if (dot == -1) return false;

      //ext = filename.Substring(dot).Trim();
      //if (ext.Length == 0) return false;   // no extension so return false;

      //ext = ext.ToLower();
      //if (".flv".Equals(ext)) { return true; };

      if (filename.Contains("http://www.youtube.com/v"))
        return true;
      else
        return false;
		}

    private string getIDSimple(string googleID)
    {
      int lastSlash = googleID.LastIndexOf("/");
      return googleID.Substring(lastSlash + 1);
    }

		public override bool Play(string strFile)
		{
			Log.Info("Playing flv with YoutubeFlvPlayer :{0}",strFile);
			try
			{
        //Uri site = new Uri(strFile);
        if (FlvControl != null)
        {
        }

        //string[] param = site.Query.Substring(1).Split('&');
        string video_id = getIDSimple(strFile);
       
        FlvControl = new FlashControl();
        FlvControl.Player.AllowScriptAccess = "always";
        
				FlvControl.Player.FlashCall += new _IShockwaveFlashEvents_FlashCallEventHandler(OnFlashCall);
        FlvControl.Player.FSCommand += new _IShockwaveFlashEvents_FSCommandEventHandler(Player_FSCommand);
        //FlvControl.Player.FlashVars = site.Query.Replace("?", "&") + "&eh=myCallbackEventHandler&token=AA7Zx0hd0JeezyPS6TqLB0_1UnEUMZybQEmzIAWcIa7UHPEHNg--";
        FlvControl.Player.OnProgress += new _IShockwaveFlashEvents_OnProgressEventHandler(Player_OnProgress);
        FlvControl.Player.AllowScriptAccess = "always";
        FlvControl.Player.LoadMovie(0, string.Format("http://www.youtube.com/v/{0}&color1=0x2b405b&color2=0x6b8ab6&fs=1&autoplay=1&enablejsapi=1&playerapiid=ytplayer", video_id));
        FlvControl.Player.AllowScriptAccess = "always";
        //FlvControl.Player.SetVariable("allowScriptAccess", "always");
        //FlvControl.Player.LoadMovie(0, strFile);

        //FlvControl.Player.CallFunction("<invoke name=\"addEventListener\"><arguments><string>\"onStateChange\"</string><string>\"onStateChange\"</string></arguments></invoke>");

				//FlvControl.Player.Movie = System.IO.Directory.GetCurrentDirectory()+"\\player.swf";
        		

				GUIGraphicsContext.form.Controls.Add(FlvControl);
				GUIWindowManager.OnNewAction +=new OnActionHandler(OnAction2);
				
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
				msg.Label = strFile;
				GUIWindowManager.SendThreadMessage(msg);
				_notifyPlaying = true;
				FlvControl.ClientSize = new Size(0, 0);
				FlvControl.Visible = true;
				FlvControl.Enabled = false;
        FlvControl.SendToBack();
				_needUpdate = true;
				_isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
				_positionX = GUIGraphicsContext.VideoWindow.Left;
				_positionY = GUIGraphicsContext.VideoWindow.Top;
				_videoWidth = GUIGraphicsContext.VideoWindow.Width;
				_videoHeight = GUIGraphicsContext.VideoWindow.Height;

				_currentFile = strFile;
				_duration = -1;
				_currentPosition = -1;
				_playState = PlayState.Playing;
				_updateTimer = DateTime.Now;
				SetVideoWindow();
				return true;
			}
			catch (Exception ex)
			{
        Log.Error("Flv on Play Error {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
			}
			return false;
		}


    void Player_OnProgress(object sender, _IShockwaveFlashEvents_OnProgressEvent e)
    {
      Log.Debug("On progres {0}", e.percentDone.ToString());
    }

    void Player_FSCommand(object sender, _IShockwaveFlashEvents_FSCommandEvent e)
    {
      Log.Info("OnFsCommand reached with value request:{0},Object:{1}", e.args, e.command);
    }

    private void GetState()
    {
      if (FlvControl == null)
        return;
      try
      {
        string restp = FlvControl.Player.CallFunction("<invoke name=\"getPlayerState\" returntype=\"xml\"></invoke>");
        XmlDocument document = new XmlDocument();
        document.LoadXml(restp);
        XmlNode list = document.SelectSingleNode("number");
        switch (list.InnerText)
        {
          case "0":
            _playState = PlayState.Ended;
            break;
          case "1":
            _playState = PlayState.Playing;
            break;
          case "2":
            _playState = PlayState.Paused;
            break;
          case "3":
            _playState = PlayState.Paused;
            break;
        }
        //Log.Debug("Player state {0}", restp);
      }
      catch
      {

      }
    }


		public void OnFlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent foEvent)
		{
      //Log.Info("OnFlashCall reached with value request:{0},Object:{1}", foEvent.request, sender);
			XmlDocument document = new XmlDocument();
			document.LoadXml(foEvent.request);
      Log.Debug("FLV event {0}", foEvent.request);
			// Get all the arguments
			XmlNodeList list = document.GetElementsByTagName("invoke");
			String lsName = list[0].Attributes["name"].Value;
			list = document.GetElementsByTagName("arguments");
			String lsState = list[0].FirstChild.InnerText;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_ITEM, 0, 0, 0, 0, 0, null); 
      if (lsName.Equals("myCallbackEventHandler"))
			{
				switch (lsState)
				{
          case "itemBegin":
            _playState = PlayState.Playing;
//            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_ITEM, 0, 0, 0, 0, 0, null);
            msg.Object = foEvent.request;
            GUIWindowManager.SendThreadMessage(msg);
            break;
          case "itemEnd":
//            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_ITEM, 0, 0, 0, 0, 0, null);
            msg.Object = foEvent.request;
            GUIWindowManager.SendThreadMessage(msg);
            //_playState = PlayState.Ended;
            break;
          case "done":
            _playState = PlayState.Ended;
            break;
          case "init":
            _playState = PlayState.Init;
            break;
          case "streamPlay":
            _playState = PlayState.Playing;
            break;
          case "streamPause":
            _playState = PlayState.Paused;
            break;
          case "streamStop":
            _playState = PlayState.Ended;
            break;
          case "streamError":
            _playState = PlayState.Ended;
            break;

          //switch(list[0].ChildNodes[1].InnerText){
            //  case "0":
            //  case "1":
            //    if(_playState == PlayState.Playing){
            //      _playState = PlayState.Paused;
            //    }
            //    else {
            //      _playState = PlayState.Init;
            //    }
            //    break;
            //  case "2":
            //    _playState = PlayState.Playing;
            //    break;
            //  case "3":
            //    _playState = PlayState.Ended;
            //    break;
            //}
						
						
	//					break;
					case "time":
						_currentPosition = Convert.ToInt32(list[0].ChildNodes[1].InnerText);
						_duration = _currentPosition+ Convert.ToInt32(list[0].ChildNodes[2].InnerText);
						
						break;
						
				}
			}
		}
		public override  bool CanSeek()
		{
			return true;
		}

		public void OnAction2(Action foAction)
		{
      if (foAction.wID == Action.ActionType.ACTION_SHOW_GUI || foAction.wID == Action.ActionType.ACTION_SHOW_FULLSCREEN)
      {
        SetVideoWindow();
      }
      
      if (foAction.wID == Action.ActionType.ACTION_NEXT_ITEM)
      {
        try
        {
          if (_playState == PlayState.Playing || _playState == PlayState.Paused)
          {
            Log.Debug("Flv Stop {0}", FlvControl.Player.CallFunction("<invoke name=\"playNext\"></invoke>"));
          }
        }
        catch
        {

        }
      }
      
      if (foAction.wID == Action.ActionType.ACTION_PREV_ITEM)
      {
        try
        {
          if (_playState == PlayState.Playing || _playState == PlayState.Paused)
          {
            Log.Debug("Flv Stop {0}", FlvControl.Player.CallFunction("<invoke name=\"playPrevious\"></invoke>"));
          }
        }
        catch
        {
        }
      }
		}

		public override double Duration
		{
			get
			{
				return _duration;
			}
		}

    
		public override double CurrentPosition
		{
			get
			{
				if (FlvControl == null) return 0.0d;
        if (_playState != PlayState.Playing)
          return 0.0d;
				//UpdateStatus();
				//if (_started == false) return 0;
				try
				{
          NumberFormatInfo provider = new NumberFormatInfo();
          provider.NumberDecimalSeparator = ".";
          provider.NumberGroupSeparator = ",";
          provider.NumberGroupSizes = new int[] { 3 };

          string restp = FlvControl.Player.CallFunction("<invoke name=\"getCurrentTime\" returntype=\"xml\"></invoke>");
          XmlDocument document = new XmlDocument();
          document.LoadXml(restp);
          // Get all the arguments
          XmlNode list = document.SelectSingleNode("number");
          String lsName = list.InnerText;
          //Log.Debug("flv getVidState :{0}", FlvControl.Player.CallFunction("<invoke name=\"getVidState\" returntype=\"xml\"></invoke>"));
          return Convert.ToDouble(lsName, provider);
          //_updateTimer = DateTime.Now;
					//return _currentPosition;
					//Log.Info("Flash Player - current time {0}", _currentPosition);
					
					//}
				}
				catch (Exception ex )
				{
          //Log.Error("Flv on pause error {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);

        }
				try
				{
					return _currentPosition;
				}
				catch (Exception)
				{
					//FlvControl = null;
					return 0.0d;
				}
			}

		}
		/*
        public override int Speed
        {
            get
            {
                if (_playState == PlayState.Init) return 1;
                if (FlvControl == null) return 1;
                int liSpeed = 1;
                try
                {
                    String lsXml = FlvControl.Player.CallFunction("<invoke name=\"getVideoSpeed\" returntype=\"string\"></invoke>");
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(lsXml);
                    XmlNodeList list = document.GetElementsByTagName("number");
                    liSpeed = Convert.ToInt32(list[0].InnerText);
                    //Log.Info("lsTest - total time {0}", lsTest);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }

                return liSpeed;
            }
            set
            {
                if (FlvControl == null) return;
                if (_playState != PlayState.Init)
                {
                    if (value < 0)
                    {
                        int liNewSpeed = Speed + value;
                        try
                        {
                            FlvControl.Player.CallFunction("<invoke name=\"setVideoSpeed\" returntype=\"string\"><arguments><number>" + liNewSpeed + "</number></arguments></invoke>");
                            //XmlDocument document = new XmlDocument();
                            //document.LoadXml(lsXml);
                            //XmlNodeList list = document.GetElementsByTagName("number");
                            //_currentPosition = Convert.ToDouble(list[0].InnerText);
                            //Log.Info("lsTest - total time {0}", lsTest);
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                        
                    }
                    else
                    {
                        try
                        {
                            FlvControl.Player.CallFunction("<invoke name=\"setVideoSpeed\" returntype=\"string\"><arguments><number>" + value + "</number></arguments></invoke>");
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                }
            }
        }
		 * */
		public override bool Ended
		{
			get
			{
				return (_playState==PlayState.Ended);
			}
		}
		public override bool Playing
		{
			get
			{
				try
				{
					if (FlvControl == null)
						return false;
          GetState();
					//UpdateStatus();
					//String lsPlaying = FlvControl.Player.CallFunction("<invoke name=\"videoPlaying\" returntype=\"xml\"></invoke>");
					//Log.Info("Flv Playing:{0}", lsPlaying);
					//Log.Info("Player.playing:{0}", FlvControl.Player.Playing);
					//Log.Info("control.playing:{0}", FlvControl.Playing);
					//if (_started == false)
					//    return false;
					//if (Paused) return true;
					//return true;
          return (_playState == PlayState.Playing || _playState == PlayState.Paused || _playState == PlayState.Init);


				}
				catch (Exception ex)
				{
          Log.Error("Flv on pause error {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);

					return false;
				}

			}
		}

		public override void Pause()
		{
			if (FlvControl == null) return;
      GetState();
			//if (_started == false) return;
			try
			{
				if (_playState == PlayState.Paused)
				{
					//FlvControl.Player.Play();
          FlvControl.Player.CallFunction("<invoke name=\"playVideo\"></invoke>");
					//FlvControl.Player.CallFunction("<invoke name=\"playpause\" returntype=\"xml\"></invoke>");
          //FlvControl.Player.CallFunction("<invoke name=\"sendEvent\" returntype=\"xml\"><arguments><string>playpause</string></arguments></invoke>");
					//_playerIsPaused = false;
				}
				else
				{
          FlvControl.Player.CallFunction("<invoke name=\"pauseVideo\"></invoke>");
					//FlvControl.Player.CallFunction("<invoke name=\"playpause\" returntype=\"xml\"></invoke>");
					//FlvControl.Player.CallFunction("<invoke name=\"sendEvent\" returntype=\"xml\"><arguments><string>playpause</string></arguments></invoke>");
					//FlvControl.Player.StopPlay();
					//_playerIsPaused = true;
				}
			}
			catch (Exception ex)
			{
				FlvControl = null;
        Log.Error("Flv on pause error {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
				return;
			}
		}

		public override bool Paused
		{
			get
			{
				try
				{
					//if (_started == false) return false;
					//return _playerIsPaused;
					return (_playState == PlayState.Paused);
				}
				catch (Exception ex)
				{
					FlvControl = null;
          Log.Error("Flv on pause error {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
					return false;
				}
			}
		}


		public override string CurrentFile
		{
			get
			{
				return _currentFile;
			}
		}


		public override void Process()
		{
			//Log.Info("in Process");
			//UpdateStatus();
			if (_needUpdate)
			{
				SetVideoWindow();
			}
			if (CurrentPosition >= 10.0)
			{
				if (_notifyPlaying)
				{
					_notifyPlaying = false;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
          msg.Label = "";
          GUIWindowManager.SendThreadMessage(msg);
          //Log.Info("Message Playing 10 sec sent");
				}
			}
		}

		public override void  SetVideoWindow()
		{
			
			if (FlvControl == null) return;
			if (GUIGraphicsContext.IsFullScreenVideo != _isFullScreen)
			{
				_isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
				_needUpdate = true;
			}
			if (!_needUpdate) return;
			_needUpdate = false;


			if (_isFullScreen)
			{
				Log.Info("Flv:Fullscreen");

				_positionX = GUIGraphicsContext.OverScanLeft;
				_positionY = GUIGraphicsContext.OverScanTop;
				_videoWidth = GUIGraphicsContext.OverScanWidth;
				_videoHeight = GUIGraphicsContext.OverScanHeight;

				FlvControl.Location = new Point(0, 0);
				FlvControl.ClientSize = new System.Drawing.Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
				FlvControl.Size = new System.Drawing.Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);

				_videoRectangle = new Rectangle(0, 0, FlvControl.ClientSize.Width, FlvControl.ClientSize.Height);
				_sourceRectangle = _videoRectangle;
        //FlvControl.Player.CallFunction("<invoke name=\"setSize\" returntype=\"xml\"><arguments><number>" + GUIGraphicsContext.Width.ToString() + "</number><number>" + GUIGraphicsContext.Height.ToString() + "</number></arguments></invoke>");

				//FlvControl.fullScreen=true;
				//FlvControl.stretchToFit = true;
				//Log.Write("FlvPlayer:done");
				return;
			}
			else
			{

				FlvControl.ClientSize = new System.Drawing.Size(_videoWidth, _videoHeight);
				FlvControl.Location = new Point(_positionX, _positionY);

				_videoRectangle = new Rectangle(_positionX, _positionY, FlvControl.ClientSize.Width, FlvControl.ClientSize.Height);
				_sourceRectangle = _videoRectangle;
				//Log.Write("AudioPlayer:set window:({0},{1})-({2},{3})",_positionX,_positionY,_positionX+FlvControl.ClientSize.Width,_positionY+FlvControl.ClientSize.Height);
			}
			
			//GUIGraphicsContext.form.Controls[0].Enabled = false;
		}

		public override void Stop()
		{
      Log.Info("Attempting to stop...{0}", FlvControl);
			if (FlvControl == null) return;
			try
			{
				//Log.Info("before {0}", Playing);
        //FlvControl.Player.Stop();
        FlvControl.Player.StopPlay();
        FlvControl.Player.DisableLocalSecurity();
        try
        {
          if (_playState == PlayState.Playing || _playState == PlayState.Paused)
          {
            FlvControl.Player.CallFunction("<invoke name=\"stopVideo\"></invoke>");
          }
        }
        catch
        {
        }
        FlvControl.Player.Visible = false;
				FlvControl.Visible = false;
				FlvControl.ClientSize = new Size(0, 0);
        GUIGraphicsContext.form.Controls[0].Enabled = false;
        GUIGraphicsContext.form.Controls[0].Visible = false;
        FlvControl.Dispose();
        //_playerIsPaused = false;
        //_started = false;
        //Playing = false;
				//GUIGraphicsContext.OnNewAction -= new OnActionHandler(OnAction2);
				//Log.Info("after {0}", Playing);
			}
			catch (Exception ex)
			{
        Log.Error("Flv on stop error {0} \n {1} \n {2}", ex.Message, ex.Source, ex.StackTrace);

				FlvControl = null;
			}
		}

		public override bool HasVideo
		{
			get
			{
				return true;
			}
		}

		public override bool FullScreen
		{
			
			get
			{
				return _isFullScreen;
			}
			set
			{
				if (value != _isFullScreen)
				{
					//Log.Info("setting fullscreen to {0}", value);
					_isFullScreen = value;
					_needUpdate = true;
				}
			}
		}

		public override int PositionX
		{
			get { return _positionX; }
			set
			{
				if (value != _positionX)
				{
					//Log.Info("setting position x to {0}", value);
					_positionX = value;
					_needUpdate = true;
				}
			}
		}

		public override int PositionY
		{
			get { return _positionY; }
			set
			{
				if (value != _positionY)
				{
					//Log.Info("setting position y to {0}", value);
					_positionY = value;
					_needUpdate = true;
				}
			}
		}

		public override int RenderWidth
		{
			get { return _videoWidth; }
			set
			{
				if (value != _videoWidth)
				{
					//Log.Info("setting width to {0}", value);
					_videoWidth = value;
					_needUpdate = true;
				}
			}
		}
		public override int RenderHeight
		{
			get { return _videoHeight; }
			set
			{
				if (value != _videoHeight)
				{
					//Log.Info("setting position height to {0}", value);
					_videoHeight = value;
					_needUpdate = true;
				}
			}
		}



		
		public override void SeekRelative(double dTime)
		{
			double dCurTime = CurrentPosition;
			dTime = dCurTime + dTime;
			if (dTime < 0.0d) dTime = 0.0d;
			if (dTime < Duration)
			{
				SeekAbsolute(dTime);
			}
		}

		public override void SeekAbsolute(double dTime)
		{
			if (dTime < 0.0d) dTime = 0.0d;
			if (dTime < Duration)
			{
				if (FlvControl == null) return;
				try
				{
					Log.Info("Attempting to seek...");
          //FlvControl.Player.CallFunction("<invoke name=\"vidSeek\" returntype=\"xml\"><arguments><number>" + dTime + "</number></arguments></invoke>");
          //FlvControl.Player.CallFunction("<invoke name=\"scrub\" returntype=\"xml\"><arguments><number>" + dTime + "</number></arguments></invoke>");
          //FlvControl.Player.CallFunction("<invoke name=\"sendEvent\" returntype=\"xml\"><arguments><string>scrub</string><number>" + dTime + "</number></arguments></invoke>");
					//FlvControl.Player.CurrentFrame() = (int)dTime;
					Log.Info("seeking complete");
				}
				catch (Exception e) {
					Log.Error(e);
				}
			}
		}

		public override void SeekRelativePercentage(int iPercentage)
		{
			double dCurrentPos = CurrentPosition;
			double dDuration = Duration;

			double fCurPercent = (dCurrentPos / Duration) * 100.0d;
			double fOnePercent = Duration / 100.0d;
			fCurPercent = fCurPercent + (double)iPercentage;
			fCurPercent *= fOnePercent;
			if (fCurPercent < 0.0d) fCurPercent = 0.0d;
			if (fCurPercent < Duration)
			{
				SeekAbsolute(fCurPercent);
			}
		}


		public override void SeekAsolutePercentage(int iPercentage)
		{
			if (iPercentage < 0) iPercentage = 0;
			if (iPercentage >= 100) iPercentage = 100;
			double fPercent = Duration / 100.0f;
			fPercent *= (double)iPercentage;
			SeekAbsolute(fPercent);
		}
		

	}
}
