using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO.Ports;

namespace iSOBOT_PC
{
    class Program
    {
        static void Main(string[] args)
        {
            String portName = "COM3";
            int baudRate = 38400;

            SerialPort port = new SerialPort(portName, baudRate);
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.Open();

            if (port.IsOpen)
            {
                Console.WriteLine(portName + " is open");
            }
            else
            {
                Console.WriteLine("Unable to open " + portName);
            }
            byte[] buffer = new byte[4];

            while (true)
            {
                buffer[0] = 0x00;
                buffer[1] = 0x65;
                buffer[2] = 0x01;
                buffer[3] = (byte)(buffer[0] ^ buffer[1] ^ buffer[2]);
                port.Write(buffer, 0, 4);
                Thread.Sleep(10000);
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.Write("Data Received:");
            Console.WriteLine(indata);
        }
    }
}
