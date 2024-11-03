using Console = System.Console;
using System.Net;

namespace SeaBattle;

class Program
{
    static void Main(string[] args)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        
        string[] modeNames = ["1 - Join", "2 - Create and join", "3 - Server only"];
        int[] modes = [0, 0, 0];
        int choice = 0;
        bool confirmed = false;
        
        while (true)
        {
            Console.Clear();
            
            PrintWelcomeText();
            
            modes[choice] = 1;
            int i = 0;
            
            foreach (var mode in modes)
            {
                ConsoleColor color;
                if (mode == 1)
                    color = ConsoleColor.Yellow;
                else
                    color = ConsoleColor.Gray;
                WriteLineColored($"{modeNames[i]}", color);
                i++;
            }
            ConsoleKeyInfo input = Console.ReadKey();
            modes[choice] = 0;
            
            switch (input.Key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    choice--;
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    choice++;
                    break;
                case ConsoleKey.D1:
                    choice = 0;
                    break;
                case ConsoleKey.D2:
                    choice = 1;
                    break;
                case ConsoleKey.D3:
                    choice = 2;
                    break;
                case ConsoleKey.Enter:
                    confirmed = true;
                    break;
            }
            if (choice > 2)
            {
                choice = 0;
            }
            else if (choice < 0)
            {
                choice = 2;
            }
            
            if (confirmed)
                break;
        }
        
        
        
        if (choice == 0)
        {
// ----------------------------------- Skip for test

            // string ipAddress = "localhost";
            // string port = "5000";
            // if (Environment.OSVersion.Platform == PlatformID.Unix)
            // {
            //     port = "5001";
            // }
            // string name = "Client";
            
// ----------------------------------- Connect as client

            string ipAddress;
            string port;
            string name;
            
            Console.Clear();
            Console.WriteLine("Enter the adress for connection - ip:port");
            string address = Console.ReadLine();
            
            
            if (address[address.Length - 4] != ':')
                address += ":5000";
            
            ipAddress = address.Split(":")[0];
            port = address.Split(":")[1];
            
            
            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);
            Console.WriteLine($"{name} - {ipAddress}:{port}");
            Client client = new Client(ipAddress, port, name);
            client.Connect();
        } else if (choice == 1)
        { 
 // ----------------------------------- Skip for test
 
            // string portStr = "5000";
            // int port = 5000;
            // if (Environment.OSVersion.Platform == PlatformID.Unix)
            // {
            //     portStr = "5001";
            //     port = 5001;
            // }
            // string ipAddress = "localhost";
            // string name = "HOST";
            
// ----------------------------------- Create Server and connect as client

            string ipAddress = "localhost";
            string name;
            
            Console.Clear();

            (string PortStr, int Port) serverPort = EnterServerPort();
            string portStr = serverPort.PortStr;
            int port = serverPort.Port;
            
            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);
            
            Server server = new Server(port);
            Thread serverThread = new Thread(new ThreadStart(server.Start));
            serverThread.Start();
            
            Thread.Sleep(250);
            
            Client client = new Client(ipAddress, portStr, name);
            client.Connect();
        } else if (choice == 2)
        {
            string ipAddress = "localhost";
            string name;
            
            Console.Clear();

            (string PortStr, int Port) serverPort = EnterServerPort();
            string portStr = serverPort.PortStr;
            int port = serverPort.Port;
            
            Server server = new Server(port);
            Thread serverThread = new Thread(new ThreadStart(server.Start));
            serverThread.Start();
        }

        (string PortStr, int Port) EnterServerPort()
        {
            string portStr;
            int port;
            
            Console.WriteLine("Enter the port (default: Windows - 5000, Mac - 5001)");

            readPort:
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("       ");
            Console.SetCursorPosition(0, 1);
            portStr = Console.ReadLine();
            if (portStr == "")
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    portStr = "5000";
                    //Console.WriteLine("Windows");
                } else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    portStr = "5001";
                    //Console.WriteLine("Mac");
                }
                else
                {
                    WriteLineColored("Unknown OS, write it yourself", ConsoleColor.Red);
                    goto readPort;
                }
                Console.SetCursorPosition(0, 1);
                Console.WriteLine(portStr);
            } else if (portStr.Length != 4)
            {
                WriteLineColored("Invalid port.", ConsoleColor.Red);
                goto readPort;
            }
            
            try
            {
                port = int.Parse(portStr);
            }
            catch (FormatException)
            {
                WriteLineColored("Invalid port.", ConsoleColor.Red);
                goto readPort;
            }

            return (portStr, port);
        }


        static void PrintWelcomeText()
        {
            WriteLineColored("Battleship", ConsoleColor.Blue);
            Console.WriteLine("\nControls" +
                              "\n  WASD / ↑←↓→ - Move ship/cursor" +
                              "\n  1,2,3,4 - Choose ship" +
                              "\n  R - Rotate ship" +
                              "\n  Enter - Place ship / Shoot / Confirm" +
                              "\n  Backspace - Delete ship" +
                              "\n  C - Confirm position of ships" +
                              "\n");
            
        }
        
        
        static void WriteLineColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        // static void WriteColored(string text, ConsoleColor color)
        // {
        //     Console.ForegroundColor = color;
        //     Console.Write(text);
        //     Console.ForegroundColor = ConsoleColor.Gray;
        // }
    }
   
}

