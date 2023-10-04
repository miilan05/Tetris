using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;
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
        class position
        {
            public position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x { get; set; }
            public int y { get; set; }
        }
        private static List<position> currentObjectPositions = new List<position>();

        static void Main(string[] args)
        {
            ShowWindow(ThisConsole, MAXIMIZE);
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
            currentObjectPositions = new List<position> {new position(0, 2), new position(1, 2) , new position(2, 2) , new position(3, 2) };
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
            updateMatrix();
            printBoard();
        }

        static void updateMatrix() {
            //if (currentObjectPositions.Where(k => MATRIX[k.y + 1, k.x] == true || k.y >= HEIGHT - 1).Count() > 0) currentObjectPositions.Clear();
            currentObjectPositions.ForEach(pos =>
            {
                //MATRIX[pos.y, pos.x] = false;
                //MATRIX[pos.y + 1, pos.x ] = true;
                pos.y += 1;
            });
        }

        static void printBoard()
        {
            SetCursorPosition(0, 0);
            printEdge();
            printGame();
            printEdge();
            printMatrix();
        }

        static void printMatrix()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                string s = "";
                for (int j = 0; j < WIDTH; j++)
                {
                    s += ocupied(j, i) ? "x" : " ";
                }
                Console.WriteLine(s);
            }
        }

        static void printEdge()
        {
            string s = "+" + string.Concat(System.Linq.Enumerable.Repeat("-", (int)WIDTH*2)) + "+";
            Console.WriteLine(s);
        }

        static void printGame()
        {
            for (int i = 4; i < HEIGHT; i++)
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
            return currentObjectPositions.Where(pos => pos.x == x && pos.y == y).Count() > 0;
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
                        moveLeft();
                        printBoard();
                        break;

                    case ConsoleKey.RightArrow:
                        moveRight();
                        printBoard();
                        break;

                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static void moveRight()
        {
            if (currentObjectPositions.Where(pos => pos.x >= WIDTH - 1).Count() == 0) currentObjectPositions.ForEach(pos => pos.x += 1);
        }
        static void moveLeft()
        {
            if (currentObjectPositions.Where(pos => pos.x == 0).Count() == 0) currentObjectPositions.ForEach(pos => pos.x -= 1);
        }
    }
}
