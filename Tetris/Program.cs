using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using static System.Net.Mime.MediaTypeNames;

namespace Tetris
{
    internal class Program
    {
        private static int WIDTH = 15;
        private static int HEIGHT = 26;
        private static bool[,] MATRIX = new bool[HEIGHT + 4, WIDTH];
        private static bool[,] IShape = new bool[1, 4] { { true, true, true, true } };

        private static bool[,] OShape = new bool[2, 2] { { true, true },
                                                       { true, true } };

        private static bool[,] ZShape = new bool[2, 3] { { false, true, true },
                                                       { true, true, false} };

        private static bool[,] LShape = new bool[2, 3] { { true, false, false },
                                                       { true, true, true} };
        static void Main(string[] args)
        {
            fillArray();
            addObject();
            Timer timer = new Timer(TickFunction, null, 0, 1000);
            Thread inputThread = new Thread(InputHandler);
            inputThread.Start();
        }
        static void fillArray()
        {
            for (int i = 0; i < HEIGHT + 4; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    MATRIX[i, j] = false;
                }
            }
        }

        static void addObject()
        {
            for (int j = 0; j < 4; j++)
            {
                MATRIX[2, j] = IShape[0, j];
            }
        }

        private static int[,] Transpose(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int[,] transposedMatrix = new int[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposedMatrix[j, i] = matrix[i, j];
                }
            }

            return transposedMatrix;
        }

        static void TickFunction(object state)
        {
            SetCursorPosition(0, 0);
            updateMatrix();
            printBoard();
        }

        static void updateMatrix()
        {
            for (int i = HEIGHT + 3; i > 1;  i--)
            {
                for (int j = WIDTH - 1; j > 0; j--)
                {
                    MATRIX[i, j] = MATRIX[i - 1, j];
                    
                }
            }
        }

        static void printBoard()
        {
            printEdge();
            printGame();
            printEdge();

        }

        static void printEdge()
        {
            string s = "+" + string.Concat(System.Linq.Enumerable.Repeat("-", (int)WIDTH*2)) + "+";
            Console.WriteLine(s);
        }

        static void printGame()
        {
            for (int i = 4; i < HEIGHT + 4; i++)
            {
                string s = "";
                for (int j = 0; j < WIDTH; j++)
                {
                    s += ocupied(j, i) ? "[]" : "  ";
                }
                Console.Write($"|{s}|\n");
            }
        }

        static bool ocupied(int x, int y) { 
            return MATRIX[y, x];
        }

        static void InputHandler()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Read a key without displaying it

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("Up Arrow Pressed");
                        break;

                    case ConsoleKey.DownArrow:
                        Console.WriteLine("Down Arrow Pressed");
                        break;

                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("Left Arrow Pressed");
                        break;

                    case ConsoleKey.RightArrow:
                        Console.WriteLine("Right Arrow Pressed");
                        break;

                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}
