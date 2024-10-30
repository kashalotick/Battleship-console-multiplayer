using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace SeaBattle;

public class Server
{
    private readonly string _version = "1.0";
    private readonly TcpListener _server;
    private readonly List<TcpClient> _clientsList = new List<TcpClient>();
    private readonly int _port;
    private bool _active = false;
    
    public Server(int port)
    {
        _port = port;
        _server = new TcpListener(IPAddress.Any, _port);
        
        ServerWrite(findIPv4() + ":" + port);

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
            string forVerifying;
            
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                forVerifying = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception e)
            {
                ServerWrite(e.Message);
                continue;
            }
            //stream.Close();
            if (forVerifying.Split(":")[1] != _version)
            {
                ServerWrite("Connection rejectted");
                ServerResponce(client, "Connection rejected - different versions.", "error");
                client.Close();
            }
            else
            {
                ServerWrite("Verified");
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
    }
    
    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        
        int bytesRead;

        try
        {
            while (client.Connected)
            {
                byte[] buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ServerWrite($"Получено сообщение: {message}");
                
                if (message[0] == '/')
                {
                    if (message == "/ip")
                    {
                        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                        {
                            ServerWrite(ip + ":" + _port);
                        }
                    }else if (message.Substring(0, 3) == "/cl")
                    {
                        TcpClient currentclient = _clientsList[int.Parse(message.Split(" ")[1])];
                        //Console.WriteLine(int.Parse(message.Split(" ")[1]));
                        IPEndPoint remoteEndPoint = currentclient.Client.RemoteEndPoint as IPEndPoint;
                        IPEndPoint localEndPoint = currentclient.Client.LocalEndPoint as IPEndPoint;
                        ServerWrite($"--------------------------");
                        ServerWrite($"Удаленный IP-адрес клиента: {remoteEndPoint.Address}");
                        ServerWrite($"Удаленный порт клиента: {remoteEndPoint.Port}");
                        ServerWrite($"Локальный IP-адрес сервера: {localEndPoint.Address}");
                        ServerWrite($"Локальный порт сервера: {localEndPoint.Port}");
                        ServerWrite($"--------------------------");
                    } else if (message.Substring(0, 5) == "/tell")
                    {
                        TcpClient currentclient = _clientsList[int.Parse(message.Split(" ")[1])];
                        string tell = message.Split("\'")[1];
                        Responce(currentclient, tell);;
                      
                    }
                }


                
            }

            void Responce(TcpClient client, string response)
            {
                NetworkStream stream = client.GetStream();
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
        }
        catch (Exception ex)
        {
            ServerWrite($"Ошибка: {ex.Message}");
        }
        finally
        {
            client.Close();
            _clientsList.Remove(client);
            ServerWrite($"Client left");
            ServerWrite($"Count of clients: {_clientsList.Count}.");
        }

    }
    
    public void Stop()
    {
        _active = false;
        _server.Stop();
    }

    private string findIPv4()
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