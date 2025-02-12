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


namespace UdpImageEx
{
    public partial class Form1 : Form
    {

        private const int PORT = 9051;
        private string imagePath;
        private const int ChunkSize = 1024;
        private Socket clientSocket;

        public Form1()
        {
            InitializeComponent();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg; *.jpeg; *.png; *.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;
                pictureBox1.Image = Image.FromFile(imagePath);
            }
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(imagePath);
                    IPEndPoint receiverEP = new IPEndPoint(IPAddress.Parse("192.168.50.223"), PORT); 

                    int totalChunks = (int)Math.Ceiling((double)imageData.Length / ChunkSize);
                    for (int i = 0; i < totalChunks; i++)
                    {
                        int offset = i * ChunkSize;
                        int size = Math.Min(ChunkSize, imageData.Length - offset);
                        byte[] chunkData = new byte[size];
                        Array.Copy(imageData, offset, chunkData, 0, size);
                        clientSocket.SendTo(chunkData, receiverEP);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("이미지 전송 오류: " + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("먼저 이미지를 불러와주세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
