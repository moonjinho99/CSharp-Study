using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VideoSever
{

    class Program
    {
        static List<IPEndPoint> connectedClients = new List<IPEndPoint>();

        static void Main(string[] args)
        {
            UdpClient server = new UdpClient(9050);
            IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("====영상 서버====");

            try
            {
                while (true)
                {
                    byte[] imageData = server.Receive(ref clientEP);


                    Console.WriteLine("Address2 : " + clientEP.Address.ToString() + ", Port2 : " + clientEP.Port.ToString() + "가 접속했습니다.");



                    if (!connectedClients.Contains(clientEP))
                    {
                        Console.WriteLine("Address : " + clientEP.Address.ToString() + ", Port : " + clientEP.Port.ToString() + "가 접속했습니다.");

                        connectedClients.Add(clientEP);
                    }

                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        BroadcastImageData(imageData, server);
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
            }
            finally
            {
                server.Close();
            }
        }

        static void BroadcastImageData(byte[] imageData, UdpClient server)
        {
            try
            {
                foreach (var client in connectedClients)
                {
                    server.Send(imageData, imageData.Length, client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
            }
        }
    }
}
