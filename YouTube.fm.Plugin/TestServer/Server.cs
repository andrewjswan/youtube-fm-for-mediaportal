using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TestServer
{
  public partial class Server : Form
  {
    TcpListener tcpListener = new TcpListener(Dns.GetHostAddresses("localhost")[0],10);
    public Server()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      tcpListener.Start();
      BackgroundWorker backgroundWorker = new BackgroundWorker();
      backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
      backgroundWorker.RunWorkerAsync();
    }

    private void Write(string s)
    {
      if (listBox1.InvokeRequired)
      {
        listBox1.Invoke(new MethodInvoker(delegate { listBox1.Items.Add(s); }));
      }
      else
      {
        listBox1.Items.Add(s);
      }
      
    }

    void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      while (true)
      {
        Socket socketForClient = tcpListener.AcceptSocket();
        if (socketForClient.Connected)
        {
          Write("Client connected");
          NetworkStream networkStream = new NetworkStream(socketForClient);
          System.IO.StreamWriter streamWriter =
            new System.IO.StreamWriter(networkStream);
          System.IO.StreamReader streamReader =
            new System.IO.StreamReader(networkStream);
          string theString = "Sending";
          streamWriter.WriteLine(theString);
          Write(theString);
          streamWriter.Flush();
          theString = streamReader.ReadLine();
          Write(theString);
          streamReader.Close();
          networkStream.Close();
          streamWriter.Close();
        }
        else
        {
          Write("Exiting...");        
          break;
        }
        socketForClient.Close();
      }
    }
  }
}

