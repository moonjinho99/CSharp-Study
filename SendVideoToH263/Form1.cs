using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFmpeg.AutoGen;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace SendVideoToH263
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;

        private H263VideoStreamEncoder _encoder;
        private Thread _videoThread;
        private Thread _changeThread;
        private byte[] encodedData;


        public Form1()
        {
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0);

            _videoThread = new Thread(PlayVideo);
            _videoThread.IsBackground = true;
            _videoThread.Start();
        }

        private void ChangeBtn_Click(object sender, EventArgs e)
        {
            _changeThread = new Thread(ChangeVideo);
            _changeThread.IsBackground = true;
            _changeThread.Start();
        }

        private unsafe void ChangeVideo()
        {
            using (Mat frame = new Mat())
            {
                while (true)
                {
                    _capture.Read(frame);
                    if (!frame.Empty())
                    {
                        encodedData = EncodeToH263(frame);
                        DecodeToH263(encodedData, frame);
                    }
                }
            }
        }


        private void PlayVideo()
        {
            using (Mat frame = new Mat())
            {
                while (true)
                {
                    _capture.Read(frame);
                    if (frame.Empty())
                        break;

                    pictureBox1.Invoke(new Action(() =>
                    {
                        MemoryStream stream = new MemoryStream();
                        stream = frame.ToMemoryStream();
                        pictureBox1.Image = Image.FromStream(stream);
                        
                    }));

                    Thread.Sleep(30);
                }
            }
        }


        private unsafe byte[] EncodeToH263(Mat frame)
        {
            var fps = 25;
            var sourceSize = new System.Drawing.Size(frame.Width, frame.Height);
            var sourcePixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
            var stream = frame.ToMemoryStream();
            using (var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat))
            {
                using (var vse = new H263VideoStreamEncoder(stream, fps, destinationSize))
                {
                    byte[] bitmapData;

                    using (var frameImage = Image.FromStream(stream))
                    using (var frameBitmap = frameImage is Bitmap bitmap ? bitmap : new Bitmap(frameImage))
                    {
                        bitmapData = GetBitmapData(frameBitmap);
                    }

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

                        SaveEncodedDataToFile(byteData, @"D:\media\encoded_video.h264");
                        return byteData;
                                             
                    }
                }
            }
        }

        private unsafe void DecodeToH263(byte[] encodedData, Mat frame)
        {
            using (H263VideoStreamDecoder decoder = new H263VideoStreamDecoder(25, new System.Drawing.Size(frame.Width, frame.Height)))
            {
                /* decoder.DecodeFrame(encodedData, decoder.FrameSize, out AVFrame avframe);
                 var mat = new Mat(avframe.height, avframe.width, MatType.CV_8UC3, (IntPtr)avframe.data[0]);

                 return mat;*/
                /* decoder.DecodeFrame(encodedData, new System.Drawing.Size(frame.Width, frame.Height), out Mat mat);

                 pictureBox2.Image = Image.FromStream(mat.ToMemoryStream());*/

                decoder.DecodeFrame(encodedData, decoder.FrameSize, out MemoryStream stream);

                pictureBox2.Image = Image.FromStream(stream);
            }
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

        public void SaveEncodedDataToFile(byte[] data, string filePath)
        {
            // 바이트 배열을 파일에 씁니다.
            File.WriteAllBytes(filePath, data);
        }
    }
}

