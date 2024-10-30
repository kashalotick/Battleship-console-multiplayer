using Console = System.Console;

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
            if (input.KeyChar == '1' || input.KeyChar == 'w')
            {
                mode = 1;
                hostColor = ConsoleColor.Gray;
                joinColor = ConsoleColor.Yellow;
            } else if (input.KeyChar == '2' || input.KeyChar == 's')
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

// 123.456.78.90
// 123.456.78.90:5002
        if (mode == 1)
        {
            Console.Clear();
            Console.WriteLine("Enter the adress for connection - ip:port");
            string adress = Console.ReadLine();
            if (adress[adress.Length - 4] != ':')
                adress += ":5000";
            
            string ipAdress = adress.Split(":")[0];
            string port = adress.Split(":")[1];
            string name;

            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);
            
            Client client = new Client(ipAdress, port, name);
            client.Callback();
        }
        else
        {
            string portStr;
            int port;
            string ipAdress = "localhost";
            string name;
            
            Console.Clear();
            Console.WriteLine("Enter the port (default: 5000)");
            
            readPort:
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("       ");
            Console.SetCursorPosition(0, 1);
            portStr = Console.ReadLine();
            if (portStr == "")
            {
                portStr = "5000";
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

            Server server = new Server(port);
            
            Console.WriteLine("Enter your name");
            do
            {
                Console.SetCursorPosition(0, 3);
                name = Console.ReadLine();
            } while (name.Length < 1);


            Client client = new Client(ipAdress, portStr, name);
            client.Callback();
        }
        
        
        
        void PrintColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
       

        
        
        
        
        
        
        
    }
}
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