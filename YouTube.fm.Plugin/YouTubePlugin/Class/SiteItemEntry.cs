using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.Class
{
  public class SiteItemEntry
  {
    Dictionary<string, string> setting = new Dictionary<string, string>();

    public SiteItemEntry()
    {
      ConfigString = "";
      Provider = "";
      Title = "";
    }

    public string Url { get; set; }
    public string Provider { get; set; }
    public string Title { get; set; }
    public string ConfigString { get; set; }

    public string GetValue(string en)
    {
      if (setting.ContainsKey(en))
        return setting[en];
      return "";
    }

    public void SetValue(string en, string val)
    {
      if (string.IsNullOrEmpty(val))
        return;
      if (setting.ContainsKey(en))
        setting[en] = val;
      else
        setting.Add(en, val);
    }

    public void PharseSettings(string param)
    {
      setting.Clear();
      string[] separat = { "|" };
      foreach (string s in param.Split(separat, StringSplitOptions.RemoveEmptyEntries))
      {
        SetValue(s.Split('=')[0], s.Split('=')[1]);
      }
    }

    public string GetConfigString()
    {
      string s = "";
      foreach (KeyValuePair<string, string> keyValuePair in setting)
      {
        s += keyValuePair.Key + "=" + keyValuePair.Value + "|";
      }
      return s;
    }
  }
}
