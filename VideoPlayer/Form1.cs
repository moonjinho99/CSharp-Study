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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;



namespace VideoPlayer
{
    public partial class Form1 : Form
    {

        private const int PORT = 1234;
        private const string RECEIVER_IP = "192.168.56.1";
        private Socket senderSocket;
        string videoFilePath = "";

        public Form1()
        {
            InitializeComponent();
            senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //전송하기
        private void SendFrames()
        {
            using (VideoCapture capture = new VideoCapture(videoFilePath))
            {
                using (Mat frame = new Mat())
                {
                    while (true)
                    {
                        capture.Read(frame);
                        if (frame.Empty())
                            break;

                        pictureBox1.Image = BitmapConverter.ToBitmap(frame);

                        byte[] imageData = frame.ToBytes();

                        byte[] lengthBytes = BitConverter.GetBytes(imageData.Length);
                        senderSocket.Send(lengthBytes);

                        senderSocket.Send(imageData);

                        double fps = capture.Get(VideoCaptureProperties.Fps);

                        Thread.Sleep((int)fps);
                    }
                }
            }
        }

        //불러오기
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "동영상 파일 (*.mp4)|*.mp4|모든 파일 (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                videoFilePath = openFileDialog.FileName;
                senderSocket.Connect(new IPEndPoint(IPAddress.Parse(RECEIVER_IP), PORT));

                // 전송을 담당할 스레드 시작
                Thread sendingThread = new Thread(SendFrames);
                sendingThread.Start();
            }
        }
    }
}
