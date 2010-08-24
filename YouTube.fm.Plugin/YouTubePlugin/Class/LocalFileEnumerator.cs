using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace YouTubePlugin
{
  public class LocalFileEnumerator
  {
    public List<LocalFileStruct> Items { get; set; }


    public LocalFileEnumerator()
    {
      Items = new List<LocalFileStruct>();
    }

    public void Save()
    {
      string filename = Config.GetFile(Config.Dir.Config, "youtube.xml");
      Stream myStream;
      if ((myStream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None)) != null)
      {
                  // Code to write the stream goes here.
        XmlDocument doc = new XmlDocument();
        XmlWriter writer = null;
        try
        {
            // Create an XmlWriterSettings object with the correct options. 
            XmlWriterSettings settings = new XmlWriterSettings();
            string st = string.Empty;
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;
            writer = XmlWriter.Create(myStream, settings);
            writer.WriteStartElement("LocalFileEnumerator");
            foreach (LocalFileStruct lfs in Items)
            {
                writer.WriteStartElement("Items");
                writer.WriteElementString("LocalFile", lfs.LocalFile);
                writer.WriteElementString("Title", lfs.Title);
                writer.WriteElementString("VideoId", lfs.VideoId);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
            if (writer != null)
                writer.Close();
            myStream.Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
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
        Items.Clear();
        string filename = Config.GetFile(Config.Dir.Config, "youtube.xml");
          XmlDocument doc = new XmlDocument();
          if (File.Exists(filename))
          {
              try
              {
                  doc.Load(filename);
                  XmlNode ver = doc.DocumentElement.SelectSingleNode("/LocalFileEnumerator");
                  XmlNodeList fileList = ver.SelectNodes("Items");
                  foreach (XmlNode nodefile in fileList)
                  {
                      LocalFileStruct lfs = new LocalFileStruct();
                      lfs.LocalFile = nodefile.SelectSingleNode("LocalFile").InnerText;
                      lfs.Title = nodefile.SelectSingleNode("Title").InnerText;
                      lfs.VideoId = nodefile.SelectSingleNode("VideoId").InnerText;
                      Items.Add(lfs);
                  }
              }
              catch(Exception ex)
              {
                  Log.Error(ex);
              }
          }
    }


  }
}
