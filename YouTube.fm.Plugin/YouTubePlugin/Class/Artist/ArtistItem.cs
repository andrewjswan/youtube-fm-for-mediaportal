using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTubePlugin.Class.Artist
{
  public class ArtistItem
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public int Db_id { get; set; }

    public ArtistItem()
    {
      Db_id = -1;
    }
  }
}
