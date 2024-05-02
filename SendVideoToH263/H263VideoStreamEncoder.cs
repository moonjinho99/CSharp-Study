using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using OpenCvSharp;
using FFmpeg.AutoGen;

namespace SendVideoToH263
{
    public sealed unsafe class H263VideoStreamEncoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;
        private readonly SwsContext* _pSwsContext;
        private readonly int _linesizeU;
        private readonly int _linesizeY;
        private readonly int _linesizeV;
        private readonly int _uSize;
        private readonly int _ySize;



        public H263VideoStreamEncoder(int fps, System.Drawing.Size frameSize)
        {
            ffmpeg.avcodec_register_all();
            ffmpeg.av_register_all();

            var codecId = AVCodecID.AV_CODEC_ID_H263;
            var pCodec = ffmpeg.avcodec_find_encoder(codecId);

            _pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
            _pCodecContext->width = frameSize.Width;
            _pCodecContext->height = frameSize.Height;

            _pCodecContext->time_base = new AVRational { num = 1, den = fps };
            _pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;


            ffmpeg.av_opt_set(_pCodecContext->priv_data, "preset", "veryslow", 0);
            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null);

            _pFrame = ffmpeg.av_frame_alloc();
            _pFrame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
            _pFrame->width = frameSize.Width;
            _pFrame->height = frameSize.Height;

            ffmpeg.av_frame_get_buffer(_pFrame, 32);

            _pPacket = ffmpeg.av_packet_alloc();

            _pSwsContext = ffmpeg.sws_getContext(frameSize.Width, frameSize.Height, AVPixelFormat.AV_PIX_FMT_BGR24,
            frameSize.Width, frameSize.Height, AVPixelFormat.AV_PIX_FMT_YUV420P,
             ffmpeg.SWS_BICUBIC, null, null, null);
        }

        public byte[] EncodeFrame(Mat frame)
        {
            byte[] encodedData = null;

            byte[] frameBytes = new byte[frame.Total() * frame.ElemSize()];
            Marshal.Copy(frame.Data, frameBytes, 0, frameBytes.Length);

            byte_ptrArray8 srcData = new byte_ptrArray8 { [0] = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(frameBytes, 0) };
            int pixelBytes = frame.ElemSize();
            int width = frame.Width;
            int stride = pixelBytes * width;
            int_array8 srcLinesizes = new int_array8 { [0] = stride };
            byte_ptrArray8 dstData = _pFrame->data;
            int_array8 dstLinesizes = _pFrame->linesize;
            ffmpeg.sws_scale(_pSwsContext, srcData, srcLinesizes, 0, frame.Height, dstData, dstLinesizes);

            ffmpeg.avcodec_send_frame(_pCodecContext, _pFrame);

            while (ffmpeg.avcodec_receive_packet(_pCodecContext, _pPacket) == 0)
            {
                encodedData = new byte[_pPacket->size];
                Marshal.Copy((IntPtr)_pPacket->data, encodedData, 0, _pPacket->size);

                ffmpeg.av_packet_unref(_pPacket);
            }

            return encodedData;
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
            var data = (IntPtr)pFrame->data[0];
            var rawData = new byte[frame.Total()];
            Marshal.Copy(data, rawData, 0, rawData.Length);
            Marshal.Copy(rawData, 0, frame.Data, rawData.Length);

            return frame;
        }

        public void Dispose()
        {
            ffmpeg.avcodec_close(_pCodecContext);
            ffmpeg.av_free(_pFrame);
            ffmpeg.av_free(_pPacket);
            ffmpeg.sws_freeContext(_pSwsContext);
        }
    }
}
