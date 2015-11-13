# Windows #
youtubevideosbase.xml (Id **29050**) - base main screen <br />
youtubeplaylist.xml (Id **29051**) - playlist screen<br />
youtubeinfo.xml (Id **29052**) - now playing screen <br />
youtubeinfoex.xml (Id **29053**) - info screen <br />
youtube.common.window.xml - (new in **(0.8.1)**)<br />
youtube.facade.xml <br />
youtube.mediainfo.xml - (changed from youtube.common.logos.xml) **(0.8.1)** <br />
youtubeFullScreen.xml - (new in **(0.8.3)**)<br />
youtubeOSD.xml - OSD+OSD Expanded(new in **(0.8.3)**)<br />




# Button / Logos Icons #

Buttons info:<br />
-In playing now screen:<br />
1. button (id 5555) is for video full screen view.<br />
2. button (id 35) is for go to new Info screen.<br />

-In Info screen :<br />
1. button (id 96) is hiden and play the video and go to now playing screen (click enter in kyebourd/o.k in remote) this to be use if entring info screen from video selecting , wich means if you stend on a video lets say in listview click f9/info you go to info screen first with no playing the video .<br />

Logos:<br />

the tags ...IsHD & ...IsWatched & ...IsDownloaded, to be use with Icon/logo , and set as if are equal to true the video have hd and full hd video stream / or this video was watch or not /or this video was downloaded or not, and toggle the visibility depending on this properties is true or not.


-Control id 50 type change to facade in now playing (youtubeinfo.xml) **(0.8.0)**<br />
The code Change to Facade wich mean you can view this list with all facade views , with a code change.

Exsample for Fimstrip view:<br />

1- Change the code 

&lt;define&gt;

#viewmode:coverflow

&lt;/define&gt;

 TO 

&lt;define&gt;

#viewmode:filmstrip

&lt;/define&gt;

(on top of the xml)<br />

2- Reaplace the all code of the coverflow with fimstrip code (id 50)<br />
Then you can have filmstrip view insted of coverflow.. and so on for others views<br />

Note: only one view can work (not 2 defrent view/code)<br />

-View now playing (artist/artist image/video title) and next (artist/video title)this tags view only 10 sec' when a video start playing in full screen view ,then go off screen . use the code:


&lt;visible&gt;

string.equals(#Youtube.fm.FullScreen.ShowTitle, true )

&lt;/visible&gt;

 for playing now info.


&lt;visible&gt;

string.equals(#Youtube.fm.FullScreen.ShowNextTitle, true )

&lt;/visible&gt;

 for next playing info.







# Exposed labels #

### In all windows ###
**For selected item : (all this properties are available only in standard feed listing, other listing some properties may not initialized   )
```
      #Youtube.fm.Curent.Video.Title
      #Youtube.fm.Curent.Video.Duration
      #Youtube.fm.Curent.Video.PublishDate
      #Youtube.fm.Curent.Video.Autor
      #Youtube.fm.Curent.Video.ViewCount
      #Youtube.fm.Curent.Video.WatchCount
      #Youtube.fm.Curent.Video.FavoriteCount
      #Youtube.fm.Curent.Video.Rating
      #Youtube.fm.Curent.Video.NumLike
      #Youtube.fm.Curent.Video.NumDisLike
      #Youtube.fm.Curent.Video.PercentLike
      #Youtube.fm.Curent.Video.Image
      #Youtube.fm.Curent.Video.Summary
      #Youtube.fm.Curent.Video.IsHD
      #Youtube.fm.Curent.Video.IsWatched
      #Youtube.fm.Curent.Video.IsDownloaded  *(0.8.1)*

```**

**For now playing item (youtubeinfo.xml)
```
      #Youtube.fm.NowPlaying.Artist.Name
      #Youtube.fm.NowPlaying.Video.Title
      #Youtube.fm.NowPlaying.Video.Duration
      #Youtube.fm.NowPlaying.Video.PublishDate
      #Youtube.fm.NowPlaying.Video.Autor
      #Youtube.fm.NowPlaying.Video.ViewCount
      #Youtube.fm.NowPlaying.Video.WatchCount
      #Youtube.fm.NowPlaying.Video.FavoriteCount
      #Youtube.fm.NowPlaying.Video.Rating
      #Youtube.fm.NowPlaying.Video.NumLike
      #Youtube.fm.NowPlaying.Video.NumDisLike
      #Youtube.fm.NowPlaying.Video.PercentLike
      #Youtube.fm.NowPlaying.Video.Image
      #Youtube.fm.NowPlaying.Video.Comments
      #Youtube.fm.NowPlaying.Video.FanArt
      #Youtube.fm.NowPlaying.Video.Summary
      #Youtube.fm.NowPlaying.Video.IsHD
      #Youtube.fm.NowPlaying.Video.IsWatched
      #Youtube.fm.NowPlaying.Video.IsDownloaded  *(0.8.1)*

```**

**For Info item (youtubeinfoex.xml)
```
      #Youtube.fm.Info.Video.Title
      #Youtube.fm.Info.Video.Duration
      #Youtube.fm.Info.Video.PublishDate
      #Youtube.fm.Info.Video.Autor
      #Youtube.fm.Info.Video.ViewCount
      #Youtube.fm.Info.Video.WatchCount
      #Youtube.fm.Info.Video.FavoriteCount
      #Youtube.fm.Info.Video.Rating
      #Youtube.fm.Info.Video.NumLike
      #Youtube.fm.Info.Video.NumDisLike
      #Youtube.fm.Info.Video.PercentLike
      #Youtube.fm.Info.Video.Image
      #Youtube.fm.Info.Video.Comments
      #Youtube.fm.Info.Video.FanArt
      #Youtube.fm.Info.Video.Summary
      #Youtube.fm.Info.Video.IsHD
      #Youtube.fm.Info.Video.IsWatched
      #Youtube.fm.Info.Video.IsDownloaded  *(0.8.1)*
      #Youtube.fm.Info.Artist.Name
      #Youtube.fm.Info.Artist.Tags
      #Youtube.fm.Info.Artist.Bio
      #Youtube.fm.Info.Artist.Image

```**

**For Downloading info + video Rating (Can be use in All screens)**(0.8.0)**```
      #Youtube.fm.IsDownloading
      #Youtube.fm.Download.Progress
      #Youtube.fm.Download.Item 
      #Youtube.fm.Curent.Video.Rating  (view as:8.5/10) *(0.8.3)*


```**

**For Full Screen (youtubeFullScreen.xml)**(0.8.3)