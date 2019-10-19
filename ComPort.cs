using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Lab1
{
    public class OnReceivedEventArgs:EventArgs
    {
        private byte[] data;
        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        public OnReceivedEventArgs(byte[] data)
        {
            this.data = data;
        }
    }

    public delegate void OnReceivedHandler(object sender, OnReceivedEventArgs e);
    class ComPort
    {
        public event OnReceivedHandler OnReceived;

        private SerialPort serialPort;

        public String Name { get; private set; }

        public int BaudRate
        {
            get { return serialPort.BaudRate; }
            set { serialPort.BaudRate = value; }
        }

        public ComPort(String name, int speed)
        {
            this.Name = name;
            try
            {
                serialPort = new SerialPort(name, speed, Parity.None, 8, StopBits.One);
                serialPort.ReadBufferSize = 2048;
                serialPort.WriteBufferSize = 2048;
                serialPort.Open();
            }
            catch(Exception e)
            {
                throw;
            }
            serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);

           // Encoding enc = Encoding.GetEncoding(1251);
            //String buffer = enc.GetString(data);

            OnReceivedEventArgs args = new OnReceivedEventArgs(data);
            OnReceived.Invoke(this, args);
        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show(e.EventType.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void serialPort_SendData(byte[] package_b)
        {
            try
            {
                //Encoding enc = Encoding.GetEncoding(1251);
                //byte[] package_b = enc.GetBytes(data);

                while (serialPort.BytesToRead != 0)
                    Thread.Sleep(20);

                serialPort.RtsEnable = true;
                serialPort.Write(package_b, 0, package_b.Length);

                Thread.Sleep(100);   
                serialPort.RtsEnable = false;
            }
            catch (Exception ex)
            {
                serialPort.RtsEnable = false;
                throw ex;
            }
        }

        public void close()
        {
            while (serialPort.BytesToRead != 0)
            {
                Thread.Sleep(50);
            }
            serialPort.Close();
        }
    }
}
