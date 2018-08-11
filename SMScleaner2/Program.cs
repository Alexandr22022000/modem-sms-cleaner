using System;
using System.Text;
using System.IO.Ports;

namespace TestSMS_2
{
    class Program
    {
        static Modem modem = null;

        static void Main(string[] args)
        {
            string port = Console.ReadLine();
            Modem modem = new Modem("COM" + port);

            modem.GetCIMI();

            modem.DelAllMessage();

            Console.WriteLine("Success!");

            modem.Stop();

            Console.ReadLine();
        }
    }

    class Modem
    {
        SerialPort port;
        bool isError = false;

        public Modem(string comPort)
        {
            port = new SerialPort();

            port.BaudRate = 2400; // еще варианты 4800, 9600, 28800 или 56000
            port.DataBits = 7; // еще варианты 8, 9

            port.StopBits = StopBits.One; // еще варианты StopBits.Two StopBits.None или StopBits.OnePointFive         
            port.Parity = Parity.Odd; // еще варианты Parity.Even Parity.Mark Parity.None или Parity.Space

            port.ReadTimeout = 500; // самый оптимальный промежуток времени
            port.WriteTimeout = 500; // самый оптимальный промежуток времени

            port.Encoding = Encoding.GetEncoding("windows-1251");
            port.PortName = comPort;

            // незамысловатая конструкция для открытия порта
            if (port.IsOpen)
                port.Close(); // он мог быть открыт с другими параметрами
            try
            {
                port.Open();
            }
            catch (Exception e)
            {
                isError = true;
                Console.WriteLine("Connecting error!");
            }

            port.DataReceived += ReadData;
        }

        public void Stop()
        {
            if (isError) return;
            port.Close();
        }

        private void ReadData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();

            //Console.WriteLine(data);

            if (data.IndexOf("CIMI", 0) != -1)
            {
                int i = data.IndexOf("\n", 0) + 1;
                if (data.IndexOf("\n", i) == -1)
                {
                    Console.WriteLine("Connection - OK");
                    return;
                }
                data = data.Substring(i, data.IndexOf("\n", i) - (i + 1));
                Console.WriteLine(data + " - CIMI\n");
            }
        }

        public void DelAllMessage()
        {
            if (isError) return;

            port.Write("AT+CPMS =\"SM\"" + "\r\n");
            System.Threading.Thread.Sleep(500);

            for (int i = 0; i < 10; i++)
            {
                port.Write("AT+CMGD=" + i + "\r\n");
                System.Threading.Thread.Sleep(500);
            }
        }

        public void GetCIMI()
        {
            if (isError) return;

            port.Write("AT+CIMI" + "\r\n");
            System.Threading.Thread.Sleep(500);
        }
    }
}
