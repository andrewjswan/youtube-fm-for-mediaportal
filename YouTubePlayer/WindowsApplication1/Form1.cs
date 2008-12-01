using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication1
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      Uri url = new Uri("http://youtube.com/get_video?video_id=Hr0Wv5DJhuk&l=220&t=OEgsToPDskJ-KpUvcpQffX6emUDSsM24&ext=.flv");
      string[] param = url.Query.Substring(1).Split('&');
      foreach (string s in param)
      {
        if (s.Split('=')[0] == "video_id")
        {
          MessageBox.Show(s.Split('=')[1]);
        }
      }
    }
  }
}