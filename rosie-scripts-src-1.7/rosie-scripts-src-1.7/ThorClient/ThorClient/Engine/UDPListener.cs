using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    internal class UDPListener
    {
        public delegate void CommandEvent(string cmd);
        public static event CommandEvent commandEventEvent;

        public static bool keepGoing = false;
        public static void Start(int UDPPort = 9988)
        {
            keepGoing = true;
            Task.Run(() =>
            {
                while (keepGoing)
                {
                    try
                    {
                        int recv;
                        byte[] data = new byte[1024];
                        Trace.WriteLine($"Starting UDP listener at {UDPPort}", Log.UDP);
                        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, UDPPort);

                        Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                        newsock.Bind(ipep);
                        Trace.WriteLine("Waiting for a client...", Log.UDP);

                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        EndPoint Remote = (EndPoint)(sender);

                        while (SocketConnected(newsock) && keepGoing)
                        {
                            data = new byte[1024];
                            recv = newsock.ReceiveFrom(data, ref Remote);

                            string imsg = Encoding.ASCII.GetString(data, 0, recv);
                            commandEventEvent?.Invoke(imsg.Trim(new char[] { '\r', '\n' }));

                            Trace.WriteLine(imsg.Trim(new char[] { '\r', '\n' }), Log.UDP);

                            newsock.SendTo(data, recv, SocketFlags.None, Remote);
                        }
                        Trace.WriteLine("Client disconnected.", Log.UDP);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            });
        }

        private static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }
}
