using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace SeaBattle;

public class Client
{
    private readonly string _version = "1.0";
    private readonly TcpClient _client;
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly string _name;
    private bool _connected;
    private NetworkStream _stream;
    public Matrix Field = new Matrix(10, 10);
    public Matrix Selection = new Matrix(10, 10);
    public Matrix EnemyField = new Matrix(10, 10);
    public int PosX;
    public int PosY;
    private string _recievedData;
    private int[][] _ships= [[1, 1, 1, 1], [1, 1, 1], [1, 1], [1]];
    private (int type, int status) _currentShip = (0, 0);
    private string _cursor = "select";
    private bool _vertical = false;
    public Client(string ipAddress, string port, string name)
    {
        _port = int.Parse(port);
        _ipAddress = ipAddress;
        // (_ipAddress, ConsoleColor.Yellow);
        // PrintColored(_port.ToString(), ConsoleColor.Yellow);
        _name = name;
        try
        {
            _client = new TcpClient(_ipAddress, _port);
        }
        catch (SocketException e)
        {
            PrintColored($"Connection Error", ConsoleColor.Red);
        }
        
    }

    public void Connect()
    {
        _stream = _client.GetStream();
        _connected = true;
        byte[] data = Encoding.UTF8.GetBytes($"{_name}:{_version}");
        _stream.Write(data, 0, data.Length);

        if (!GetServerResponse(_stream))
            _connected = false;
        
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        
            
        while (true)
        {
            Console.Clear();
            Field.WriteMatrix(PosX, PosY);
            DrawShips();
            Console.WriteLine($"{PosX}, {PosY}");
            Console.WriteLine(_currentShip);
            Console.WriteLine(_cursor);
            string input;
            ConsoleKeyInfo inputKey = Console.ReadKey();
            if (inputKey.KeyChar == 'e' || inputKey.KeyChar == 'у')
            {
                input = "";
                data = Encoding.UTF8.GetBytes($"e: {PosX};{PosY}");
                _stream.Write(data, 0, data.Length);
            } else if (inputKey.KeyChar == '/')
            {
                input = Console.ReadLine();
                data = Encoding.UTF8.GetBytes(input);
                if (input == "/q")
                {
                    break;
                }
                _stream.Write(data, 0, data.Length);
            }
            else
            {
                switch (inputKey.Key)
                {
                    case ConsoleKey.W:
                        PosY--;
                        break;
                    case ConsoleKey.A:
                        PosX--;
                        break;
                    case ConsoleKey.S:
                        PosY++;
                        break;
                    case ConsoleKey.D:
                        PosX++;
                        break;
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                    case ConsoleKey.D4:
                        ChooseShip(int.Parse(inputKey.KeyChar.ToString()));
                        break;
                    case ConsoleKey.Enter:
                        _ships[_currentShip.type][_currentShip.status] = 0;
                        ChooseShip(_currentShip.type + 1);
                        break;
                    case ConsoleKey.Backspace:
                        _ships[_currentShip.type][_currentShip.status] = 1;
                        break;
                }
            }

            if (PosY < 0)
                PosY++;
            else if (PosY > 9)
                PosY--;
            else if (PosX < 0)
                PosX++;
            else if (PosX > 9)
                PosX--;
            
            Console.WriteLine(_recievedData);
        }
        if (_connected)
        {
            _connected = false;
            Thread.Sleep(250);
        }
    }

    private void ChooseShip(int ship)
    {
        
        ship -= 1;
        int t = _currentShip.status;
        
        if (ship != _currentShip.type)
        {
            t = 0;
        }
        else
        {
            t++;
            if (t >= _ships[ship].Length)
            {
                t = 0;
            }
        }
        _currentShip = (ship, t);
        
        Console.WriteLine($"Cureent Ship: {_currentShip}");
    }

    private void DrawShips()
    {
        int i = 0;
        foreach (int[] stack in _ships)
        {
            int j = 0;
            foreach (int ship in stack)
            {

                if (_currentShip.type == i && _currentShip.status == j)
                {
                    if (_ships[i][j] == 1) 
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (_ships[i][j] == 0) 
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else
                {
                    if (_ships[i][j] == 1) 
                        Console.ForegroundColor = ConsoleColor.Gray;
                    else if (_ships[i][j] == 0) 
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                Console.Write($"({i}, {j})");
                for (int k = 0; k < i + 1; k++)
                {
                    Console.Write('\u25a0');
                }
                Console.Write("  ");
                j++;

            }
            Console.Write("\n");
            i++;
        }
    }
    private void PlacingShips()
    {
        Field.WriteMatrix(PosX, PosY);
    }

    private void ReceiveMessages()
    {
        try
        {
            while (_connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _recievedData = message;
                //PrintColored($"{message}", ConsoleColor.Magenta);
            }
        }
        catch (Exception ex)
        {
            PrintColored($"Error: {ex.Message}", ConsoleColor.Red);
        }
        finally
        {
            _client.Close();
            PrintColored($"Connection closed", ConsoleColor.Yellow);
        }
    }

    public void Callback()
    {
        Console.WriteLine(_version);
        Console.WriteLine(_client);
        Console.WriteLine(_ipAddress);
        Console.WriteLine(_port);
        Console.WriteLine(_name);
    }
    void PrintColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    private bool GetServerResponse(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        string note = message.Split("!")[1];
        message = message.Split("!")[0];
        bool isConnected = true;
        switch (note)
        {
            case "error":
                PrintColored(message, ConsoleColor.Red);
                isConnected = false;
                break;
            case "warning":
                PrintColored(message, ConsoleColor.Yellow);
                break;
            case "say":
                PrintColored(message, ConsoleColor.Magenta);
                break;
            default:
                PrintColored(message, ConsoleColor.Green);
                break;
        }
        return isConnected;
    }
}