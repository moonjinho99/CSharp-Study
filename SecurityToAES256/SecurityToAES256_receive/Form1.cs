using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace SecurityToAES256_receive
{
    public partial class Form1 : Form
    {

        static string aes_key = "AXe8YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
        static string aes_iv = "bsxnWolsAyO7kCfWuyrnqg==";
        TcpListener listener;
        Thread listenerThread;
        TcpClient client;
        NetworkStream stream;


        public Form1()
        {
            InitializeComponent();

            StartServer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void ListenForClients()
        {
            while(true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client_obj)
        {
            client = (TcpClient)client_obj;
            stream = client.GetStream();

            while(true)
            {
                if (!stream.DataAvailable) break;

                //헤더 읽기
                byte[] header = new byte[8];
                stream.Read(header, 0, header.Length);

                //헤더에서 타입과 같이 추출
                string type = Encoding.UTF8.GetString(header, 0, 4).Trim();
                int length = BitConverter.ToInt32(header, 4);

                byte[] data = new byte[length];
                stream.Read(data, 0, data.Length);

                if(type == "TEXT")
                {
                    byte[] decryptedText = DecryptBytes(data, Convert.FromBase64String(aes_key), Convert.FromBase64String(aes_iv));
                    string decryptedTextString = Encoding.UTF8.GetString(decryptedText);
                    Invoke(new Action(() =>
                    {
                        afterText.Text = decryptedTextString;
                    }));
                }
                else if(type == "IMG")
                {
                    byte[] decryptedImage = DecryptBytes(data, Convert.FromBase64String(aes_key), Convert.FromBase64String(aes_iv));
                    using(MemoryStream imgStream = new MemoryStream(decryptedImage))
                    {
                        Image image = Image.FromStream(imgStream);
                        Invoke(new Action(() =>
                        {
                            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                            pictureBox2.Image = image;
                            
                        }));
                    }

                }
            }
        }


        public static byte[] DecryptBytes(byte[] cipherBytes, byte[] key, byte[] IV)
        {
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(cipherBytes, 0, cipherBytes.Length);
                        csDecrypt.FlushFinalBlock();
                        return msDecrypt.ToArray();
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
            stream.Close();
        }
    }
}
