using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace SeaBattle;

public class Client
{
    private readonly string _version = "1.0.0";
    private readonly TcpClient _client;
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly string _name;
    private bool _connected;
    private NetworkStream _stream;
    private string _recievedData;
    
    public Matrix Field = new Matrix(10, 10);
    public Matrix MergedField = new Matrix(10, 10);

    public int PosX;
    public int PosY;

    private Ship[][] _confirmedShips = [[null, null, null, null], [null, null, null], [null, null], [null]];
    private Ship[][] _shipCoordinates = [[null, null, null, null], [null, null, null], [null, null], [null]];
    private int[][] _ships= [[1, 1, 1, 1], [1, 1, 1], [1, 1], [1]];
    private int _shipsCount = 10;
    private (int type, int status) _currentShip = (0, 0);
    
    private bool _vertical;
    private bool _allowToPlace = true;
    private bool _fixSelection;
    private bool _allowToConfirm;
    
    private bool _ready;
    
    private bool _yourTurn;
    public Matrix EnemyField = new Matrix(10, 10);
    private Matrix _opponentEndField  = new Matrix(10, 10);
    private bool _youWinner = false;

    
    public Client(string ipAddress, string port, string name)
    {
        // Console.CursorVisible = false;
        _port = int.Parse(port);
        _ipAddress = ipAddress;
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
// ----------------------------------- Skip for test
        // if (_name == "HOST")
        // {
        //     Field.Mtrx = new int[10, 10]
        //     {
        //         { 0, 0, 0, 0, 0, 1, 0, 1, 0, 0 },
        //         { 0, 1, 0, 0, 0, 0, 0, 1, 0, 0 },
        //         { 0, 1, 0, 0, 0, 0, 0, 1, 0, 0 },
        //         { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        //         { 0, 1, 0, 1, 1, 1, 0, 0, 0, 1 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        //         { 0, 1, 1, 0, 0, 0, 0, 1, 0, 0 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        //         { 1, 0, 1, 0, 0, 1, 1, 0, 0, 0 }
        //     };
        // }
        // else
        // {
        //     Field.Mtrx = new int[10, 10]
        //     {
        //         { 1, 1, 1, 1, 0, 1, 0, 1, 0, 1 },
        //         { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
        //         { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        //         { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        //         { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //         { 1, 0, 1, 0, 0, 1, 1, 0, 0, 1 }
        //     };
        // }
        // _shipsCount = 0;
        // _allowToConfirm = true;
        // // if (_name != "HOST") 
        // //     Thread.Sleep(2000);
        // _ready = true;
 // ----------------------------------- Placing
        
        PlacingShips(data);
        string jsonMessage = JsonConvert.SerializeObject(Field.Mtrx);
        data = Encoding.UTF8.GetBytes(jsonMessage);
        _stream.Write(data, 0, data.Length);
        data = Encoding.UTF8.GetBytes($"!ReadyFoPlay!");
        _stream.Write(data, 0, data.Length);
        PrintColored("Waiting for opponent...", ConsoleColor.DarkGray);
        while (true)
        {
            if (_recievedData == "!GameStarted!")
            {
                break;
            }
        }
// ----------------------------------- Game
        Gameplay(data);
        
// ----------------------------------- End
        Console.Clear();
        Field.Mtrx = JsonConvert.DeserializeObject<int[,]>(_recievedData.Split(" ")[1]);
        EnemyField.Mtrx = JsonConvert.DeserializeObject<int[,]>(_recievedData.Split(" ")[2]);
        _opponentEndField.Mtrx = JsonConvert.DeserializeObject<int[,]>(_recievedData.Split(" ")[3]);
        
        
        Thread.Sleep(100);
        PrintColored("Your field", ConsoleColor.Blue);
        Field.WriteMatrix(-1, -1);
        PrintColored("Opponent`s field", ConsoleColor.Red);
        MergedField.Mtrx = MergedField.CollapseMatrix(_opponentEndField, EnemyField, 10, 10);
        MergedField.WriteMatrix(-1, -1);

        if (_youWinner)
            PrintColored("You win!", ConsoleColor.Green);
        else
            PrintColored("You lost!", ConsoleColor.Red);
        
        PrintColored("END GAME", ConsoleColor.DarkBlue);
        Console.ReadLine();
        while (true)
        {
            string input = Console.ReadLine();
            if (input == "/q")
                break;
            data = Encoding.UTF8.GetBytes(input);
            _stream.Write(data, 0, data.Length);
        }
        if (_connected)
        {
            _connected = false;
            data = Encoding.UTF8.GetBytes($"/q");
            _stream.Write(data, 0, data.Length);
            Thread.Sleep(250);
        }
    }

    private void Gameplay(byte[] data)
    {
        PosX = 0;
        PosY = 0;

        VisualiseShip(); 
        //Field.WriteMatrix(-1, -1);
        
        bool waitingMsg = false;
        int[,] cachedField = Field.Mtrx;
        int[,] cachedEnemyField = EnemyField.Mtrx;
        while (true)
        {
            if (_recievedData[0] == '!' && _recievedData.Split(" ")[0] == "!YouWin!")
            {
                _youWinner = true;
                break;
            }
            if (_recievedData[0] == '!' && _recievedData.Split(" ")[0] == "!YouLost!")
            {
                _youWinner = false;
                break;
            }
            
            if (cachedField != Field.Mtrx || cachedEnemyField != EnemyField.Mtrx)
            {
                
                cachedField = Field.Mtrx;
                cachedEnemyField = EnemyField.Mtrx;

                if (_recievedData[0] == '!' && _recievedData.Split(" ")[0] != "!TurnYour!")
                {
                    Console.Clear();

                    PrintColored("Your field", ConsoleColor.Blue);
                    Field.WriteMatrix(-1, -1);
                    PrintColored("Opponent`s field", ConsoleColor.Red);
                    EnemyField.WriteMatrix(-1, -1);
                }
                
                waitingMsg = false;
            }
            if (_recievedData[0] == '!' && _recievedData.Split(" ")[0] != "!TurnYour!")
            {
                _yourTurn = false;
                Thread.Sleep(200);
                if (!waitingMsg)
                {
                    PrintColored($"Opponent`s turn...", ConsoleColor.Red);
                    Thread.Sleep(200);
                    waitingMsg = true;
                }
                continue;
            }
            Console.Clear();

            while (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
            }
            
            _yourTurn = true;
            PrintColored("Your field", ConsoleColor.Blue);
            Field.WriteMatrix(-1, -1);
            PrintColored("Opponent`s field", ConsoleColor.Red);
            DisplaySelectionOnEnemyField();
            MergedField.WriteMatrix(PosX, PosY);
            PrintColored("Your turn...", ConsoleColor.Blue);

            waitingMsg = false;
            
            ConsoleKeyInfo inputKey = Console.ReadKey();
            string input;
            if (inputKey.KeyChar == '/')
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
                    case ConsoleKey.UpArrow: 
                        PosY--;
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        PosX--;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        PosY++;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        PosX++;
                        break;
                    case ConsoleKey.Enter:
                        if (EnemyField.Mtrx[PosY, PosX] == 0)
                        {
                            data = Encoding.UTF8.GetBytes($"!Shoot! {PosY} {PosX}");
                            _stream.Write(data, 0, data.Length);
                            Thread.Sleep(50);
                        }
                        break;
                }

                if (PosY > 9)
                    PosY--;
                if (PosY < 0)
                    PosY++;
                if (PosX > 9)
                    PosX--;
                if (PosX < 0)
                    PosX++;
            }
        }
    }
    
    private void PlacingShips(byte[] data)
    {
        PrintColored("Waiting for opponent...", ConsoleColor.DarkGray);
        while (true)
        {
            if (_recievedData != "!ReadyForPlacingShips!")
            {
                continue;
            }
            Console.Clear();
            if (_ready)
            {
                PrintColored("Ready", ConsoleColor.Green);
                break;
            }
            
            VisualiseShip(); 
            MergedField.WriteMatrix(PosX, PosY);
            
            if (MergedField.AllowToPlace)
                _allowToPlace = true;
            else
                _allowToPlace = false;
            if (_ships[_currentShip.type][_currentShip.status] == 0)
                _fixSelection = true;
            else 
                _fixSelection = false;
            
            DrawShips();
            Console.ForegroundColor = ConsoleColor.Gray;
            if (_shipsCount == 0)
            {
                if (_allowToConfirm)
                    PrintColored("Press C to confirm", ConsoleColor.Yellow);
                else
                    PrintColored("Press C to continue", ConsoleColor.Gray);
            }

            // Console.WriteLine($"{PosX}, {PosY}");
            // Console.WriteLine(_currentShip);
            // Console.WriteLine(_cursor);
            // Console.WriteLine(_vertical);

            // Console.WriteLine($"Count: {_shipsCount}");
            // PrintColored($"{_allowToPlace}", ConsoleColor.Magenta);
            // PrintColored($"{_fixSelection}", ConsoleColor.Magenta);
            // Console.WriteLine($"||{ship.type}, {ship.x}, {ship.y}, {ship.vertical}||");
           
            Ship ship = _shipCoordinates[_currentShip.type][_currentShip.status];
            ConsoleKeyInfo inputKey = Console.ReadKey();
            string input;
            
            // if (inputKey.KeyChar == 'e' || inputKey.KeyChar == 'у')
            // {
            //     // input = "";
            //     // data = Encoding.UTF8.GetBytes($"e: {PosX};{PosY}");
            //     // _stream.Write(data, 0, data.Length);
            // } else
            if (inputKey.KeyChar == '/')
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
                    case ConsoleKey.UpArrow: 
                        if (!_fixSelection)
                            PosY--;
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        if (!_fixSelection)
                            PosX--;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        if (!_fixSelection)
                            PosY++;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        if (!_fixSelection)
                            PosX++;
                        break;
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                    case ConsoleKey.D4:
                        ChooseShip(int.Parse(inputKey.KeyChar.ToString()));
                        break;
                    case ConsoleKey.R:
                        if (!_fixSelection)
                            _vertical = !_vertical;
                        break;
                    case ConsoleKey.C:
                        if (_shipsCount == 0)
                        {
                            if (!_allowToConfirm )
                                _allowToConfirm = true;
                            else if (_allowToConfirm)
                            {
                                if (!_ready)
                                {
                                    _ready = true;
                                    PrintColored("Confirmed. Ready", ConsoleColor.Green);
                                }
                                else
                                    PrintColored("Cannot Confirm", ConsoleColor.Red);
                            }
                        }
                        break;
                    case ConsoleKey.Escape:
                        if (_allowToConfirm && _shipsCount == 0)
                            _allowToConfirm = false;
                        break;
                    case ConsoleKey.Enter:
                        PlaceShip();
                        break;
                    case ConsoleKey.Backspace:
                        if (!_allowToConfirm)
                            DeleteShip();
                        else
                            _allowToConfirm = false;
                        break;
                }
            }

            if (_vertical)
            {
                if (PosY > 9 - _currentShip.type)
                    PosY = 9 - _currentShip.type;
                else if (PosX > 9)
                    PosX--;
            }
            else
            { 
                if (PosY > 9)
                    PosY--;
                else if (PosX > 9 - _currentShip.type)
                    PosX = 9 - _currentShip.type;
            }
            
            if (PosY < 0)
                PosY++;
            else if (PosX < 0)
                PosX++;
            
            Console.WriteLine(_recievedData);

        }
    }

    private void VisualiseShip()
    {
        Matrix Selection = new Matrix(10, 10);
        Ship shipData = new Ship(_currentShip.type, PosY, PosX, _vertical);
        
        Selection.Mtrx = ReWriteField(Selection, PosX, PosY, 5);
        _shipCoordinates[_currentShip.type][_currentShip.status] = shipData;
        MergedField.Mtrx = Selection.Mtrx;
        MergedField.Mtrx = Selection.MergeMatrix(Field, Selection, 10, 10);
    }
    private void DisplaySelectionOnEnemyField()
    {
        Matrix Selection = new Matrix(10, 10);
        Selection.Mtrx[PosY, PosX] = 7;
        MergedField.Mtrx = Selection.MergeMatrix(EnemyField, Selection, 10, 10);
    }

    private void PlaceShip()
    {
        int ifPlaced = _ships[_currentShip.type][_currentShip.status];
        Ship ship = _shipCoordinates[_currentShip.type][_currentShip.status];
        _confirmedShips[_currentShip.type][_currentShip.status] = ship;
        if (ifPlaced == 1 && _allowToPlace)
        {
            _shipsCount--;
            _ships[_currentShip.type][_currentShip.status] = 0;
            Field.Mtrx = ReWriteField(Field, ship.x, ship.y, 1);
            ChooseShip(_currentShip.type + 1);
        }
    }
    
    private void DeleteShip()
    {
        int ifPlaced = _ships[_currentShip.type][_currentShip.status];
        Ship ship = _shipCoordinates[_currentShip.type][_currentShip.status];
        _confirmedShips[_currentShip.type][_currentShip.status] = null;
        if (ifPlaced == 0)
        {
            _shipsCount++;
            _ships[_currentShip.type][_currentShip.status] = 1;
            Field.Mtrx = ReWriteField(Field, ship.x, ship.y, 0);
        }
    }
    
    private int[,] ReWriteField(Matrix field, int x, int y, int value)
    {
        if (value == 5)
        {
            field.Mtrx = CheckFieldArea(field, x, y);
        }
        else
        {
            if (_vertical)
                for (int i = y; i <= y + _currentShip.type; i++)
                    field.Mtrx[i, x] = value;
            else {
                for (int i = x; i <= x + _currentShip.type; i++)
                    field.Mtrx[y, i] = value;}
        }
        
        
        return field.Mtrx;
    }

    private int[,] CheckFieldArea(Matrix field, int x, int y)
    {
        if (_vertical)
        {
            for (int i = y; i <= y + _currentShip.type; i++)
            {
                if (x-1 >= 0)
                    field.Mtrx[i, x-1] = 2;
                if (x+1 <= 9)
                    field.Mtrx[i, x+1] = 2;
                field.Mtrx[i, x] = 5;
            }
            if (y-1 >= 0)
            {
                for (int i = x - 1; i <= x + 1 ; i++)
                {
                    if (i >= 0 && i <= 9)
                        field.Mtrx[y-1, i] = 2;
                }
            }
            if (y+1+_currentShip.type <= 9)
            {
                for (int i = x - 1; i <= x + 1; i++)
                {
                    if (i >= 0 && i <= 9)
                        field.Mtrx[y+1+_currentShip.type, i] = 2;
                }
            }

                
        }
        else
        {
            for (int i = x; i <= x + _currentShip.type; i++)
            {
                if (y-1 >= 0)
                    field.Mtrx[y-1, i] = 2;
                if (y+1 <= 9)
                    field.Mtrx[y+1, i] = 2;
                field.Mtrx[y, i] = 5;
            }
            if (x-1 >= 0)
            {
                for (int i = y - 1; i <= y + 1 ; i++)
                {
                    if (i >= 0 && i <= 9)
                        field.Mtrx[i, x-1] = 2;
                }
            }
            if (x+1+_currentShip.type <= 9)
            {
                for (int i = y - 1; i <= y + 1; i++)
                {
                    if (i >= 0 && i <= 9)
                        field.Mtrx[i, x+1+_currentShip.type] = 2;
                }
            }
        }
        return field.Mtrx;
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
        
        

        if (_ships[ship][t] == 0)
        {
            Ship selectedShip = _confirmedShips[_currentShip.type][_currentShip.status];
            PosX = selectedShip.x;
            PosY = selectedShip.y;
            _vertical = selectedShip.vertical;
        }
    }

    private void DrawShips()
    {
        int i = 0;
        foreach (int[] stack in _ships)
        {
            int j = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{i + 1}: ");
            foreach (int ship in stack)
            {
                if (_currentShip.type == i && _currentShip.status == j)
                {
                    if (_ships[i][j] == 1) 
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (_ships[i][j] == 0) 
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                else
                {
                    if (_ships[i][j] == 1) 
                        Console.ForegroundColor = ConsoleColor.Gray;
                    else if (_ships[i][j] == 0) 
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                }
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

    private void ReceiveMessages()
    {
        try
        {
            while (_connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0 && !_ready)
                    break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                // PrintColored(message, ConsoleColor.Cyan);

                if (message[0] == '!')
                {
                    if (message.Substring(0, 5) == "!Turn")
                    {
                        Field.Mtrx = JsonConvert.DeserializeObject<int[,]>(message.Split(" ")[1]);
                        EnemyField.Mtrx = JsonConvert.DeserializeObject<int[,]>(message.Split(" ")[2]);
                    } else if (message.Substring(0, 3) == "!You")
                    {
                        Field.Mtrx = JsonConvert.DeserializeObject<int[,]>(message.Split(" ")[1]);
                        _opponentEndField.Mtrx = JsonConvert.DeserializeObject<int[,]>(message.Split(" ")[2]);
                    }
                }
                _recievedData = message;
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