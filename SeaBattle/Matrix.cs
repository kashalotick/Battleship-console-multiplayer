using Newtonsoft.Json;

namespace SeaBattle;

public class Matrix
{
    public readonly int Cols;
    public readonly int Rows;
    public int[,] Mtrx;
    public bool AllowToPlace;
    
    
    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Cols = columns;
        Mtrx = new int[Rows, Cols];
    }

    public void WriteMatrix(int x, int y)
    {
        // ▗▄▖
        // ▐ ▌
        // ▝▀▘

        AllowToPlace = true;
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.WriteLine("    0  1  2  3  4  5  6  7  8  9");
        Console.WriteLine("  ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄");
        for (int i = 0; i < Rows; i++)
        {
            Console.Write($"{i} █");
            for (int j = 0; j < Cols; j++)
            {
                int element = Mtrx[i, j];
                string symbol = " ";
                
                // 0  1  2  3  4  7
                //    ■  ∙  ✕  □  ◆ 
                switch (element)
                {
                    case 0:
                        symbol = " ";
                        break;
                    case 1:
                    case 5:
                    case 15:
                    case 12:
                    case 21:
                        symbol = "\u25a0";
                        break;
                    case 2:
                    case 6:
                    case 16:
                    case 26:
                        symbol = "\u2219";
                        break;
                    case 3:
                    case 13:
                    case 23:
                        symbol = "\u2715";
                        break;
                    case 4:
                    case 14:
                    case 24:
                        symbol = "\u25a1";
                        break;
                    case 7:
                    case 17 :
                        symbol = "\u25c6";
                        break;
                }
                // Coloring
                if (element == 13 || element == 14 || element == 16)
                {
                    PrintColored($" {symbol} ", ConsoleColor.Magenta);
                } else if (element > 20)
                {
                    PrintColored($" {symbol} ", ConsoleColor.DarkGray);
                } else if (element > 10 || element == 3 || element == 4)
                {
                    PrintColored($" {symbol} ", ConsoleColor.Red);
                    AllowToPlace = false;
                }
                else if (i == y && j == x || element == 5 || element == 2 || element == 7)
                {
                    PrintColored($" {symbol} ", ConsoleColor.Yellow);
                }
                else
                {
                    Console.Write($" {symbol} ");
                }
            }
            Console.Write("█");
            Console.WriteLine();
        }
        Console.WriteLine("  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀");
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
                    } else if (element2 == 7)
                    {
                        result[i, j] = element1 + 10;
                    }
                    else
                    {
                        result[i, j] = element2 + 10;
                    }
                }
            }
        }
        return result;
    }

    public int[,] CollapseMatrix(Matrix mat1, Matrix mat2, int cols, int rows)
    {
        int[,] result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int element1 = mat1.Mtrx[i, j];
                int element2 = mat2.Mtrx[i, j];
                if (element1 != element2)
                {
                    result[i, j] = element1 + 20;
                }
                else
                {
                    result[i, j] = element2;
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
}