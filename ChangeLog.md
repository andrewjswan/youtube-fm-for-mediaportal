## Change log ##
0.8.3.0 (10.02.2012 rev 305)
  * Remote application (remotely control the plugin from a other pc)
  * Rework of fullscreen context menu
  * Fixed some playback bugs
  * Some major gfx changes thx to ysmp and M3rcury
  * New skin tag #Youtube.fm.Curent.Video.Rating
  * Speedup artist info grabbing
  * Current logged in user videos, playlists grouped in User profile menu
  * New menu item: Feature
  * Custom fullscreen and OSD screen files (youtubeFullScreen.xml and youtubeOSD.xml)
  * New skin tag  #Youtube.fm.Next....
  * New startup parameter PLAY
  * Browse artists by tags (genres)
0.8.2.0 (14.11.2011 rev 258)
  * Browse in startup option
  * Layout change with dialog
  * Playlist selected item skin properties
  * Similar artist context menu
  * Italian translation
  * Startup parameter for search and all videos from a artist
  * Default layout option
  * Rework of VEVO videos
  * Paging for videos
  * Recently viewed artist in artist menu
  * Some minor improvement in skin files
0.8.1.0 (05.11.2011 rev 222)
  * Improved artist fanart handing
  * Disco menu item
  * Browse menu item
  * Startup option
  * Localization for artist info (only for last.fm supported lang.)
  * localization :
    * Dutch
    * Swedish
    * German
#### Skin changes (major changes) ####
  * Support for #itemcount
  * Support for #itemtype
  * New tags:
    * #Youtube.fm.Curent.Video.IsDownloaded
    * #Youtube.fm.NowPlaying.Video.IsDownloaded
    * #Youtube.fm.Info.Video.IsDownloaded
  * Default skin (4x3)
0.8.0.0 (26.10.2011 rev 199)
  * Menu item  Billboard (information grabbed from www.billboard.com )
  * Support for Watch Later youtube playlits
  * Updated Youtube API
  * Local database for storing play statistics
  * Menu item for Play Statistics
  * Context menu : Info
  * Context menu : Play next
  * Fixed: Prevent crash on startup if something wrong with user credentials
  * Item image cache remake (configurable)
  * Default image for menu items
#### Skin changes ####
  * In all screen implemented #currentmodule
  * New skin property
    * #Youtube.fm.Curent.Video.NumLike
    * #Youtube.fm.Curent.Video.NumDisLike
    * #Youtube.fm.NowPlaying.Video.NumLike
    * #Youtube.fm.NowPlaying.Video.NumDisLike
    * #Youtube.fm.Info.Video.NumLike
    * #Youtube.fm.Info.Video.NumDisLike
  * New screen youtubeinfoex.xml (Id 29053)
  * New skin properties
    * #Youtube.fm.Curent.Video.IsHD
    * #Youtube.fm.NowPlaying.Video.IsHD
    * #Youtube.fm.Info.Video.IsHD
    * #Youtube.fm.Curent.Video.IsWatched
    * #Youtube.fm.NowPlaying.Video.IsWatched
    * #Youtube.fm.Info.Video.IsWatched
    * #Youtube.fm.IsDownloading
    * #Youtube.fm.Download.Progress
    * #Youtube.fm.Download.Item
  * Support for facade view for related videos in now playing and info screen (control id 50)

0.7.3.0
  * Status Progress for saving playlist
  * Add to play lit from now playing screen
  * Some error handing when saving a playlist
  * Recently played tracks from last.fm
0.7.2.0
  * Fixed playlist saving bug
  * Fixed skin properties
  * Bigger video thumbs
0.7.1.0
  * Fixed some playlist related bugs
0.6.9.0
  * Dynamic playlist creation remade
  * Minor bug fixes
  * Translation
0.6.8.0
  * Some minor GUI Improvements
  * New default skin THX  ysmp
0.6.7.0
  * Removed MP last.fm reference using a separated lib, so need to configure last.fm from plugin
  * Artist thumbs from last.fm
  * Top played user track from last.fm
0.6.6.0
  * Minor bug fixes
  * Tree view like home menu
0.6.5.0
  * Playlist menu item
  * Improved Standard feed menu item
0.6.2.0
  * Improved now playing screen
  * Instant play feature removed
  * Option to import/export artist list
  * Renamed view button to Home
  * Reduce plugin internet traffic
  * Some improvement in playback error handing
0.6.1.0
  * Artist listing
  * Some minor bug fixes
  * MP 1.2 compatibility
0.5.9.4
  * Customizable start page
  * Vevo videos
0.5.9.3
  * Playback fixed
  * Support for Full HD videos
0.5.9.2
  * Configuration fixed
0.5.8
  * Spelling suggestions
  * Item count fixed in playlist
  * Some skin changes
0.5.6
  * Playback for non embeddable videos
  * Some navigation improvements
  * Online fanart  download fixed
0.5.4
  * Saving to Online playlis fixed
0.5.3
  * The all playlist relatated cod remaked
  * Continuos play
0.5.2
  * Possibility to download videos
0.5.1
  * Update to latest google api 1.4.0.2
  * List of video response for a video  (Context menu)
  * Add all to play list (Context menu)
0.4.1
  * Better error handing
  * Option setting in GUI (Context menu)
  * Checking of availability of HD video
0.3.7
  * Instant play - updated
0.3.6
  * Playlist bug fixed
  * Video quality selection
  * setting for sms style keyboard
  * context menu in nowplaying screen
0.3.1
  * Some bug fix - THx to Ronilse
  * Bette search result handing, sort by relevancy not views count
0.3
  * FanArt support
  * FlvSplitter compatible (Check configuration)
  * Better music videos filtering
0.2
  * Improved similar artist finding
  * Now playing screen (need a skinner to fix this)
  * Now playing song is announced to last.fm
0.1
  * Plugin name changes
  * Online playlits
0.0.2
  * Advanced configuration
  * Online favorites (all data is stored in youtube site, for this you need to configure user identification)
  * Some bug fixing
0.0.1
  * Initial release