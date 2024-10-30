using Console = System.Console;
using System.Net;

namespace SeaBattle;

class Program
{
    static void Main(string[] args)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // var field = new Matrix();
        // Console.WriteLine(field.ToString());
        //
        // for (int i = 0; i < 10; i++)
        // {
        //     for (int j = 0; j < 10; j++)
        //     {
        //         if ((j + i) % 2 == 0)
        //         {
        //             Console.BackgroundColor = ConsoleColor.DarkGray;
        //         }
        //         else
        //         {
        //             Console.BackgroundColor = ConsoleColor.Black;
        //         }
        //
        //         if (i == 5 && j > 3 && j < 7)
        //         {
        //             Console.ForegroundColor = ConsoleColor.DarkRed;
        //             Console.Write(" \u2715 ");
        //         }
        //         else
        //         {
        //             Console.ForegroundColor = ConsoleColor.White;
        //             Console.Write("   ");
        //         }
        //         
        //     }
        //     Console.Write("\n");
        // }
        //
        // Console.ReadKey();

        ConsoleKeyInfo input;
        int mode = 1;
        ConsoleColor hostColor = ConsoleColor.Gray;
        ConsoleColor joinColor = ConsoleColor.Yellow;
        while (true)
        {
            Console.Clear();
            PrintColored("1 - Join", joinColor);
            PrintColored("2 - Host", hostColor);

            input = Console.ReadKey();
            if (input.KeyChar == '1' || input.KeyChar == 'w' || input.Key == ConsoleKey.UpArrow)
            {
                mode = 1;
                hostColor = ConsoleColor.Gray;
                joinColor = ConsoleColor.Yellow;
            } else if (input.KeyChar == '2' || input.KeyChar == 's' || input.Key == ConsoleKey.DownArrow)
            {
                mode = 2;
                hostColor = ConsoleColor.Yellow;
                joinColor = ConsoleColor.Gray;
            } else if (input.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(mode);
                Console.WriteLine("Confirm");
                Console.WriteLine();
                break;
            }
        }


        if (mode == 1)
        {
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
            
            if (ipAddress == "auto")
            {
                ipAddress = findIPv4();
            }

            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);
            
            Client client = new Client(ipAddress, port, name);
            client.Connect();
        }
        else
        {
            string portStr;
            int port;
            string ipAddress = "localhost";
            string name;
            
            
            Console.Clear();
            Console.WriteLine("Enter the port (default: Windows: 5000, Mac: 5001)");
            
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
                    PrintColored("Unknown OS, write it yourself", ConsoleColor.Red);
                    goto readPort;
                }
                Console.SetCursorPosition(0, 1);
                Console.WriteLine(portStr);
            } else if (portStr.Length != 4)
            {
                PrintColored("Invalid port.", ConsoleColor.Red);
                goto readPort;
            }
            
            try
            {
                port = int.Parse(portStr);
            }
            catch (FormatException)
            {
                PrintColored("Invalid port.", ConsoleColor.Red);
                goto readPort;
            }
            
            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);
            
            Server server = new Server(port);
            Thread serverThread = new Thread(new ThreadStart(server.Start));
            serverThread.Start();
            
            Thread.Sleep(1000);
            
            Client client = new Client(ipAddress, portStr, name);
            client.Connect();
            
            // serverThread.Join();
        }
        
        
        
        static void PrintColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
       

        static string findIPv4()
        {
            string ipv4 = "";
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                var ipArr = ip.ToString().Split('.');
                if (ipArr.Length == 4)
                {
                    if (ipArr[0] == "192" || ipArr[0] == "172" || ipArr[0] == "10")
                    {
                        ipv4 = ip.ToString();
                    }
                }
            }
            return ipv4;
        }
    }
}
//localhost:5001
/*
 
0  1  2  3  4
   ■  ∙  ✕  □

0 - empty
1 - ship
2 - missed
3 - damaged
4 - killed (also damaged on your field)

Your field                       |   Enemy's field
- 0  1  2  3  4  5  6  7  8  9   |   - 0  1  2  3  4  5  6  7  8  9
0                   ∙  ∙  ∙      |   0    ∙        ∙  ∙  ∙  ∙  ∙  ∙
1       ■        ■  ∙  □  ∙      |   1    ✕ ∙      ∙  □  ∙  ■  ∙  ■
2       ■        □  ∙  □  ∙      |   2    ✕        ∙  □  ∙  ■  ∙  ✕
3       ■        □  ∙  ∙  ∙      |   3             ∙  □  ∙  ✕  ∙  ∙
4       ■                        |   4             ∙  ∙  ∙ 
5                      ■         |   5               
6       ∙          ∙             |   6      
7             ∙  ∙               |   7       ∙ ∙ 
8             ∙                  |   8       ∙           
9                                |   9                            

*/