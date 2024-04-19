using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

//서버입니다.
namespace VideoPlayerClient
{
    public partial class Form1 : Form
    {
        private const int PORT = 1234;
        private Socket receiverSocket;
        private Thread receiveThread;

        public Form1()
        {
            InitializeComponent();
            receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            receiverSocket.Listen(1);

            receiveThread = new Thread(new ThreadStart(ReceiveFrames));
            receiveThread.Start();
        }

        private void ReceiveFrames()
        {
            try
            {
                Socket clientSocket = receiverSocket.Accept();
                Console.WriteLine("클라이언트가 연결되었습니다.");

                while (true)
                {
                    // 프레임 데이터의 길이를 수신
                    byte[] lengthBytes = new byte[4];
                    clientSocket.Receive(lengthBytes);
                    int frameLength = BitConverter.ToInt32(lengthBytes, 0);

                    // 프레임 데이터를 수신
                    byte[] imageData = new byte[frameLength];
                    int bytesRead = 0;
                    int totalBytesRead = 0;
                    while (totalBytesRead < frameLength && (bytesRead = clientSocket.Receive(imageData, totalBytesRead, frameLength - totalBytesRead, SocketFlags.None)) > 0)
                    {
                        totalBytesRead += bytesRead;
                    }

                    // 수신한 프레임 데이터를 Mat 형식으로 변환하여 출력
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        Mat frame = Mat.FromStream(ms, ImreadModes.Color);
                        BeginInvoke(new Action(() =>
                        {
                            if (frame.Cols > 0 && frame.Rows > 0)
                            {
                                pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                            }
                            else
                            {
                                MessageBox.Show("유효하지 않은 이미지입니다.");
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("프레임 수신 중 오류가 발생했습니다 : " + ex.Message);
                Console.WriteLine("에러 : " + ex.StackTrace);
            }
            finally
            {
                receiverSocket.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }
        }

    }
}
