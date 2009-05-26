using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace YouTubePlugin
{
  [Serializable]
  public class LocalFileEnumerator
  {

    private List<LocalFileStruct> items;

    public List<LocalFileStruct> Items
    {
      get { return items; }
      set { items = value; }
    }

    
    public LocalFileEnumerator()
    {
      Items = new List<LocalFileStruct>();
    }

    public void Save()
    {
      string filename = Config.GetFile(Config.Dir.Config, "youtube.xml");
      try
      {
        XmlSerializer serializer = new XmlSerializer(typeof(LocalFileEnumerator));
        TextWriter writer = new StreamWriter(filename);
        serializer.Serialize(writer, this);
        writer.Close();
      }
      catch (Exception ex)
      {
          Log.Error(ex);   
      }
    }


    public LocalFileStruct Get(string id)
    {
      foreach (LocalFileStruct lf in Items)
      {
        if (lf.VideoId == id)
          return lf;
      }
      return null;
    }

    public void Load()
    {
      string filename = Config.GetFile(Config.Dir.Config, "youtube.xml");
      try
      {
        LocalFileEnumerator lfe = new LocalFileEnumerator();
        XmlSerializer serializer = new XmlSerializer(typeof(LocalFileEnumerator));
        FileStream fs = new FileStream(filename, FileMode.Open);
        lfe = (LocalFileEnumerator)serializer.Deserialize(fs);
        fs.Close();
        foreach (LocalFileStruct lf in lfe.Items)
        {
          if (File.Exists(lf.LocalFile))
          {
            Items.Add(lf);
          }
        }
        
      }
      catch
      {
      }
    }


  }
}
