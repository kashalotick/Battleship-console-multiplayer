using System.Net.Sockets;

namespace SeaBattle;

public class Client
{
    private readonly string _version = "1.0";
    private readonly TcpClient _client;
    private readonly string _ipAddress;
    private readonly string _port;
    private readonly string _name;
    
    public Client(string ipAdress, string port, string name)
    {
        _client = new TcpClient();
        _ipAddress = ipAdress;
        _port = port;
        _name = name;
    }

    public void Callback()
    {
        Console.WriteLine(_version);
        Console.WriteLine(_client);
        Console.WriteLine(_ipAddress);
        Console.WriteLine(_port);
        Console.WriteLine(_name);
    }
}