using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ThorClient.Engine;

namespace ThorClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MyTraceListener mtl = new MyTraceListener();
        private IpcConnection conn = null;
        private SolidColorBrush NormalBrush = new SolidColorBrush(Color.FromRgb(28, 103, 88));
        private SolidColorBrush DataBrush = new SolidColorBrush(Color.FromRgb(41, 52, 98));
        private SolidColorBrush ErrorBrush = new SolidColorBrush(Color.FromRgb(205, 16, 77));
        private SolidColorBrush UDPBrush = new SolidColorBrush(Color.FromRgb(64, 13, 81));

        private string APPName = "ThorClient";
        private string RemotePCHostName = ".";
        private string FullSaveName = "C:\\temp\\exp01";
        private int UDPPort = 9988;

        private bool interacting = false;
        private bool cantalk = false;
        private bool acquiring = false;

        private bool canMoveFiles = false;

        public MainWindow()
        {
            InitializeComponent();
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(mtl, "PropertyChanged", traceOnPropertyChanged);
            Trace.Listeners.Add(mtl);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"ThorClient v{Utils.GetVersion()}";
            Trace.WriteLine($"Started v{Utils.GetVersion()}.");

            UDPListener.commandEventEvent += (msg) =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (msg.Contains("StartAcquiring") && cantalk) {

                            canMoveFiles = false;
                            string[] parts = msg.Split(' ');

                            if (parts.Length > 1) {
                                // first part - command
                                // second part - path where to save.

                                string savePath = $"D:\\temp\\{parts[1]}";
                                txtFullSaveName.Text = savePath;
                                FullSaveName = savePath;

                                try {
                                    if (!Directory.Exists(savePath)) {
                                        Directory.CreateDirectory(savePath);
                                    }
                                }
                                catch { 
                                
                                }
                            }

                            // if alraedy acquiring. stop and then start again.
                            if(acquiring)
                                btnStartStopAcq_Click(null, null);

                            btnStartStopAcq_Click(null, null);
                        }

                        if (msg.Contains("StopAcquiring") && cantalk)
                        {
                            // only execute if acquiring.
                            if (acquiring)
                            {
                                canMoveFiles = true;
                                btnStartStopAcq_Click(null, null);


                                // move files.
                            }
                        }

                    }));
                }
                catch (Exception eu) {
                    Trace.WriteLine(eu.Message, Log.ERROR);
                }
            };
        }

        private void moveFiles(string newPath) {
            try
            {

                try
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }
                catch
                {

                }

                string src = @"C:\temp\exp01";
                if (Directory.Exists(src))
                {
                    foreach (var file in new DirectoryInfo(src).GetFiles())
                    {
                        try
                        {
                            file.MoveTo($@"{newPath}\{file.Name}");
                            Trace.WriteLine($"Moved > {file.Name} to {newPath}.", Log.DATA);
                        }
                        catch (Exception jj)
                        {
                            Trace.WriteLine($"Failed to move file {file.Name}. {jj.Message}", Log.ERROR);
                        }
                    }
                }
            }
            catch (Exception k) {
                Trace.WriteLine($"Failed to move files {k.Message}", Log.ERROR);
            }

        }

        private void traceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Trace")
                {
                    setShortMessage(mtl.Trace, mtl.Category);
                }
            }
            catch { 
            }
        }

        private void setShortMessage(string msg = "", string cat = "")
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (msg == null)
                        return;

                    lblShortMsg.Content = msg;
                    AppendText($"{DateTime.Now} {msg}\r", cat);
                }));
            }
            catch { 
            }
        }

        private void AppendText(string text, string category)
        {
            try
            {
                if (category == Log.STATUS)
                    return;

                if (category == Log.ERROR)
                    txtURL.AppendText(text, ErrorBrush);
                else if (category == Log.DATA)
                    txtURL.AppendText(text, DataBrush);
                else if (category == Log.UDP)
                    txtURL.AppendText(text, UDPBrush);
                else
                    txtURL.AppendText(text, NormalBrush);

                txtURL.ScrollToEnd();
            }
            catch
            {
            }
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (interacting)
                {
                    UDPListener.keepGoing = false;
                    stopSequence();
                }
                else
                {
                    Trace.WriteLine("Starting..");

                    if (!int.TryParse(txtUDPPort.Text.Trim(), out UDPPort))
                    {
                        Trace.WriteLine($"In-valid UDP listener port = {txtUDPPort.Text}");
                        return;
                    }

                    conn = new IpcConnection();
                    conn.ConnectEventEvent += Conn_ConnectEventEvent;
                    conn.AckEventEvent += Conn_AckEventEvent;

                    if (!string.IsNullOrEmpty(txtAppName.Text.Trim()))
                        APPName = txtAppName.Text.Trim();
                    else
                        APPName = "ThorClient";

                    Trace.WriteLine($"Application Name = {APPName}");

                    if (!string.IsNullOrEmpty(txtRemotePCHostName.Text.Trim()))
                        RemotePCHostName = txtRemotePCHostName.Text.Trim();
                    else
                        RemotePCHostName = ".";

                    Trace.WriteLine($"Remote PC Host Name = {RemotePCHostName}");

                    if (!string.IsNullOrEmpty(txtFullSaveName.Text.Trim()))
                        FullSaveName = txtFullSaveName.Text.Trim();
                    else
                        FullSaveName = "D:\\temp\\exp01";

                    Trace.WriteLine($"Full Save Name = {FullSaveName}");

                    if (!UDPListener.keepGoing)
                        UDPListener.Start(UDPPort);

                    interacting = true;

                    toggleFields();

                    btnStartStop.Content = "Stop";

                    conn.dontStop = true;

                    startInteraction();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, Log.ERROR);
            }
        }

        private void Conn_AckEventEvent(bool good)
        {
            if (canMoveFiles) {
                moveFiles(FullSaveName);
            }
        }

        private void stopSequence()
        {
            try
            {
                Trace.WriteLine($"Stop!!");

                if (conn != null)
                {
                    conn.SendToClient(Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.TearDown));
                    Trace.WriteLine("Bye, see you next time.");

                    conn.ConnectEventEvent -= Conn_ConnectEventEvent;
                    conn.AckEventEvent -= Conn_AckEventEvent;
                    conn.Disconnect();
                    conn = null;
                }

                btnStartStop.Content = "Start";
                btnStartStopAcq.Visibility = Visibility.Collapsed;
                interacting = false;
                cantalk = false;
                toggleFields();
            }
            catch(Exception e) {
                Trace.WriteLine(e.ToString());
            }

        }

        private void Conn_ConnectEventEvent(bool can)
        {
        }

        private void startInteraction()
        {

            conn.RemotePCHostName = RemotePCHostName;
            conn.FullSaveName = FullSaveName;

            conn._connectionServerID = $"{APPName}ThorImagePipe";
            Trace.WriteLine($"Server Pipe Name = {conn._connectionServerID}");

            conn._connectionClientID = $"ThorImage{APPName}Pipe";
            Trace.WriteLine($"Client Pipe Name = {conn._connectionClientID}");

            conn.StartNamedPipeClient();
            conn.SendToClient(Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.Establish), conn.GetHostName());

            cantalk = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                btnStartStopAcq.Visibility = cantalk ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void toggleFields()
        {
            txtAppName.IsEnabled = !interacting;
            txtRemotePCHostName.IsEnabled = !interacting;
            txtFullSaveName.IsEnabled = !interacting;
            btnStartStopAcq.Content = acquiring ? "Stop Acquiring" : "Start Acquiring";
        }

        private void btnStartStopAcq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (acquiring)
                {
                    Trace.WriteLine("Stop acquiring");
                    btnStartStopAcq.Content = "Start Acquiring";
                    conn.SendToClient(Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.StopAcquiring), conn.FullSaveName);
                    acquiring = false;
                    Trace.WriteLine("Stopped.");
                }
                else
                {
                    Trace.WriteLine("Start acquiring");
                    btnStartStopAcq.Content = "Stop Acquiring";
                    conn.SendToClient(Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.StartAcquiring), conn.FullSaveName);
                    acquiring = true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, Log.ERROR);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            stopSequence();
        }
    }
}
