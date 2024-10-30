using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace SeaBattle;

public class Server
{
    private readonly TcpListener _server;
    private readonly List<TcpClient> _clientsList = new List<TcpClient>();
    private bool _active = false;
    
    public Server(int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        _server.Start();
        _active = true;
        
        ServerResponce("Server started.");
        ServerResponce("Waiting for clients...");

        while (_active)
        {
            TcpClient client = _server.AcceptTcpClient();
            
            if (_clientsList.Count < 2)
            {
                ServerResponce("Client connected.");
                _clientsList.Add(client);
                ServerResponce($"Count of clients: {_clientsList.Count}.");
                
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
            else
            {
                ServerResponce("Connection rejectted");
                SendError(client, "Connection rejected - server if full.");
                client.Close();
            }

        }
    }

    private void HandleClient(object client)
    {
        // ServerResponce("Handle is working"); 
    }
    
    public void Stop()
    {
        _active = false;
        _server.Stop();
    }
    
    
    
    
    
    
    private void SendError(TcpClient client, string errorMessage)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(errorMessage);
        stream.Write(data, 0, data.Length);
    }
    private void ServerResponce(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}