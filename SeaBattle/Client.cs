using System.Net.Sockets;
using System.Text;

namespace SeaBattle;

public class Client
{
    private readonly string _version = "1.0";
    private readonly TcpClient _client;
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly string _name;
    private bool _connected;
    
    public Client(string ipAddress, string port, string name)
    {
        _ipAddress = ipAddress;
        _port = int.Parse(port);
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
        NetworkStream stream = _client.GetStream();
        _connected = true;
        byte[] data = Encoding.UTF8.GetBytes($"{_name}:{_version}");
        stream.Write(data, 0, data.Length);

        if (!GetServerResponse(stream))
            _connected = false;
        
        
        while (true)
        {
            string input = Console.ReadLine();

            data = Encoding.UTF8.GetBytes(input);
            stream.Write(data, 0, data.Length);
            
            if (input == "/q")
            {
                break;
            }
        }

        if (_connected)
        {
            _connected = false;
            _client.Close();
        }


        //_client.Close();
        
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
        switch (note)
        {
            case "error":
                PrintColored(message, ConsoleColor.Red);
                return false;
            case "warning":
                PrintColored(message, ConsoleColor.Yellow);
                break;
            case "success":
                PrintColored(message, ConsoleColor.Green);
                break;
            default:
                PrintColored($"Unknown note: {message}", ConsoleColor.Blue);
                break; 
        }
        return true;
    }
}