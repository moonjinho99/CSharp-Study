using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;


namespace SecurityToAES256
{
    public partial class Form1 : Form
    {
        static string aes_key = "AXe8YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
        static string aes_iv = "bsxnWolsAyO7kCfWuyrnqg==";
        static byte[] imgData = null;

        private static Crypto cp = new Crypto();

        public Form1()
        {
            InitializeComponent();
        }

        private void securityBtn_Click(object sender, EventArgs e)
        {
            string inputText = beforeText.Text;

            try
            {
                byte[] encryptedImage = cp.EncryptImage(imgData, Convert.FromBase64String(aes_key), Convert.FromBase64String(aes_iv));

                byte[] encryptedText = cp.EncryptStringToBytes(inputText, Convert.FromBase64String(aes_key), Convert.FromBase64String(aes_iv));

                SendDataToServer(encryptedText, encryptedImage);
            }
            catch(Exception ee)
            {
                Console.WriteLine("Error : " + ee.Message);
            }
        }       

        public void SendDataToServer(byte[] textData, byte[] imageData)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 5000);
                NetworkStream stream = client.GetStream();

                // 텍스트 데이터 헤더 작성 및 전송
                byte[] textHeader = CreateHeader("TEXT", textData.Length);
                stream.Write(textHeader, 0, textHeader.Length);
                stream.Write(textData, 0, textData.Length);

                // 이미지 데이터 헤더 작성 및 전송
                byte[] imageHeader = CreateHeader("IMG", imageData.Length);
                stream.Write(imageHeader, 0, imageHeader.Length);
                stream.Write(imageData, 0, imageData.Length);


                stream.Close();
                client.Close();

                MessageBox.Show("서버에 데이터를 보냈습니다.");
            } catch(Exception e)
            {
                MessageBox.Show("에러 : " + e.Message);
            }
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.png)|*.BMP;*.JPG;*.JPEG,*.PNG";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
               
                Image originalImg = Image.FromFile(dialog.FileName);

                pictureBox1.Image = originalImg;

                using(MemoryStream ms = new MemoryStream())
                {
                    originalImg.Save(ms, ImageFormat.Jpeg);

                    imgData = ms.ToArray();
                }
            }
        }

        public byte[] CreateHeader(string type, int length)
        {
            // 헤더 형식: [타입(4바이트)][길이(4바이트)]
            byte[] typeBytes = Encoding.UTF8.GetBytes(type.PadRight(4).Substring(0, 4));
            byte[] lengthBytes = BitConverter.GetBytes(length);

            byte[] header = new byte[8];
            Array.Copy(typeBytes, 0, header, 0, 4);
            Array.Copy(lengthBytes, 0, header, 4, 4);

            return header;
        }

    }
}
