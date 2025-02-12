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

namespace TcpFileClient
{
    public partial class Form1 : Form
    {
        string m_spliter = "'\\'";
        string m_fName = string.Empty;
        string[] m_split = null;
        private const int ChunkSize = 1024; // 작은 데이터 조각의 크기
        byte[] m_clientData = null;

        enum DataPacketType { TEXT = 1, IMAGE };


        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("===클라이언트===");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            char[] delimeter = m_spliter.ToCharArray();

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            openFileDialog.ShowDialog();

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            textBox1.Text = openFileDialog.FileName;
            pictureBox1.ImageLocation = openFileDialog.FileName;

            m_split = textBox1.Text.Split(delimeter);
            int limit = m_split.Length;

            m_fName = m_split[limit - 1].ToString();

            if (textBox1.Text != null)
                button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] fileName = Encoding.UTF8.GetBytes(m_fName);
                byte[] fileData = File.ReadAllBytes(textBox1.Text);
                byte[] fileNameLen = BitConverter.GetBytes(fileName.Length);
                byte[] fileType = BitConverter.GetBytes((int)DataPacketType.IMAGE);

                // 서버의 IP 주소와 포트 번호
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 9050);

                // 파일 전송 시작 메시지 전송
                clientSocket.SendTo(BitConverter.GetBytes((int)DataPacketType.IMAGE), serverEP);
                clientSocket.SendTo(fileNameLen, serverEP);
                clientSocket.SendTo(fileName, serverEP);

                // 데이터를 조각으로 나누어 전송
                int totalChunks = (int)Math.Ceiling((double)fileData.Length / ChunkSize);
                for (int i = 0; i < totalChunks; i++)
                {
                    int offset = i * ChunkSize;
                    int size = Math.Min(ChunkSize, fileData.Length - offset);
                    byte[] chunkData = new byte[size];
                    Array.Copy(fileData, offset, chunkData, 0, size);
                    clientSocket.SendTo(chunkData, serverEP);
                }

                // 파일 전송 완료 메시지 전송
                clientSocket.SendTo(BitConverter.GetBytes((int)DataPacketType.IMAGE), serverEP);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
               
                byte[] textData = Encoding.UTF8.GetBytes(textBox2.Text+"\r\n");
                byte[] fileType = BitConverter.GetBytes((int)DataPacketType.TEXT);

                m_clientData = new byte[fileType.Length + textData.Length];

                fileType.CopyTo(m_clientData, 0);
                textData.CopyTo(m_clientData, 4);

                // 서버의 IP 주소와 포트 번호
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 9050);

                // 텍스트 전송 시작 메시지 전송
                clientSocket.SendTo(BitConverter.GetBytes((int)DataPacketType.TEXT), serverEP);

                // 텍스트 데이터 전송
                clientSocket.SendTo(m_clientData, serverEP);

                // 텍스트 전송 완료 메시지 전송
                clientSocket.SendTo(BitConverter.GetBytes((int)DataPacketType.TEXT), serverEP);

                clientSocket.Close();
             }
        }
    }
}
