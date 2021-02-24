using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace netCore
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] ports = SerialPort.GetPortNames();
            var thbPorts = new List<string>();
            Console.WriteLine("=========Finding all COM Ports===============");
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }

            Console.WriteLine("=======Checking THB Devices=========");
            foreach (string port in ports)
            {
                System.IO.Ports.SerialPort _serialPort = new SerialPort();
                _serialPort.PortName = port;
                _serialPort.BaudRate = 115200;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                try
                {
                    _serialPort.Open();
                    _serialPort.Write("RMX.WHOIS\r\n");
                    string message = _serialPort.ReadLine();
                    Match m = Regex.Match(message, @"HW_MODEL=THB_SENSOR");
                    if (m.Success)
                        thbPorts.Add(_serialPort.PortName);
                    _serialPort.Close();
                }
                catch
                {
                }
            }

            if (thbPorts.Count==0)
            {
                Console.WriteLine("THB Device not detected");
                return;
            }
            thbPorts.ForEach(delegate(string name) { Console.WriteLine(name); });
            Console.WriteLine("====Asking for Data=======");
            bool _continue = true;
            Console.WriteLine("Type CTRL+C  to exit");
            while (_continue)
            {
                thbPorts.ForEach(delegate(string name)
                {
                    System.IO.Ports.SerialPort _serialPort = new SerialPort();
                    _serialPort.PortName = name;
                    _serialPort.BaudRate = 115200;
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;
                    try
                    {
                        _serialPort.Open();
                        _serialPort.Write("THB.CURR\r\n");
                        string message = _serialPort.ReadLine();
                        // Console.WriteLine("Message {0}",message);
                        string temperature = Regex.Match(message, @"(?s)(?<=<T=).*?(?=>)").Value;
                        string humidity = Regex.Match(message, @"(?s)(?<=<H=).*?(?=>)").Value;
                        string pressure = Regex.Match(message, @"(?s)(?<=<P=).*?(?=>)").Value;
                        string density = Regex.Match(message, @"(?s)(?<=<D=).*?(?=>)").Value;
                        _serialPort.Close();
                        Console.WriteLine("------------------  " + name);
                        Console.WriteLine("Time: " + DateTime.Now + " °C");
                        Console.WriteLine("Temperature: " + temperature + " °C");
                        Console.WriteLine("Humidity: " + humidity + " %");
                        Console.WriteLine("Pressure: " + pressure + " hPa");
                        Console.WriteLine("Density: " + density + " kg/m3");
                    }
                    catch
                    {
                    }

                    Thread.Sleep(1000);
                });
            }

            Thread.Sleep(1000);
        }
    }
}