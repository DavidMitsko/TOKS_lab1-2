using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Lab1
{
    public partial class Form1 : Form
    {
        ComPort port;
        public Form1()
        {
            InitializeComponent();
            LoadSpeed();
            portComboBox.DataSource = System.IO.Ports.SerialPort.GetPortNames();
        }

        private byte[] ParsePackage(byte[] package)
        {
            byte flag = 0;
            if (package[0] == Convert.ToByte("01111110", 2))
            {
                flag = package[0];
            }
            byte[] undecodeData = new byte[package.Length - 2];
            for (int i = 1, j = 0; i < package.Length - 1; i++, j++)
            {
                undecodeData[j] = package[i];
            }
            byte[] decodeData = BitStuffing.DecodeData(undecodeData);

            byte[] flagPart = new byte[] { flag };
            List<byte> firstList = new List<byte>(flagPart);
            List<byte> middleList = new List<byte>(decodeData);
            List<byte> lastList = new List<byte>(flagPart);
            firstList.AddRange(middleList);
            firstList.AddRange(lastList);
            byte[] data = firstList.ToArray();

            return data;
        }

        private void portOnReceived(object sender, OnReceivedEventArgs e)
        {
            debugBox.Clear();
            debugBox.Text += BitStuffing.ConvertBytesToBinaryString(e.Data);
            byte[] data = ParsePackage(e.Data);
            byte[] data1;
            int n;
            if (data[data.Length - 2] == 0)
            {
                data1 = new byte[data.Length - 3];
                n = 2;
            }
            else
            {
                data1 = new byte[data.Length - 2];
                n = 1;
            }
            for (int i = 1, j = 0; i < data.Length - n; i++, j++)
            {
                data1[j] = data[i];
            }
            debugBox.Text += "\r\n";
            foreach (byte bt in e.Data)
            {
                debugBox.Text += Convert.ToString(bt, 16) + " ";
            }
            debugBox.Text += "\r\n";
            debugBox.Text += BitStuffing.ConvertBytesToBinaryString(data1);
            Encoding enc = Encoding.GetEncoding(1251);
            String buffer = enc.GetString(data1);
            textBox2.Text += buffer;
        }

        private void LoadSpeed()
        {
            int[] speed = new int[] {50, 75, 110, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            speedComboBox.DataSource = speed;
            speedComboBox.SelectedItem = 9600;
        }

        private void SpeedComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(port != null)
                port.BaudRate = int.Parse(speedComboBox.SelectedItem.ToString());
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                port = new ComPort(portComboBox.SelectedItem.ToString(), int.Parse(speedComboBox.SelectedItem.ToString()));
                port.OnReceived += new OnReceivedHandler(portOnReceived);

                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                sendButton.Enabled = true;

                portComboBox.Enabled = false;

            }
            catch(System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            disconnectButton.Enabled = false;
            sendButton.Enabled = false;
            connectButton.Enabled = true;
            speedComboBox.Enabled = true;
            portComboBox.Enabled = true;

            if(port != null)
            {
                port.close();
            }

        }

        private byte[] CreatePackage(string message)
        {
            byte flag = Convert.ToByte("01111110", 2);
            byte[] bytesSent = Encoding.ASCII.GetBytes(message);
            byte[] dataAfterStuffing = BitStuffing.CodeData(bytesSent);

            byte[] flagPart = new byte[] { flag };
            List<byte> firstList = new List<byte>(flagPart);
            List<byte> middleList = new List<byte>(dataAfterStuffing);
            List<byte> lastList = new List<byte>(flagPart);
            firstList.AddRange(middleList);
            firstList.AddRange(lastList);
            byte[] package = firstList.ToArray();
            return package;
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                debugBox.Clear();
                byte[] data;
                debugBox.Text += BitStuffing.ConvertBytesToBinaryString(Encoding.ASCII.GetBytes(textBox.Text));
                data = CreatePackage(textBox.Text);
                port.serialPort_SendData(data);
                debugBox.Text += "\r\n";
                foreach (byte bt in data)
                {
                    debugBox.Text += Convert.ToString(bt, 16) + " ";
                }
                debugBox.Text += String.Format("\r\n");
                debugBox.Text += BitStuffing.ConvertBytesToBinaryString(data);
            }
            catch(System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            textBox.Clear();
        }

        private void clearButton1_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }
    }
}
