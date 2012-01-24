using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TestClient
{
  public class Settings
  {
    public static string ConfigFile
    {
      get { return Path.Combine(Path.GetTempPath(), "YoutubeFmClient.xml"); }
    }

    public string Host { get; set; }
    public int Port { get; set; }
    public bool TopMost { get; set; }


    public Settings()
    {
      Host = "localhost";
      Port = 18944;
      TopMost = true;
    }

    public void Save()
    {
      XmlSerializer serializer = new XmlSerializer(typeof(Settings));
      // Create a FileStream to write with.

      Stream writer = new FileStream(ConfigFile, FileMode.Create);
      // Serialize the object, and close the TextWriter
      serializer.Serialize(writer, this);
      writer.Close();
    }

    public static Settings Load()
    {
      Settings settings = new Settings();
      if (!Directory.Exists(Path.GetDirectoryName(ConfigFile)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile));
      }
      if (File.Exists(ConfigFile))
      {
        XmlSerializer mySerializer =
          new XmlSerializer(typeof(Settings));
        FileStream myFileStream = new FileStream(ConfigFile, FileMode.Open);
        settings = (Settings)mySerializer.Deserialize(myFileStream);
        myFileStream.Close();
      }
      else
      {
        settings.Save();
      }
      return settings;
    }


  }
}
