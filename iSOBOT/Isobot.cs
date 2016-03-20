using System;
using System.IO.Ports;

namespace iSOBOT
{
    public class Isobot
    {
        public int BaudRate { get; set; } = 0;
        public string PortName { get; set; } = "None";

        private SerialPort port;
        private byte[] sendBuffer;

        //Default Constructor
        public Isobot()
        {
            sendBuffer = new byte[4];
        }

        //Constructor to open serial connection
        public Isobot(int baudRate, string portName)
        {
            sendBuffer = new byte[4];
            BaudRate = baudRate;
            PortName = portName;
            SerialConnect();
        }

        // Destructor
        ~Isobot()
        {
            port.Close();
        }

        // Connect to transmitter via serial port
        public void SerialConnect()
        {
            if (BaudRate != 0 && PortName != "None")
            {
                port = new SerialPort(PortName, BaudRate);
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                try
                {
                    port.Open();
                }
                catch
                { //todo 
                }

                if (port.IsOpen)
                {
                    Console.WriteLine(PortName + " is open");
                }
                else
                {
                    Console.WriteLine("Unable to open " + PortName);
                }
            }
            else
            {
                Console.WriteLine("Port name or baud rate not set prior to opening connection");
            }
        }


        public void SendCommand(Commands cmd, Channel chan, bool repeat)
        {
            sendBuffer[0] = (byte) chan;
            sendBuffer[1] = (byte) cmd;
            sendBuffer[2] = (byte) (repeat ? 0x1 : 0x0);
            sendBuffer[3] = GetChecksum(sendBuffer[0], sendBuffer[1], sendBuffer[2]);
            port.Write(sendBuffer, 0, 4);
        }

        static byte GetChecksum(byte b1, byte b2, byte b3)
        {
            return (byte)(b1 ^ b2 ^ b3);
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.Write("Data Received:");
            Console.WriteLine(indata);
        }
    }
    public enum Commands : byte
    {
        StopRepeating = 0x00,
        Dance = 0x97,
        WalkForward = 0xb7,
        WalkBack = 0xb8,
        WalkForwardLeft = 0xb9,
        WalkForwardRight = 0xb1,
        WalkLeft = 0xbb,
        WalkRight = 0xbc,
        WalkBackLeft = 0xbd,
        WalkBackRight = 0xbe,
        GetupBelly = 0xE4,
        GetupBack = 0xE5,
    };

    public enum Channel : byte
    {
        A = 0x00,
        B = 0x01
    };
}
