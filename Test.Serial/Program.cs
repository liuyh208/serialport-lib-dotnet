using System;
using System.IO;
using System.Threading;

using SerialPortLib;
using NLog;

namespace Test.Serial
{
    class MainClass
    {
        private static string defaultPort = "COM1";
        private static SerialPortInput serialPort;
        private static SerialPortInput serialPort2;
        private static  int sumBytes = 0;
        public static void Main(string[] args)
        {
            // NOTE: To disable debug output uncomment the following two lines
            //LogManager.Configuration.LoggingRules.RemoveAt(0);
            //LogManager.Configuration.Reload();

            serialPort = new SerialPortInput();
            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;

            serialPort2=new SerialPortInput();
            serialPort2.SetPort("COM2");
            serialPort2.Connect();

            serialPort2.MessageReceived += SerialPort2_MessageReceived;

            while (true)
            {
                Console.WriteLine("\nPlease enter serial to open (eg. \"COM7\" or \"/dev/ttyUSB0\" without double quotes),");
                Console.WriteLine("or enter \"QUIT\" to exit.\n");
                Console.Write("Port [{0}]: ", defaultPort);
                string port = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(port))
                    port = defaultPort;
                else
                    defaultPort = port;

                // exit if the user enters "quit"
                if (port.Trim().ToLower().Equals("quit"))
                    break;
            
                serialPort.SetPort(port, 115200);
                serialPort.Connect();
                sumBytes = 0;
                Console.WriteLine("Waiting for serial port connection on {0}.", port);
                while (!serialPort.IsConnected)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                }
                // This is a test message (ZWave protocol message for getting the nodes stored in the Controller)
                var testMessage = new byte[] { 0x01, 0x03, 0x00, 0x02, 0xFE };
                // Try sending some data if connected
                if (serialPort.IsConnected)
                {
                    //Console.WriteLine("\nConnected! Sending test message 5 times.");
                    //for (int s = 0; s < 5; s++)
                    //{
                    //    Thread.Sleep(1000);
                    //    Console.WriteLine("\nSEND [{0}]", (s + 1));
                    //    serialPort.SendMessage(testMessage);
                    //}
                    
                    var bs = File.ReadAllBytes(@"d://TestData.txt");
                    Console.WriteLine("{0} start Send file^.",DateTime.Now.ToLocalTime());
                    serialPort.SendMessage(bs);
                    Console.WriteLine("{0} Send file over,total: {1} bytes", DateTime.Now.ToLocalTime(),bs.Length);
                }
                Console.WriteLine("\nTest sequence completed, now disconnecting.");

               // serialPort.Disconnect();
            }
        }

        static void SerialPort2_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            sumBytes = sumBytes + args.Data.Length;
            Console.WriteLine("{1} COM2 Received bytes: {0} / {2 }", args.Data.Length, DateTime.Now.ToLongTimeString(), sumBytes);
            // On every message received we send an ACK message back to the device
           // serialPort.SendMessage(new byte[] { 0x06 });
        }

        static void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("Serial port connection status = {0}", args.Connected);
        }
    }
}
