using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ImageReceive
{
    public partial class Form1 : Form
    {

        private const int PORT = 9051;
        //private UdpClient client;
        private MemoryStream receivedImageStream;
        private Socket receiverSocket;


        public Form1()
        {
            InitializeComponent();
            receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReceiveImage();
        }

        private void ReceiveImage()
        {
            try
            {
                IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remoteEP = senderEP;
                receivedImageStream = new MemoryStream();

                while (true)
                {
                    byte[] chunkData = new byte[1024];
                    int bytesRead = receiverSocket.ReceiveFrom(chunkData, ref remoteEP);
                    receivedImageStream.Write(chunkData, 0, bytesRead);

                    if (bytesRead < 1024)
                        break;
                }

                receivedImageStream.Position = 0;

                pictureBox1.Image = Image.FromStream(receivedImageStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 수신 오류 : " + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
 
                if (receivedImageStream != null)
                    receivedImageStream.Close();
            }
        }
    }
}
