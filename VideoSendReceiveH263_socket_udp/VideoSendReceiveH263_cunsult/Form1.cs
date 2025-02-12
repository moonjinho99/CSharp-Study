using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using FFmpeg.AutoGen;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VideoSendReceiveH263_cunsult
{
    public partial class Form1 : Form
    {
        private const int ChunkSize = 1024;

        private VideoCapture _capture;

        private Thread senderThread = null;
        private Thread receiveThread = null;
        private byte[] encodedData;

        public Form1()
        {
            FFmpegBinariesHelperCunsult.RegisterFFmpegBinaries();
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {

            _capture = new VideoCapture(0);

            senderThread = new Thread(SendVideo);
            senderThread.IsBackground = true;
            senderThread.Start();
        }

        private void linkBtn_Click(object sender, EventArgs e)
        {
            receiveThread = new Thread(ReceiveVideo);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void SendVideo()
        {
            using (Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                senderSocket.Connect(IPAddress.Parse("192.168.56.1"), 9051);

                Mat frame = new Mat();

                while (true)
                {
                    _capture.Read(frame);

                    if (!frame.Empty())
                    {
                        pictureBox1.Image = Image.FromStream(frame.ToMemoryStream());

                        Bitmap image = frame.ToBitmap();
                        image = new Bitmap(image, new System.Drawing.Size(704, 576));

                        encodedData = EncodeToH263(image);

                        senderSocket.Send(BitConverter.GetBytes(encodedData.Length));

                        for (int i = 0; i < encodedData.Length; i += ChunkSize)
                        {
                            int size = Math.Min(ChunkSize, encodedData.Length - i);
                            senderSocket.Send(encodedData, i, size, SocketFlags.None);
                        }
                    }

                    Thread.Sleep(25);
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
                    buffer = new byte[1024];
                    receiverSocket.Receive(buffer);
                    totalDataSize = BitConverter.ToInt32(buffer, 0);

                    while (memoryStream.Length < totalDataSize)
                    {
                        buffer = new byte[ChunkSize];
                        receivedDataSize = receiverSocket.Receive(buffer);
                        memoryStream.Write(buffer, 0, receivedDataSize);
                    }

                    if (memoryStream.Length == totalDataSize)
                    {
                        byte[] imageData = memoryStream.ToArray();

                        DecodeToH263(imageData);

                        memoryStream.Dispose();
                        memoryStream = new MemoryStream();
                    }
                }

                Thread.Sleep(25);
            }
        }

        private unsafe byte[] EncodeToH263(Bitmap encodeBitmap)
        {
            var fps = 25;
            var sourceSize = new System.Drawing.Size(encodeBitmap.Width, encodeBitmap.Height);
            var sourcePixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
            using (var vfc = new VideoFrameConverterCunsult(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat))
            {
                using (var vse = new H263VideoStreamEncoderCunsult(fps, destinationSize))
                {
                    byte[] bitmapData;
                    bitmapData = GetBitmapData(encodeBitmap);
                    
                    fixed (byte* pBitmapData = bitmapData)
                    {
                        var data = new byte_ptrArray8 { [0] = pBitmapData };
                        var linesize = new int_array8 { [0] = bitmapData.Length / sourceSize.Height };
                        var avframe = new AVFrame
                        {
                            data = data,
                            linesize = linesize,
                            height = sourceSize.Height
                        };
                        var convertedFrame = vfc.Convert(avframe);

                        byte[] byteData = vse.Encode(convertedFrame);

                        return byteData;
                    }
                }
            }

        }

        private unsafe void DecodeToH263(byte[] encodedData)
        {
            using (H263VideoStreamDecoderCunsult decoder = new H263VideoStreamDecoderCunsult(25, new System.Drawing.Size(704, 576)))
            {
                decoder.DecodeFrame(encodedData, decoder.FrameSize, out MemoryStream stream);

                pictureBox2.Image = Image.FromStream(stream);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _capture.Release();
            senderThread.Abort();
            receiveThread.Abort();
        }

        private byte[] GetBitmapData(Bitmap frameBitmap)
        {
            var bitmapData = frameBitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, frameBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var length = bitmapData.Stride * bitmapData.Height;
                var data = new byte[length];
                Marshal.Copy(bitmapData.Scan0, data, 0, length);
                return data;
            }
            finally
            {
                frameBitmap.UnlockBits(bitmapData);
            }
        }
    }
}
