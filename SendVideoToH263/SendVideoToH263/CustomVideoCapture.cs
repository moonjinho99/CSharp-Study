using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace SendVideoToH263
{
    public class CustomVideoCapture : VideoCapture
    {
        public CustomVideoCapture(int index) : base(index) { }

        public void SetFrameSize(int width, int height)
        {
            //Set(VideoCaptureProperties.FrameWidth, width);
            //Set(VideoCaptureProperties.FrameHeight, height);
        }
    }
}
