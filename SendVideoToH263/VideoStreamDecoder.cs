using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FFmpeg.AutoGen.Example
{
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;

        public VideoStreamDecoder(byte[] encodedData)
        {
            _pCodecContext = null;
            _pFrame = ffmpeg.av_frame_alloc();
            _pPacket = ffmpeg.av_packet_alloc();

            if (_pFrame == null || _pPacket == null)
            {
                throw new InvalidOperationException("Failed to allocate frame or packet.");
            }

            fixed (byte* pData = encodedData)
            {
                ffmpeg.av_init_packet(_pPacket);
                _pPacket->data = pData;
                _pPacket->size = encodedData.Length;

                var pCodec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H263);
                if (pCodec == null)
                {
                    throw new InvalidOperationException("Unsupported codec.");
                }

                _pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
                if (_pCodecContext == null)
                {
                    throw new InvalidOperationException("Failed to allocate codec context.");
                }

                if (ffmpeg.avcodec_open2(_pCodecContext, pCodec, null) < 0)
                {
                    throw new InvalidOperationException("Failed to open codec.");
                }
            }
        }

        public void Dispose()
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_free(_pFrame);

            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            if (_pCodecContext != null)
            {
                ffmpeg.avcodec_close(_pCodecContext);
                
            }
        }

        public bool TryDecodeNextFrame(out AVFrame frame)
        {
            int error = ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket);
            if (error < 0)
            {
                if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    frame = *_pFrame;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Failed to send packet to decoder.");
                }
            }

            error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            if (error < 0)
            {
                if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN) || error == ffmpeg.AVERROR_EOF)
                {
                    frame = *_pFrame;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Failed to receive frame from decoder.");
                }
            }

            frame = *_pFrame;
            return true;
        }
    }
}
