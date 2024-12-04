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
    private readonly string _version = "1.0.2";
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
    private int _winner = -1;
    public Server(int port)
    {
        _port = port;
        _server = new TcpListener(IPAddress.Any, _port);
        
        ServerWrite(findIPv4() + ":" + port);
        ServerWrite("-----------------------------");
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            ServerWrite(ip.ToString());
        }
        ServerWrite("-----------------------------");
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
                // ServerWrite($"Got message from `{sender}`: {message}");
                if (message[0] == '[' && message[1] == '[')
                {
                    // ServerWrite($"Got completed field from {sender}");
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
                        IPEndPoint remoteEndPoint = currentclient.Client.RemoteEndPoint as IPEndPoint;
                        IPEndPoint localEndPoint = currentclient.Client.LocalEndPoint as IPEndPoint;
                        ServerWrite($"--------------------------");
                        ServerWrite($"Remote client IP address: {remoteEndPoint.Address}");
                        ServerWrite($"Remote client port: {remoteEndPoint.Port}");
                        ServerWrite($"Local client IP address: {localEndPoint.Address}");
                        ServerWrite($"Local client IP port: {localEndPoint.Port}");
                        ServerWrite($"--------------------------");
                    } else if (message.Substring(0, 6) == "/write")
                    {
                        if (message.Split(" ")[1] == "0")
                            _field1.WriteMatrix(-1, -1);
                        else if (message.Split(" ")[1] == "1")
                            _field2.WriteMatrix(-1, -1);
                        if (message.Split(" ")[1] == "0e")
                            _enemyField1.WriteMatrix(-1, -1);
                        else if (message.Split(" ")[1] == "1e")
                            _enemyField2.WriteMatrix(-1, -1);
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

                    } else if (message == "!ReadyFoPlay!")
                    {
                        _readyCount++;
                        // ServerWrite($"Players ready: {_readyCount}");
                        if (_readyCount == 2)
                        {
                            Thread.Sleep(1000);
                            foreach (var player in _clientsList)
                            {
                                _turn = new Random().Next(2);
                                Responce(player, "!GameStarted!");
                            }
                            ServerWrite(message);
                            Thread.Sleep(50);
                            DeclareTurn();
                            
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ServerWrite($"Error: {ex.Message}");
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

        bool ShootCheck(int y, int x)
        {   
            bool endGame = false;
            Matrix field = _turn == 0 ? _field2 : _field1;
            // ServerWrite($"Ship [{y}, {x}] Edge [{FindShipEdge(field, y, x).y}, {FindShipEdge(field, y, x).x}]");
            Matrix enemyField = _turn == 0 ? _enemyField1 : _enemyField2;
            int cacheTurn = _turn;
            ServerWrite($"Shoot Check [{y}, {x}]");
            string reply;
            if (field.Mtrx[y, x] == 1)
            {
                reply = "Hit";
                field.Mtrx[y, x] = 4;
                enemyField.Mtrx[y, x] = 4;
                // ServerWrite(KillShipCheck(field, y, x).ToString());
                if (KillShipCheck(field, y, x))
                {
                    field.Mtrx = ReWriteForKilled(field, y, x);
                    enemyField.Mtrx = ReWriteForKilled(enemyField, y, x);
                    if (cacheTurn == 0)
                        _shipsAlive2--;
                    else
                        _shipsAlive1--;
                    reply = "Kill";
                    ServerWrite($"Alive 0: {_shipsAlive1}, Alive 1: {_shipsAlive2}");
                    if (WinCheck())
                    {
                        ServerWrite($"End of game");
                        reply = "Winner";
                        endGame = true;
                    }
                }
            }
            else
            {
                reply = "Miss";
                field.Mtrx[y, x] = 6;
                enemyField.Mtrx[y, x] = 6;
                _turn = Math.Abs(_turn - 1);
            }

            if (cacheTurn == 0)
            {
                _field2.Mtrx = field.Mtrx;
                _enemyField1.Mtrx = enemyField.Mtrx;
            }
            else
            {
                _field1.Mtrx = field.Mtrx;
                _enemyField2.Mtrx = enemyField.Mtrx;
            }
            // ServerWrite($"{cacheTurn}: {reply}");
            Responce(_clientsList[cacheTurn], reply);
            if (!endGame)
                DeclareTurn();
            else
            {
                DeclareWinner();
            }
            
            return endGame;
        }

        bool WinCheck()
        {
            bool win = false;
            if (_shipsAlive1 == 0)
            {
                _winner = 1;
                win = true;
            } else if (_shipsAlive2 == 0)
            {
                _winner = 0;
                win = true;
            }
            //_winner
            return win;
        }
        
        bool KillShipCheck(Matrix field, int y, int x)
        {
            y =  FindShipEdge(field, y, x).y;
            x =  FindShipEdge(field, y, x).x;
            string direction = "none";
            bool isKilled = true;
            int iteration = 0;
            while (true)
            {
                //ServerWrite($"Iteration: {iteration}, direction: {direction}");

                // ServerWrite($"Direction: {direction}");
                if (y>0 && field.Mtrx[y-1, x] == 1)
                    isKilled = false;
                else if (x>0 && field.Mtrx[y, x-1] == 1)
                    isKilled = false;
                else if (y<9 && field.Mtrx[y+1, x] == 1)
                    isKilled = false;
                else if (x<9 && field.Mtrx[y, x+1] == 1)
                    isKilled = false;
                else
                {
                    if (y > 0 && field.Mtrx[y - 1, x] == 4 )
                    {
                        
                        if (direction == "up" || direction == "none")
                        {
                            y--;
                            direction = "up";
                            continue;
                        }
                    }
                    if (x > 0 && field.Mtrx[y, x - 1] == 4)
                    {
                        
                        if (direction == "left" || direction == "none")
                        {
                            x--;
                            direction = "left";
                            continue;
                        }
                    }
                    if (y < 9 && field.Mtrx[y + 1, x] == 4)
                    {
                        if (direction == "down" || direction == "none")
                        {
                            y++;
                            direction = "down";
                            continue;
                        }
                    }
                    if (x < 9 && field.Mtrx[y, x + 1] == 4)
                    {
                        if (direction == "right" || direction == "none")
                        {
                            x++;
                            direction = "right";
                            continue;
                        }
                    }
                    // if (iteration < 2)
                    // {
                    //     switch (direction)
                    //     {
                    //         case "up":
                    //             direction = "down";
                    //             break;
                    //         case "right":
                    //             direction = "left";
                    //             break;
                    //         case "down":
                    //             direction = "up";
                    //             break;
                    //         case "left":
                    //             direction = "right";
                    //             break;
                    //         default:
                    //             ServerWrite("Error???????????");
                    //             break;
                    //     }                        
                    //     iteration++;
                    //     continue;
                    // }
                    isKilled = true;
                }
                break;
            }
            return isKilled;
        }

        int[,] ReWriteForKilled(Matrix field, int y, int x)
        {
            y =  FindShipEdge(field, y, x).y;
            x =  FindShipEdge(field, y, x).x;
            field.Mtrx[y, x] = 3;
            field.Mtrx = FillAroundShip(field, y, x);
            while (true)
            {
                if (y > 0 && field.Mtrx[y - 1, x] == 4)
                {
                    field.Mtrx[y - 1, x] = 3;
                    y--;
                    field.Mtrx = FillAroundShip(field, y, x);
                    continue;
                }
                if (x > 0 && field.Mtrx[y, x - 1] == 4)
                {
                    field.Mtrx[y, x - 1] = 3;
                    x--;
                    field.Mtrx = FillAroundShip(field, y, x);
                    continue;
                }
                if (y < 9 && field.Mtrx[y + 1, x] == 4)
                {
                    field.Mtrx[y + 1, x] = 3;
                    y++;
                    field.Mtrx = FillAroundShip(field, y, x);
                    continue;
                }
                if (x < 9 && field.Mtrx[y, x + 1] == 4)
                {
                    field.Mtrx[y, x + 1] = 3;
                    x++;
                    field.Mtrx = FillAroundShip(field, y, x);
                    continue;
                }
                break;
            }

            return field.Mtrx;
        }

        (int y, int x) FindShipEdge(Matrix field, int y, int x)
        {
            while (true)
            {
                if (y > 0 && (field.Mtrx[y - 1, x] == 1 || field.Mtrx[y - 1, x] == 4))
                {
                    y--;
                } else if (x > 0 && (field.Mtrx[y, x - 1] == 1 || field.Mtrx[y, x - 1] == 4))
                {
                    x--;
                }
                else
                    break;
            }
            return (y, x);
        }
        
        int[,] FillAroundShip(Matrix field, int y, int x)
        {
            bool top = (y > 0 && field.Mtrx[y - 1, x] == 0) || (y > 0 && field.Mtrx[y - 1, x] == 6);
            bool left = (x > 0 && field.Mtrx[y, x - 1] == 0) || (x > 0 && field.Mtrx[y, x - 1] == 6);
            bool right = (x < 9 && field.Mtrx[y, x + 1] == 0) || (x < 9 && field.Mtrx[y, x + 1] == 6);
            bool bottom = (y < 9 && field.Mtrx[y + 1, x] == 0) || (y < 9 && field.Mtrx[y + 1, x] == 6);
            // ServerWrite($"[{y}, {x}]: {top}, {right}, {bottom}, {left}");
            // main axis fill
            if (top)
                field.Mtrx[y-1, x] = 6;
            if (right)
                field.Mtrx[y, x + 1] = 6;
            if (bottom)
                field.Mtrx[y + 1, x] = 6;
            if (left)
                field.Mtrx[y, x - 1] = 6;
            // diagonal fill
            if (top && left)
                field.Mtrx[y-1, x-1] = 6;
            if (top && right)
                field.Mtrx[y-1, x+1] = 6;
            if (bottom && right)
                field.Mtrx[y+1, x+1] = 6;
            if (bottom && left)
                field.Mtrx[y+1, x-1] = 6;
            
            return field.Mtrx;
        }
        
        void DeclareTurn()
        {
            string returnField1 = JsonConvert.SerializeObject(_turn == 0 ? _field1.Mtrx : _field2.Mtrx);
            string returnEnemyfield1 = JsonConvert.SerializeObject(_turn == 0 ? _enemyField1.Mtrx : _enemyField2.Mtrx);
            string returnField2 = JsonConvert.SerializeObject(_turn == 0 ? _field2.Mtrx : _field1.Mtrx);
            string returnEnemyfield2 = JsonConvert.SerializeObject(_turn == 0 ? _enemyField2.Mtrx : _enemyField1.Mtrx);
            Responce(_clientsList[_turn], $"!TurnYour! {returnField1} {returnEnemyfield1}");
            Responce(_clientsList[Math.Abs(_turn-1)], $"!TurnOpponent! {returnField2} {returnEnemyfield2}");
        }

        void DeclareWinner()
        {
            string returnField1 = JsonConvert.SerializeObject(_winner == 0 ? _field1.Mtrx : _field2.Mtrx);
            string returnEnemyfield1 = JsonConvert.SerializeObject(_winner == 0 ? _enemyField1.Mtrx : _enemyField2.Mtrx);
            string returnField2 = JsonConvert.SerializeObject(_winner == 0 ? _field2.Mtrx : _field1.Mtrx);
            string returnEnemyfield2 = JsonConvert.SerializeObject(_winner == 0 ? _enemyField2.Mtrx : _enemyField1.Mtrx);
            Responce(_clientsList[_winner], $"!YouWin! {returnField1} {returnEnemyfield1} {returnField2}");
            Responce(_clientsList[Math.Abs(_winner-1)], $"!YouLost! {returnField2} {returnEnemyfield2} {returnField1}");
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