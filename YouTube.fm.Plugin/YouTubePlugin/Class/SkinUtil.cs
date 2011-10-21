using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MediaPortal.GUI.Library;

namespace YouTubePlugin.Class
{
  public class SkinUtil
  {
    public static string GetDefine(string file, string defineName)
    {
      try
      {
        XmlDocument document = new XmlDocument();
        document.Load(file);

        foreach (XmlNode node in document.SelectNodes("/window/define"))
        {
          string[] tokens = node.InnerText.Split(':');

          if (tokens.Length < 2)
          {
            continue;
          }
          if (tokens[0] == defineName) return tokens[1];
        }
      }
      catch (Exception e)
      {
        Log.Error("GUIWindow.LoadDefines: {0}", e.Message);
      }
      return "";
    }
  }
}
