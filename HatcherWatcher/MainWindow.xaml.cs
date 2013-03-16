using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using System.Text;
using System.Diagnostics;

namespace HatcherWatcher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private TcpListener _listener;
    private Thread _listenerThread;

    public MainWindow()
    {
      InitializeComponent();
      StartListener();
    }

    private void StartListener()
    {
      _listenerThread = new Thread(RunListener);
      _listenerThread.Start();
    }

    private void RunListener()
    {
      _listener = new TcpListener(IPAddress.Any, 11000);
      _listener.Start();
      while (true)
      {
        TcpClient client = _listener.AcceptTcpClient();

        this.Dispatcher.BeginInvoke((Action)(() => { textBoxLog.Text += string.Format("\nNew connection from {0}", client.Client.RemoteEndPoint); }));

        ThreadPool.QueueUserWorkItem(ProcessClient, client);
      }
    }

    private void ProcessClient(object state)
    {
      TcpClient client = state as TcpClient;

      var data = new byte[client.ReceiveBufferSize];

      StringBuilder dataString = new StringBuilder();

      using (var ns = client.GetStream())
      {
          int readCount;
          while ((readCount = ns.Read(data, 0, client.ReceiveBufferSize)) != 0)
          {
              Debug.Print(Encoding.UTF8.GetString(data, 0, readCount));
          } 
      }

      client.Close();
    }

    private void buttonConnect_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
