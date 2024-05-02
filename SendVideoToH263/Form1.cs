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

namespace SendVideoToH263
{
    public partial class Form1 : Form
    {

        private VideoCapture _capture;
        private H263VideoStreamEncoder _encoder;
        private Thread _videoThread;
        private Thread _changeThread;
             
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

        private void ChangeVideo()
        {
            if (_encoder == null)
            {
                MessageBox.Show("실시간 영상이 아직 출력되고 있지 않습니다.", "알림",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Mat frame = new Mat())
            {
                while(true)
                {
                    _capture.Read(frame);
                    if (!frame.Empty())
                    {
                        byte[] encodedFrame = _encoder.EncodeFrame(frame);
                        Mat decodedFrame = _encoder.DecodeFrame(encodedFrame);

                        pictureBox2.Invoke(new Action(() =>
                        {
                            pictureBox2.Image = decodedFrame.ToBitmap();
                        }));
                    }
                }              
            }
        }


        private void PlayVideo()
        {
            _encoder = new H263VideoStreamEncoder(25, new System.Drawing.Size(704, 576));

            using (Mat frame = new Mat())
            {
                while(true)
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
                }
            }
        }
    }
}
