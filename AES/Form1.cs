﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;


namespace AES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        public static byte[] GenerateSalt()
        {
            byte[] data = new byte[32];
            using (RNGCryptoServiceProvider rgnCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rgnCryptoServiceProvider.GetBytes(data);
            }
            return data;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "All files|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    textBox1.Text = ofd.FileName;
            }
        }
        private void FileEncrypt(string inputFile, string password)
        {
            byte[] salt = GenerateSalt();
            byte[] passwords = Encoding.UTF8.GetBytes(password);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;//aes 256 bit encryption c#
            AES.BlockSize = 128;//aes 128 bit encryption c#
            AES.Padding = PaddingMode.PKCS7;
            var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;
            using (FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create))
            {
                fsCrypt.Write(salt, 0, salt.Length);
                using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                    {
                        byte[] buffer = new byte[1048576];
                        int read;
                        while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
        private void FileDecrypt(string inputFileName, string outputFileName, string password)
        {
            byte[] passwords = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];
            using (FileStream fsCrypt = new FileStream(inputFileName, FileMode.Open))
            {
                fsCrypt.Read(salt, 0, salt.Length);
                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;//aes 256 bit encryption c#
                AES.BlockSize = 128;//aes 128 bit encryption c#
                var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;
                using (CryptoStream cryptoStream = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (FileStream fsOut = new FileStream(outputFileName, FileMode.Create))
                    {
                        int read;
                        byte[] buffer = new byte[1048576];
                        while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsOut.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string password = "rog";
            GCHandle gCHandle = GCHandle.Alloc(password, GCHandleType.Pinned);
            FileEncrypt(textBox1.Text, password);
            ZeroMemory(gCHandle.AddrOfPinnedObject(), password.Length * 2);
            gCHandle.Free();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string password = "rog";
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);
            FileDecrypt(textBox1.Text + ".aes", textBox1.Text, password);
            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }
    }
}
