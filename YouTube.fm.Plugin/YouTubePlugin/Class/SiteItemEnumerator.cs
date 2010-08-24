using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace YouTubePlugin.Class
{
  public class SiteItemEnumerator
  {

    public SiteItemEnumerator()
    {
      Items = new List<SiteItemEntry>();
    }
    public List<SiteItemEntry> Items { get; set; }


    public void Save(string file)
    {
      string filename = Config.GetFile(Config.Dir.Config, file);
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
          writer.WriteStartElement("SiteItemEnumerator");
          foreach (SiteItemEntry itemEntry in Items)
          {
            writer.WriteStartElement("Items");
            writer.WriteElementString("ConfigString", itemEntry.ConfigString);
            writer.WriteElementString("Title", itemEntry.Title);
            writer.WriteElementString("Provider", itemEntry.Provider);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.Flush();
          writer.Close();
          myStream.Close();
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
    }

    public void Load(string file)
    {
      Items.Clear();
      string filename = Config.GetFile(Config.Dir.Config, file);
      XmlDocument doc = new XmlDocument();
      if (File.Exists(filename))
      {
        try
        {
          doc.Load(filename);
          XmlNode ver = doc.DocumentElement.SelectSingleNode("/SiteItemEnumerator");
          XmlNodeList fileList = ver.SelectNodes("Items");
          foreach (XmlNode nodefile in fileList)
          {
            SiteItemEntry itemEntry=new SiteItemEntry();
            itemEntry.Title = nodefile.SelectSingleNode("Title").InnerText;
            itemEntry.Provider = nodefile.SelectSingleNode("Provider").InnerText;
            itemEntry.ConfigString = nodefile.SelectSingleNode("ConfigString").InnerText;
            Items.Add(itemEntry);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
    }
  }
}
