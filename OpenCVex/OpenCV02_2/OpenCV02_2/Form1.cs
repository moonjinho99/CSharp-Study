using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Text;

namespace OpenCV02_2
{
    public partial class Form1 : Form
    {
        private const int ChunkSize = 1024; // 데이터 조각의 크기

        private VideoCapture _capture;

        private Thread receiverThread = null;

        private Thread senderThread = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            _capture = new VideoCapture(0); // 내장 카메라 사용

            receiverThread = new Thread(ReceiveVideo);
            receiverThread.IsBackground = true;
            receiverThread.Start();


            senderThread = new Thread(SendVideo);
            senderThread.IsBackground = true;
            senderThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            receiverThread.Abort();
            senderThread.Abort();
        }

        private void SendVideo()
        {
            using (Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                senderSocket.Connect(IPAddress.Parse("192.168.56.1"), 9051);

                Mat frame = new Mat();
                byte[] buffer;

                while (true)
                {
                    _capture.Read(frame);

                    if (!frame.Empty())
                    {

                        pictureBox1.Image = BitmapConverter.ToBitmap(frame);

                        // 이미지를 JPEG 형식으로 인코딩하여 바이트 배열로 변환
                        buffer = frame.ToBytes(".jpg");

                        // 전체 데이터 크기 전송
                        senderSocket.Send(BitConverter.GetBytes(buffer.Length));

                        // 데이터를 조각으로 나누어 전송
                        for (int i = 0; i < buffer.Length; i += ChunkSize)
                        {
                            int size = Math.Min(ChunkSize, buffer.Length - i);
                            senderSocket.Send(buffer, i, size, SocketFlags.None);
                        }
                    }

                    Thread.Sleep(50); // 잠시 대기 후 다음 프레임 처리
                }
            }
        }



        private void ReceiveVideo()
        {
            using (Socket receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, 9050);
                receiverSocket.Bind(senderEP);

                byte[] buffer;
                int totalDataSize;
                int receivedDataSize;
                MemoryStream memoryStream = new MemoryStream();

                while (true)
                {
                    // 전체 데이터 크기 수신
                    buffer = new byte[4];
                    receiverSocket.Receive(buffer);
                    totalDataSize = BitConverter.ToInt32(buffer, 0);

                    // 데이터를 조각으로 받아 메모리 스트림에 저장
                    while (memoryStream.Length < totalDataSize)
                    {
                        buffer = new byte[ChunkSize];
                        receivedDataSize = receiverSocket.Receive(buffer);
                        memoryStream.Write(buffer, 0, receivedDataSize);
                    }

                    // 메모리 스트림에서 이미지로 변환하여 출력
                    if (memoryStream.Length == totalDataSize)
                    {
                        byte[] imageData = memoryStream.ToArray();
                        Mat frame = Cv2.ImDecode(imageData, ImreadModes.Color);

                        if (!frame.Empty())
                        {
                            pictureBox2.Image = BitmapConverter.ToBitmap(frame);
                        }

                        memoryStream.Dispose();
                        memoryStream = new MemoryStream();
                    }
                }
            }
        }
    }
}
