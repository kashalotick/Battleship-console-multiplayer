using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace SeaBattle;

public class Server
{
    private readonly string _version = "1.0";
    private readonly TcpListener _server;
    private readonly List<TcpClient> _clientsList = new List<TcpClient>();
    private readonly int _port;
    private bool _active = false;
    
    private int _readyCount = 0;
    private int _shipsAlive1 = 10;
    private Matrix _field1 = new Matrix(10, 10);
    private Matrix _enemyField1 = new Matrix(10, 10);
    private int _shipsAlive2 = 10;
    private Matrix _field2 = new Matrix(10, 10);
    private Matrix _enemyField2 = new Matrix(10, 10);

    private int _turn;
    
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
        int iteration = 0;
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        if (_clientsList.Count == 2)
        {
            foreach (var player in _clientsList)
            {
                Responce(player, "!ReadyForPlacingShips!");
            }
        }
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
                
                int sender;
                if (client == _clientsList[0])
                    sender = 0;
                else if (client == _clientsList[1])
                    sender = 1;
                else 
                 sender = -1;
                ServerWrite($"Получено сообщение от `{sender}`: {message}");
                if (message[0] == '[' && message[1] == '[')
                {
                    ServerWrite($"Got completed field from {sender}");
                    if (sender == 0)
                    {
                        _field1.Mtrx = JsonConvert.DeserializeObject<int[,]>(message);
                        // _field1.WriteMatrix(-1, -1);
                    }
                    if (sender == 1)
                    {
                        _field2.Mtrx = JsonConvert.DeserializeObject<int[,]>(message);
                        // _field2.WriteMatrix(-1, -1);
                    }
                    
                }
                else if (message[0] == '/')
                {
                    
                    if (message == "/ip")
                    {
                        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                        {
                            ServerWrite(ip + ":" + _port);
                        }
                    } 
                    else if (message == "/q")
                    {
                        break;
                    } else if (message.Substring(0, 3) == "/cl")
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
                    } else if (message.Substring(0, 6) == "/write")
                    {
                        if (message.Split(" ")[1] == "0")
                            _field1.WriteMatrix(-1, -1);
                        else if (message.Split(" ")[1] == "1")
                            _field2.WriteMatrix(-1, -1);
                    } else if (message.Substring(0, 5) == "/tell")
                    {
                        TcpClient currentclient = _clientsList[int.Parse(message.Split(" ")[1])];
                        string tell = message.Split("\'")[1];
                        Responce(currentclient, tell);

                    }
                } else if (message[0] == '!')
                {
                    if (message.Substring(0, 7) == "!Shoot!")
                    {
                        int shootY = int.Parse(message.Split(" ")[1]);
                        int shootX = int.Parse(message.Split(" ")[2]);
                        ShootCheck(shootY, shootX);
                    }
                    else if (message == "!ReadyFoPlay!")
                    {
                        _readyCount++;
                        ServerWrite($"Players ready: {_readyCount}");
                        if (_readyCount == 2)
                        {
                            Thread.Sleep(1000);
                            foreach (var player in _clientsList)
                            {
                                Random random = new Random();
                                _turn = random.Next(2);
                                Responce(player, "!GameStarted!");
                            }
                            ServerWrite("?????????????????????????????????????");
                            ServerWrite(message);
                            Thread.Sleep(50);
                            DeclareTurn();
                            
                        }
                    }
                    ServerWrite($"Iterations: {iteration}");
                    iteration++;
                }
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
            if (_clientsList.Count == 0)
            {
                ServerWrite($"Server is closed.");
                Stop();
            }
        }

        void ShootCheck(int y, int x)
        {   
            Matrix field = _turn == 0 ? _field2 : _field1;
            int cacheTurn = _turn;
            ServerWrite($"Shoot Check [{y}, {x}]");
            string reply;
            if (field.Mtrx[y, x] == 1)
            {
                reply = "Hit";
                field.Mtrx[y, x] = 4;
                ServerWrite(KillShipCheck(field, y, x).ToString());
                if (KillShipCheck(field, y, x))
                {
                    field.Mtrx = ReWriteForKilled(field, y, x);
                    if (cacheTurn == 0)
                        _shipsAlive2--;
                    else
                        _shipsAlive1--;
                    
                    reply = "Kill";
                }
            }
            else
            {
                reply = "Miss";
                field.Mtrx[y, x] = 6;
                _turn = Math.Abs(_turn - 1);
            }
            if (cacheTurn == 0)
                _field2.Mtrx = field.Mtrx;
            else
                _field1.Mtrx = field.Mtrx;
            Responce(_clientsList[cacheTurn], reply);
            DeclareTurn();

        }

        bool KillShipCheck(Matrix field, int y, int x)
        {
            bool isKilled = true;
            if (y>0 && field.Mtrx[y-1, x] == 1)
                isKilled = false;
            else if (x>0 && field.Mtrx[y, x-1] == 1)
                isKilled = false;
            else if (y<9 && field.Mtrx[y+1, x] == 1)
                isKilled = false;
            else if (x<9 && field.Mtrx[y, x+1] == 1)
                isKilled = false;
            return isKilled;
        }

        int[,] ReWriteForKilled(Matrix field, int y, int x)
        {
            field.Mtrx[y, x] = 3;
            while (true)
            {
                if (y > 0 && field.Mtrx[y - 1, x] == 4)
                {
                    field.Mtrx[y - 1, x] = 3;
                    y--;
                    continue;
                }
                if (x > 0 && field.Mtrx[y, x - 1] == 4)
                {
                    field.Mtrx[y, x - 1] = 3;
                    x--;
                    continue;
                }
                if (y < 9 && field.Mtrx[y + 1, x] == 4)
                {
                    field.Mtrx[y + 1, x] = 3;
                    y++;
                    continue;
                }
                if (x < 9 && field.Mtrx[y, x + 1] == 4)
                {
                    field.Mtrx[y, x + 1] = 3;
                    x++;
                    continue;
                }
                break;
            }

            return field.Mtrx;
        }

        void DeclareTurn()
        {
            string returnField1 = JsonConvert.SerializeObject(_turn == 0 ? _field1.Mtrx : _field2.Mtrx);
            string returnField2 = JsonConvert.SerializeObject(_turn == 0 ? _field2.Mtrx : _field1.Mtrx);
            Responce(_clientsList[_turn], $"!TurnYour! {returnField1}");
            Responce(_clientsList[Math.Abs(_turn-1)], $"!TurnOpponent! {returnField2}");
        }
        void Responce(TcpClient client, string response)
        {
            NetworkStream newStream = client.GetStream();
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            newStream.Write(responseData, 0, responseData.Length);
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