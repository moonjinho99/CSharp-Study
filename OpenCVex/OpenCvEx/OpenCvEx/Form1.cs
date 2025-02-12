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
using System.IO;

namespace OpenCvEx
{

    public enum DRAW_MODE : int
    {
        SHAPEMODE = 0,
        ERASERMODE = 1,
        EDITMODE = 2
    }

    public partial class Form1 : Form
    {
        VideoWriter vw = new VideoWriter();
        VideoCapture vc;
        int curMode = 2;
        OpenCvSharp.Point init;
        bool drawing = false;
        Mat temp = new Mat();

        public Form1()
        {
            InitializeComponent();
        }

       private Cursor LoadCursor(byte[] cursorFile)
        {
            MemoryStream cursorMemoryStream = new MemoryStream(cursorFile);
            Cursor hand = new Cursor(cursorMemoryStream);
            return hand;
        }


        private void SetDrawMode(int mode)
        {
            switch (mode)
            {
                case (int)DRAW_MODE.SHAPEMODE:
                    curMode = (int)DRAW_MODE.SHAPEMODE;
                    break;
                case (int)DRAW_MODE.ERASERMODE:
                    curMode = (int)DRAW_MODE.ERASERMODE;
                    break;
                case (int)DRAW_MODE.EDITMODE:
                    break;
                default:
                    break;
            }
        }



        private void loadBtn_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            vc = new VideoCapture(0);
            timer1.Enabled = true;
        }

        private void drawBtn_Click(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.SHAPEMODE);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           Mat frame = new Mat();
            vc.Read(frame);
            if (frame.Empty()) return;
            else
            {
                try
                {
                    this.Invoke((Action)(() =>
                    {
                        if (curMode == (int)DRAW_MODE.SHAPEMODE)
                        {
                            temp = frame;
                            if (dataGridView1.Rows.Count > 0)
                            {
                                drawcross(temp, init);
                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                {
                                    OpenCvSharp.Point f = new OpenCvSharp.Point(Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value), Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value));
                                    OpenCvSharp.Point b = new OpenCvSharp.Point(Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value), Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value));
                                    Scalar sc = new Scalar(0, 255, 0);

                                    int thi = 1;
                                    Rect rec = new Rect(f.X, f.Y, b.X, b.Y);
                                    Cv2.Rectangle(temp, rec, sc, thi);
                                    Mat org = temp;
                                    org = org.SubMat(rec);
                                    OpenCvSharp.Point k = new OpenCvSharp.Point(b.X + f.X, b.Y + f.Y);
                                }
                            }
                            else
                            {
                                drawcross(temp, init);
                            }
                            pictureBox1.Image = BitmapConverter.ToBitmap(temp);

                        }
                    }), null);
                    drawBtn.Enabled = true;
                }
                catch { }
            }
        }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if(curMode == (int)DRAW_MODE.SHAPEMODE)
            {
                init = new OpenCvSharp.Point((double)e.X, (double)e.Y);
                drawing = true;
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            SetDrawMode(curMode);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.EDITMODE);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(curMode == (int)DRAW_MODE.SHAPEMODE)
            {
                if(drawing == true)
                {
                    OpenCvSharp.Point a = new OpenCvSharp.Point((double)e.X, (double)e.Y);
                    Mat tp = new Mat();

                    if(pictureBox1.InvokeRequired)
                    {
                        pictureBox1.Invoke(new MethodInvoker(
                        delegate ()
                        {
                            pictureBox1.Image = BitmapConverter.ToBitmap(temp);
                        }));
                            
                    }
                    else
                    {
                        pictureBox1.Image = BitmapConverter.ToBitmap(temp);
                    }
                    drawnrect(temp, a);
                }
            }
            else
            {
                OpenCvSharp.Point a = new OpenCvSharp.Point(e.X, e.Y);
                Mat tp = new Mat();
                if(pictureBox1.InvokeRequired)
                {
                    pictureBox1.Invoke(new MethodInvoker(
                        delegate ()
                        {
                            pictureBox1.Image = BitmapConverter.ToBitmap(temp);
                        }));
                }
                else
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(temp);
                }
                drawcross(temp, a);
                init = a;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            OpenCvSharp.Point a = new OpenCvSharp.Point((double)e.X, (double)e.Y);
            Mat t = new Mat();
            Scalar sc = new Scalar(0, 255, 0);
            dataGridView1.Rows.Add(Convert.ToString(init.Y), Convert.ToString(init.X), Convert.ToString(a.X - init.X), Convert.ToString(a.Y - init.Y), "", "");
            dataGridView1.ClearSelection();
            drawing = false;
        }

        private void drawcross(Mat t, OpenCvSharp.Point m)
        {
            Cv2.Line(t, new OpenCvSharp.Point(m.X - 40, m.Y), new OpenCvSharp.Point(m.X + 40, m.Y), new Scalar(0, 255, 255), 1);

            Cv2.Line(t, new OpenCvSharp.Point(m.X, m.Y - 40), new OpenCvSharp.Point(m.X, m.Y + 40), new Scalar(0, 255, 255), 1);

            Cv2.PutText(t, Convert.ToString((int)(m.X)) + "," + Convert.ToString((int)(m.Y)), new OpenCvSharp.Point(m.X, m.Y), HersheyFonts.HersheyComplex, 0.5, new Scalar(0, 255, 255), 1);

            if(pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new MethodInvoker(
                    delegate ()
                    {
                        pictureBox1.Image = BitmapConverter.ToBitmap(t);
                    }));
            }
            else
            {
                pictureBox1.Image = BitmapConverter.ToBitmap(t);
            }
        }

        private void drawnrect(Mat t,OpenCvSharp.Point m)
        {
            Cv2.Rectangle(t, new Rect(init.X, init.Y, m.X - init.X, m.Y - init.Y), new Scalar(0, 255, 255), 1);

            if(pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new MethodInvoker(
                    delegate ()
                    {
                        pictureBox1.Image = BitmapConverter.ToBitmap(t);
                    }));
            }
            else
            {
                pictureBox1.Image = BitmapConverter.ToBitmap(t);
            }
        }
    }
}
