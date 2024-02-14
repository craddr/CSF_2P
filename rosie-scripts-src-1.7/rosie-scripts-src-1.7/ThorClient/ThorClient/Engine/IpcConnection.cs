using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    public class IpcConnection
    {
        public bool dontStop = true;
        bool _threadCompleted = true;
        Thread _serverThread = null;
        Thread _pipeClient = null;
        string[] _sendBuffer = null;
        bool _thorImageLSConnectionStats = false;
        NamedPipeServerStream _pipeServer;
        public string _connectionServerID;
        public string _connectionClientID;
        public readonly int DataLength = 4;
        public string RemotePCHostName;
        public string FullSaveName;
        bool _receiveIPCCommandActive = false;

        public delegate void ConnectEvent(bool cantalk);
        public event ConnectEvent ConnectEventEvent;

        public delegate void AckEvent(bool good);
        public event AckEvent AckEventEvent;

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <returns></returns>
        public string GetHostName()
        {
            return (System.Environment.MachineName);
        }

        /// <summary>
        /// Sends to client.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="data">The data.</param>
        public void SendToClient(String command, String data = "0")
        {
            if (_threadCompleted == true)
            {
                if (_serverThread != null)
                {
                    _serverThread = null;
                }
                _sendBuffer = new string[] { Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote), Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local), command, data };

                _serverThread = new Thread(ServerThread);
                _serverThread.Start();
            }
            else {
                Trace.WriteLine("Last thread has not completed !!", Log.ERROR);
            }
        }

        /// <summary>
        /// Servers the thread.
        /// </summary>
        private void ServerThread()
        {
            try
            {
                _threadCompleted = false;
                StreamOutNamedPipe();
                if (_thorImageLSConnectionStats == true)
                {
                    switch ((ThorPipeCommand)(Enum.Parse(typeof(ThorPipeCommand), _sendBuffer[2])))
                    {
                        case ThorPipeCommand.Establish:
                            {
                                _thorImageLSConnectionStats = true;

                                Thread.Sleep(50);
                                String[] configurarionInformation = { "true", "0" };
                                _sendBuffer = new string[] { Enum.GetName(typeof(ThorPipeSrc),ThorPipeSrc.Remote),  Enum.GetName(typeof(ThorPipeDst),ThorPipeDst.Local),
                                        Enum.GetName(typeof(ThorPipeCommand),ThorPipeCommand.UpdataInformation), string.Join("/",configurarionInformation) };
                                StreamOutNamedPipe();
                            }
                            break;
                        case ThorPipeCommand.TearDown:
                            {
                                _thorImageLSConnectionStats = false;

                            }
                            break;
                        default:
                            break;
                    }
                }
                _threadCompleted = true;
            }
            catch(Exception e) {
                Trace.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Send Message Out
        /// </summary>
        public void StreamOutNamedPipe()
        {
            string msgRecv = string.Empty;

            try
            {
                _pipeServer = new NamedPipeServerStream(_connectionServerID, PipeDirection.InOut, 4, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                _pipeServer.WaitForConnection();

            }
            catch (IOException)// release the pipe resource while it's connecting, throw the IOEception
            {
                _thorImageLSConnectionStats = false;

                if (_sendBuffer[2] != Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.TearDown))
                {
                    Trace.WriteLine("The ThorImage is Disconnected. --Connection Error", Log.ERROR);
                }
                _threadCompleted = true;
                return;
            }
            catch (Exception)
            {
                return;
            }
            // Send Message
            try
            {
                // Read the request from the client. Once the client has
                // written to the pipe its security token will be available.
                _thorImageLSConnectionStats = true;
                StreamString ss = new StreamString(_pipeServer);
                // Verify our identity to the connected client using a
                // string that the client anticipates.
                ss.WriteString(String.Join("~", _sendBuffer));
                msgRecv = ss.ReadString();
                ReceiveIPCMessageACK(msgRecv);
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (Exception j)
            {
                Trace.WriteLine(j.ToString());
            }
            _pipeServer.Close();
        }

        /// <summary>
        /// Receives the ipc ack message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void ReceiveIPCMessageACK(string msg)
        {

            if (msg.Contains("~"))
            {
                String[] msgRecv = msg.Split('~');
                if (msgRecv.Length == 4)
                {
                    if (VerifyNamedPipeRouting(msgRecv))
                    {
                        if (msgRecv[2] == _sendBuffer[2] && msgRecv[3] == "1")
                        {
                            AckEventEvent?.Invoke(true);
                        }
                        else
                        {
                            AckEventEvent?.Invoke(false);

                            switch ((ThorPipeStatus)(Convert.ToInt32(msgRecv[3])))
                            {
                                case ThorPipeStatus.ThorPipeStsNoError:
                                    Trace.WriteLine("ThorPipeStsNoError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeStsBusy:
                                    Trace.WriteLine("ThorPipeStsBusy", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeStsBlankCommandError:
                                    Trace.WriteLine("ThorPipeStsBlankCommandError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeStreamNotSupportedError:
                                    Trace.WriteLine("ThorPipeStreamNotSupportedError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeFormatError:
                                    Trace.WriteLine("ThorPipeFormatError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeFormatRoutingError:
                                    Trace.WriteLine("ThorPipeFormatRoutingError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorpipeIOError:
                                    Trace.WriteLine("ThorpipeIOError", Log.ERROR);
                                    break;
                                case ThorPipeStatus.ThorPipeError:
                                    Trace.WriteLine("ThorPipeError", Log.ERROR);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Verifies the named pipe routing.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public bool VerifyNamedPipeRouting(String[] msg)
        {
            if (msg.Length == DataLength && msg[0] == Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote)
                && msg[1] == Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts the namedpipe client.
        /// </summary>
        public void StartNamedPipeClient()
        {
            if (_pipeClient == null)
            {
                _pipeClient = new Thread(ClientThread);
            }
            _pipeClient.Start();
        }

        /// <summary>
        /// Stops the namedpipe client.
        /// </summary>
        public void StopNamedPipeClient()
        {
            if (_pipeClient != null)
            {
                _pipeClient.Abort();
                _pipeClient = null;
            }
        }

        /// <summary>
        /// Clients the thread.
        /// </summary>
        /// <param name="data">The data.</param>
        private void ClientThread(object data)
        {
            while (dontStop)//endless loop
            {
                try
                {
                    // New Server NamedPipeClientStream Instance
                    NamedPipeClientStream _namedPipeClient;
                    Trace.WriteLine($"Create pipe {_connectionClientID}", Log.STATUS);
                    if (RemotePCHostName == GetHostName())
                    {
                        _namedPipeClient = new NamedPipeClientStream(".", _connectionClientID, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    }
                    else
                    {
                        _namedPipeClient = new NamedPipeClientStream(RemotePCHostName, _connectionClientID, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    }

                    if (NamedPipeDoesNotExist(_connectionClientID))
                    {
                        //sleep to lessen CPU load
                        System.Threading.Thread.Sleep(20);
                        continue;
                    }
                    else {
                        Trace.WriteLine($"Pipe {_connectionClientID} exists.");
                    }

                    // Wait for a Server to connect
                    try
                    {
                        Trace.WriteLine("Connect");
                        _namedPipeClient.Connect();

                        Trace.WriteLine("Read from server........");

                        // Read the request from the Server. Once the Server has
                        // written to the pipe its security token will be available
                        StreamString ss = new StreamString(_namedPipeClient);
                        string msg = ss.ReadString();

                        Trace.WriteLine($"Server says ........{msg}");

                        ReceiveIPCCommand(msg, ss);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message, Log.ERROR);
                    }
                    finally
                    {
                        Trace.WriteLine("Close client.");
                        _namedPipeClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, Log.ERROR);
                    break;
                }
            }
        }

        public static bool NamedPipeDoesNotExist(string pipeName)
        {
            try
            {
                int timeout = 0;
                string normalizedPath = System.IO.Path.GetFullPath(
                 string.Format(@"\\.\pipe\{0}", pipeName));
                bool exists = WaitNamedPipe(normalizedPath, timeout);
                if (!exists)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 0) // pipe does not exist
                        return true;
                    else if (error == 2) // win32 error code for file not found
                        return true;
                    // all other errors indicate other issues
                }
                return false;
            }
            catch (Exception ex)
            {
                return true; // assume it exists
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WaitNamedPipe(string name, int timeout);

        /// <summary>
        /// Receives the ipc command.
        /// </summary>
        /// <param name="thorImagePipeRecv">The thor image pipe recv.</param>
        /// <param name="ss">The ss.</param>
        public void ReceiveIPCCommand(String thorImagePipeRecv, StreamString ss)
        {
            if (thorImagePipeRecv.Contains("~"))
            {
                String[] msgRecv = thorImagePipeRecv.Split('~');
                if (VerifyNamedPipeRouting(msgRecv))
                {
                    if (ExcuteNamedPipeData(msgRecv, ss, true))
                    {
                        ConnectEventEvent?.Invoke(true);

                        ss.WriteString(String.Join("~", new String[]{Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote), Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local),
                                           msgRecv[2], "1"}));
                    }
                    else
                    {
                        ConnectEventEvent?.Invoke(false);
                        ss.WriteString(String.Join("~", new String[]{Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote), Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local),
                                   Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.Error), "2"}));
                    }
                }
                else
                {
                    ConnectEventEvent?.Invoke(false);
                    ss.WriteString(String.Join("~", new String[]{Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote), Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local),
                                       Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.Error), "11"}));
                }
            }
            else
            {
                ConnectEventEvent?.Invoke(false);
                ss.WriteString(String.Join("~", new String[]{Enum.GetName(typeof(ThorPipeSrc), ThorPipeSrc.Remote), Enum.GetName(typeof(ThorPipeDst), ThorPipeDst.Local),
                                   Enum.GetName(typeof(ThorPipeCommand), ThorPipeCommand.Error), "3"}));
            }
        }

        /// <summary>
        /// Excutes the namedpipe data.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="ss">The ss.</param>
        /// <returns></returns>
        public bool ExcuteNamedPipeData(String[] msg, StreamString ss, bool isAcknowledgment)
        {
            _receiveIPCCommandActive = true;
            if (msg.Length == 4)
            {
                switch ((ThorPipeCommand)(Enum.Parse(typeof(ThorPipeCommand), msg[2])))
                {
                    case ThorPipeCommand.Establish:
                        {
                            
                        };
                        break;
                    case ThorPipeCommand.TearDown:
                        {

                        };
                        break;
                    case ThorPipeCommand.StartAcquiring:
                        {

                        };
                        break;
                    case ThorPipeCommand.StopAcquiring:
                        {

                        };
                        break;
                    case ThorPipeCommand.AcquireInformation:
                        {

                        };
                        break;
                    default:
                        _receiveIPCCommandActive = false;
                        return false;
                }
                _receiveIPCCommandActive = false;
                return true;
            }
            else
            {
                _receiveIPCCommandActive = false;
                return false;
            }
        }

        public void Disconnect()
        {

            dontStop = false;
            

            if (_pipeClient != null)

            {
                try
                {
                    _pipeClient.Abort();
                    _pipeClient = null;
                }
                catch
                {
                }
            }

            if (_pipeServer != null)
            {
                try
                {
                    //_pipeServer.Disconnect();
                    _pipeServer.Close();
                    _pipeServer = null;
                }
                catch
                {
                }
            }

            if (_serverThread != null)
            {
                try
                {
                    _serverThread.Abort();
                    _serverThread = null;
                }
                catch
                {
                }
            }

        }
    }
}
