Starting with MediaPortal? 1.2 a designer can add a button in the skinfile that calls any Plugin with a parameter.  This allows calling Youtube.FM for search on youtube, or show videos from a specified artist or play youtube video.

Show all youtube videos of a artist, if the artist don't found in database will do a simple search with artist name
```
<control>
<description>More videos from artist</description>
<type>button</type>
<hyperlink>29050</hyperlink>
<hyperlinkParameter>ARTISTVIDEOS:Artist Name</hyperlinkParameter>
</control>
```
Search music videos with specified search term :
```
<control>
<description>home BM Video</description>
<type>button</type>
<hyperlink>29050</hyperlink>
<hyperlinkParameter>SEARCH:Search Term</hyperlinkParameter>
</control>
```
Play video with specified youtube url :
```
<control>
<description>home BM Video</description>
<type>button</type>
<hyperlink>29050</hyperlink>
<hyperlinkParameter>PLAY:Youtube Url</hyperlinkParameter>
</control>
```