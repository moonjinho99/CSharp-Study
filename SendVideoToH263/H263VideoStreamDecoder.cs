using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using OpenCvSharp;
using FFmpeg.AutoGen;
using System.Drawing;
using System.Drawing.Imaging;

namespace SendVideoToH263
{
    public sealed unsafe class H263VideoStreamDecoder : IDisposable
    {
        private readonly AVCodec* _pCodec;
        private readonly AVCodecContext* _pCodecContext;

        static H263VideoStreamDecoder()
        {
            ffmpeg.avcodec_register_all();
        }

        public Mat DecodeFrame(byte[] encodedData)
        {
            AVCodec* pCodec = ffmpeg.avcodec_find_decoder(_pCodecContext->codec_id);
            AVCodecContext* pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);

            ffmpeg.avcodec_open2(pCodecContext, pCodec, null);

            AVFrame* pFrame = ffmpeg.av_frame_alloc();

            AVPacket packet = new AVPacket();
            ffmpeg.av_init_packet(&packet);
            packet.data = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(encodedData, 0);
            packet.size = encodedData.Length;

            ffmpeg.avcodec_send_packet(pCodecContext, &packet);

            while (ffmpeg.avcodec_receive_frame(pCodecContext, pFrame) == 0)
            {
                Mat decodedFrame = ConvertFrameToMat(pFrame);

                ffmpeg.av_frame_unref(pFrame);
                return decodedFrame;
            }

            return null;
        }

        private Mat ConvertFrameToMat(AVFrame* pFrame)
        {
            var frame = new Mat(pFrame->height, pFrame->width, MatType.CV_8UC3);
            //Console.WriteLine("프레임 사이즈 : " + pFrame->height + " , "+pFrame->width);
            var data = (IntPtr)pFrame->data[0];
            var rawData = new byte[frame.Total()];
            Marshal.Copy(data, rawData, 0, rawData.Length);
            Marshal.Copy(rawData, 0, frame.Data, rawData.Length);

            return frame;
        }

        public void Dispose()
        {
            ffmpeg.avcodec_close(_pCodecContext);
            ffmpeg.av_free(_pCodecContext);
            ffmpeg.av_free(_pCodec);
        }
    }
}
