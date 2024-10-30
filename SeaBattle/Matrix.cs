namespace SeaBattle;

public class Matrix
{
    public int Cols;
    public int Rows;
    public int[,] Mtrx;
    
    
    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Cols = columns;
        Mtrx = new int[Rows, Cols];
    }

    public void WriteMatrix(int x, int y)
    {
        // PrintColoredTextBG("-  ", ConsoleColor.Black, ConsoleColor.White);
        // Console.ForegroundColor = ConsoleColor.Gray;
        // for (int i = 0; i < Cols; i++)
        // {
        //     PrintColoredTextBG($" {i} ", ConsoleColor.Black, ConsoleColor.White);;
        // }
        // Console.Write("\n");
        for (int i = 0; i < Rows; i++)
        {
            // PrintColoredTextBG($"{i}  ", ConsoleColor.Black, ConsoleColor.White);
            for (int j = 0; j < Cols; j++)
            {
                int element = Mtrx[i, j];
                string symbol = " ";
                switch (element)
                {
                    case 0:
                        symbol = "\u2219";
                        break;
                    case 5:
                    case 1:
                        symbol = "\u25a0";
                        break;
                    case 6:
                    case 2:
                        symbol = "\u2219";
                        break;
                    case 3:
                        symbol = "\u2715";
                        break;
                    case 4:
                        symbol = "\u25a1";
                        break;
                }
                
                // if ((j + i) % 2 == 0)
                // {
                //     Console.BackgroundColor = ConsoleColor.DarkGray;
                // }
                // else
                // {
                //     Console.BackgroundColor = ConsoleColor.Black;
                // }
                if (i == y && j == x)
                {
                    PrintColored($" {element} ", ConsoleColor.Yellow);
                }
                else
                    Console.Write($" {element} ");
            }
            Console.WriteLine();
        }
        Console.BackgroundColor = ConsoleColor.Black;
    }

    public int[,] MergeMatrix(Matrix mat1, Matrix mat2, int cols, int rows)
    {
        int[,] result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int element1 = mat1.Mtrx[i, j];
                int element2 = mat2.Mtrx[i, j];
                if (element2 == 0)
                {
                    result[i, j] = element1;
                }
                else
                {
                    if (element1 == 0)
                    {
                        result[i, j] = element2;
                    }
                    else
                    {
                        result[i, j] = -element2;
                    }
                }
            }
        }
        return result;
    }
    static void PrintColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    static void PrintColoredBG(string text, ConsoleColor color)
    {
        Console.BackgroundColor = color;
        Console.Write(text);
        Console.BackgroundColor = ConsoleColor.Gray;
    }
    static void PrintColoredTextBG(string text, ConsoleColor colorFG, ConsoleColor colorBG)
    {
        Console.ForegroundColor = colorFG;
        Console.BackgroundColor = colorBG;
        Console.Write(text);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.BackgroundColor = ConsoleColor.Black;
    }
}