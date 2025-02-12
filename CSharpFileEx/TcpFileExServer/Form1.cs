using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace TcpFileExServer
{
    public partial class Form1 : Form
    {

        bool initialFlag = true;
        string receivedPath = string.Empty;
        enum DataPacketType { TEXT = 1, IMAGE };
        int dataType = 0;
        string textData = string.Empty;

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("===서버===");
            Thread t_handler = new Thread(StartListening);
            t_handler.IsBackground = true;
            t_handler.Start();
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private void StartListening()
        {
            // 서버에서 TCP 대신 UDP를 사용합니다.
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 9050);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            StateObject state = new StateObject();
            state.workSocket = listener;

            try
            {
                listener.Bind(localEP);

                while (true)
                {
                    allDone.Reset();
                    // 원격 호스트의 IP 주소와 포트 번호를 저장할 EndPoint 객체를 생성합니다.
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    // UDP에서는 BeginReceiveFrom 메서드를 사용하여 데이터를 수신합니다.
                    listener.BeginReceiveFrom(state.buffer, 0, StateObject.BufferSize, 0, ref remoteEP, new AsyncCallback(ReadCallback), state);
                    allDone.WaitOne();
                }
            }
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("SocketException : {0}", se.Message));
                Console.WriteLine(string.Format("SocketException : {0}", se.Message));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception : {0}", ex.Message));
                Console.WriteLine(string.Format("Exception : {0}", ex.Message));
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            int fileNameLen = 0;
            StateObject state = ar.AsyncState as StateObject;
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 9050);

            try
            {
                int bytesRead = state.workSocket.EndReceiveFrom(ar, ref remoteEP);


                Console.WriteLine("바이트수 : " + bytesRead);
                if (bytesRead > 0)
                {
                    if (initialFlag)
                    {
                        byte[] buffer = state.buffer;

                        dataType = BitConverter.ToInt32(buffer, 0);

                        if (dataType == (int)DataPacketType.IMAGE)
                        {
                            fileNameLen = BitConverter.ToInt32(buffer, 4);
                            string fileName = Encoding.UTF8.GetString(buffer, 8, fileNameLen);

                            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            string pathDownload = Path.Combine(pathUser, "Downloads");
                            
                            receivedPath = Path.Combine(pathDownload, fileName);

                            if (File.Exists(receivedPath))
                                File.Delete(receivedPath);
                        }
                        else if (dataType == (int)DataPacketType.TEXT)
                        {
                            textData = Encoding.UTF8.GetString(buffer, 4, bytesRead - 4);
                            int markerIndex = textData.IndexOf("\r\n");
                            if (markerIndex != -1)
                            {
                                textData = textData.Substring(0, markerIndex); // 마커 이후의 데이터는 제거
                                textBox1.Text = textData;
                            }
                            Console.WriteLine("텍스트 데이터 : " + textData);

                            state.workSocket.BeginReceiveFrom(state.buffer, 0, StateObject.BufferSize, 0, ref remoteEP, new AsyncCallback(ReadCallback), state);
             
                        }
                    }

                    if (dataType == (int)DataPacketType.IMAGE)
                    {
                        BinaryWriter bw = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                        if (initialFlag)
                            bw.Write(state.buffer, 8 + fileNameLen, bytesRead - (8 + fileNameLen));
                        else
                            bw.Write(state.buffer, 0, bytesRead);

                        initialFlag = false;
                        bw.Close();
                        state.workSocket.BeginReceiveFrom(state.buffer, 0, StateObject.BufferSize, 0, ref remoteEP, new AsyncCallback(ReadCallback), state);
                    }
                }
                else
                {
                    Console.WriteLine("출력메소드");
                    if (dataType == (int)DataPacketType.IMAGE)
                    {
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox1.ImageLocation = receivedPath;
                        Invoke((MethodInvoker)delegate
                        {
                            label1.Text = "Data has been received";
                        });
                    }
                    else if (dataType == (int)DataPacketType.TEXT)
                        Invoke((MethodInvoker)delegate
                        {
                            textBox1.Text = textData;
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ReadCallback 오류: " + e.ToString());
            }
            finally
            {
                allDone.Set();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
