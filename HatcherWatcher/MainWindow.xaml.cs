using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace HatcherWatcher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private TcpListener _tcpListener = null;
    private TcpClient _tcpClient = null;
    private Thread _listenerThread;
    private MyViewModel _myViewModel = new MyViewModel();

    public MainWindow()
    {
      InitializeComponent();

      listBoxLog.DataContext = _myViewModel;
      Connect();
    }

    private void buttonConnect_Click(object sender, RoutedEventArgs e)
    {
        Connect();
    }

    private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
    {
        Disconnect();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Disconnect();
    }

    private void Connect()
    {
        try
        {
            if (_listenerThread == null)
            {
                _listenerThread = new Thread(RunListener);
                _listenerThread.Start();

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult result = socket.BeginConnect(IPAddress.Parse("192.168.250.200"), 11000, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(5000, true);

                    if (!success)
                    {
                        socket.Close();
                        throw new ApplicationException("Failed to connect to controller.");
                    }

                    string presentTime = string.Format("TIME {0} {1} {2} {3} {4} {5} {6}",
                                         DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                         DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

                    byte[] data = Encoding.ASCII.GetBytes(presentTime);
                    socket.Send(data);

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
        }
        catch (SocketException sex)
        {
            Debug.Print(sex.ToString());
        }
        catch (ApplicationException aex)
        {
            Debug.Print(aex.ToString());
        }
        catch (Exception ex)
        {
            Debug.Print(ex.ToString());
        }
    }

    private void Disconnect()
    {
        try
        {
            _tcpClient.GetStream().Close();

            while (_listenerThread.IsAlive)
            {
                Thread.Sleep(100);
            }

            _listenerThread = null;
        }
        catch (Exception ex)
        {
            Debug.Print(ex.ToString());
        }
    }

    private void RunListener()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, 11000);
            _tcpListener.Start();

            _tcpClient = _tcpListener.AcceptTcpClient();

            var data = new byte[_tcpClient.ReceiveBufferSize];
            StringBuilder dataString = new StringBuilder();

            using (NetworkStream networkStream = _tcpClient.GetStream())
            {
                int readCount;

                while ((readCount = networkStream.Read(data, 0, _tcpClient.ReceiveBufferSize)) != 0)
                {
                    String dataReceived = Encoding.UTF8.GetString(data, 0, readCount);
                    this.Dispatcher.BeginInvoke((Action)(() => { OnNewData(dataReceived); }));
                }
            }
        }
        catch (SocketException sex)
        {
            Debug.Print(sex.ToString());
        }
        catch (Exception ex)
        {
            Debug.Print(ex.ToString());
        }
        finally
        {
            _tcpClient.Close();
            _tcpListener.Stop();
        }
    }

    public void OnNewData(String data)
    {
        _myViewModel.Items.Add(data);
        listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
    }
  }
}
