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
        
        ServerWrite("Server started.");
        ServerWrite("Waiting for clients...");

        while (_active)
        {
            TcpClient client = _server.AcceptTcpClient();
            
            if (_clientsList.Count < 2)
            {
                ServerWrite("Client connected.");
                _clientsList.Add(client);
                ServerWrite($"Count of clients: {_clientsList.Count}.");
                ServerResponce(client, "Successful connection", "success");

                
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
            else
            {
                ServerWrite("Connection rejectted");
                ServerResponce(client, "Connection rejected - server if full.", "error");
                client.Close();
            }

        }
    }
    
    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;
        while (_active)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ServerWrite($"Получено сообщение: {message}");
                if (message == "/q")
                {
                    client.Close();
                    _clientsList.Remove(client); 
                    ServerWrite($"Count of clients: {_clientsList.Count}.");
                    break;
                }
                string response = "Сообщение получено";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        
        // finally
        // {
        //     client.Close();
        //     _clientsList.Remove(client); 
        //     ServerResponce($"Count of clients: {_clientsList.Count}.");
        //
        // }

    }
    
    public void Stop()
    {
        _active = false;
        _server.Stop();
    }
    
    
    private void ServerResponce(TcpClient client, string errorMessage, string note)
    {
        NetworkStream stream = client.GetStream();
        errorMessage += $"!{note}";
        byte[] data = Encoding.UTF8.GetBytes(errorMessage);
        stream.Write(data, 0, data.Length);
    }
    private void ServerWrite(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}