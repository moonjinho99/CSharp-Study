using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VideoServer
{
    class Program
    {
        static List<IPEndPoint> connectedClients = new List<IPEndPoint>();

        static void Main(string[] args)
        {
            UdpClient server = new UdpClient(9050);
            IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            

            Console.Write("====영상 서버====");

            while(true)
            {
                try
                {
                    byte[] imageData = server.Receive(ref clientEP);

                    if(!connectedClients.Contains(clientEP))
                    {
                        connectedClients.Add(clientEP);
                    }    

                    BroadcastImageData(imageData, server, clientEP);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                }
              
            }

        }
        static void BroadcastImageData(byte[] imageData, UdpClient server, IPEndPoint senderEP)
        {
            foreach(var client in connectedClients)
            {
                if(!client.Equals(senderEP))
                {
                    server.Send(imageData, imageData.Length, client);
                }
            }
        }
    }
}
