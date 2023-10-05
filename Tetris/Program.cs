using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
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
        private static int WIDTH = 16;
        private static int HEIGHT = 26;
        private static bool[,] MATRIX = new bool[HEIGHT + 4, WIDTH];
        private static bool[,] IShape = new bool[1, 4] { { true, true, true, true } };

        private static bool[,] OShape = new bool[2, 2] { { true, true },
                                                       { true, true } };

        private static bool[,] ZShape = new bool[2, 3] { { false, true, true },
                                                       { true, true, false} };

        private static bool[,] LShape = new bool[2, 3] { { true, false, false },
                                                       { true, true, true} };
        private static Timer timer;
        private static List<position> currentObjectPositions = new List<position>();
        private static int timerInterval = 250;
        static void Main(string[] args)
        {
            //ShowWindow(ThisConsole, MAXIMIZE);
            Console.SetWindowSize(WIDTH * 2 + 3, HEIGHT);
            Console.SetBufferSize(WIDTH * 2 + 3, HEIGHT);
            Console.CursorVisible = false;

            fillArray();
            addObject();
            timer = new Timer(TickFunction, null, 0, timerInterval);
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
            bool[,] randomShape;
            Random random = new Random();
            // Generate a random number to select a shape
            int shapeIndex = random.Next(4);
            switch (shapeIndex)
            {
                case 0:
                    randomShape = IShape;
                    break;
                case 1:
                    randomShape = OShape;
                    break;
                case 2:
                    randomShape = ZShape;
                    break;
                case 3:
                    randomShape = LShape;
                    break;
                default:
                    throw new InvalidOperationException("Invalid shape index.");
            }
            randomShape = random.Next(2) == 0 ? Transpose(randomShape) : randomShape;
            // Generate a random position within the top row
            int startPosition = random.Next(0, WIDTH - randomShape.GetLength(1) + 1);

            // Place the shape in the top row of the MATRIX array
            for (int i = 0; i < randomShape.GetLength(0); i++)
            {
                for (int j = 0; j < randomShape.GetLength(1); j++)
                {
                    if (!randomShape[i, j]) continue;
                    //MATRIX[i + 4, startPosition + j] = randomShape[i, j];
                    currentObjectPositions.Add(new position(startPosition + j, i + 1));               
                }
            }
        }



        private static bool[,] Transpose(bool[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            bool[,] transposedMatrix = new bool[cols, rows];

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
            if (currentObjectPositions.Where(k =>  k.y >= HEIGHT - 1 || MATRIX[k.y + 1, k.x] == true).Count() > 0) handleCollision();
            currentObjectPositions.ForEach(pos =>
            {
                pos.y += 1;
            });
        }

        static void handleCollision()
        {
            currentObjectPositions.ForEach(pos => MATRIX[pos.y, pos.x] = true);
            currentObjectPositions.Clear();
            checkForLine();
            addObject();
            timer.Change(0, timerInterval);
        }

        static void checkForLine()
        {
            for (int i = 0; i <= HEIGHT; i++)
            {
                int counter = 0;
                for (int j = 0; j < WIDTH; j++)
                {
                    counter += MATRIX[i, j] ? 1 : 0;
                }
                if (counter == WIDTH)
                {
                    for (var j = 0; j < WIDTH; j++)
                    {
                        MATRIX[i, j] = false;
                    }
                }
            }
        }

        static void printBoard()
        {
            SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            sb.Append("+");
            sb.Append(new string('-', WIDTH * 2));
            sb.AppendLine("+");

            for (int i = 4; i < HEIGHT; i++)
            {
                sb.Append("|");

                for (int j = 0; j < WIDTH; j++)
                {
                    sb.Append(ocupied(j, i) ? "[]" : "  ");
                }

                sb.AppendLine("|");
            }

            sb.Append("+");
            sb.Append(new string('-', WIDTH * 2));
            sb.AppendLine("+");
            FastConsole.WriteLine(sb.ToString());
            FastConsole.Flush();
        }

        static bool ocupied(int x, int y) { 
            return currentObjectPositions.Where(pos => pos.x == x && pos.y == y).Count() > 0 || MATRIX[y, x];
        }

        static void InputHandler()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Read a key without displaying it

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        currentObjectPositions = RotateShape(currentObjectPositions);
                        //currentObjectPositions = Transpose(currentObjectPositions)
                        break;

                    case ConsoleKey.DownArrow:
                        moveDown();
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
        static List<position> RotateShape(List<position> positions) {
            // Find the center (pivot point) of the shape
            int centerX = positions.Sum(p => p.x) / positions.Count;
            int centerY = positions.Sum(p => p.y) / positions.Count;

            // Rotate each block around the center by 90 degrees
            List<position> rotatedPositions = new List<position>();
            int bounds = 0;
            foreach (position pos in positions)
            {
                int relativeX = pos.x - centerX;
                int relativeY = pos.y - centerY;

                // Rotate the block by 90 degrees
                int rotatedX = -relativeY + centerX;
                int rotatedY = relativeX + centerY;

                rotatedPositions.Add(new position(rotatedX + 1, rotatedY));
            }

            return rotatedPositions;
        }
        static void moveDown() { timer.Change(0, timerInterval / 2); }

        static void moveRight()
        {
            if (currentObjectPositions.Where(pos => pos.x >= WIDTH - 1 || MATRIX[pos.y, pos.x + 1]).Count() == 0) currentObjectPositions.ForEach(pos => pos.x += 1);
        }
        static void moveLeft()
        {
            if (currentObjectPositions.Where(pos => pos.x == 0 || MATRIX[pos.y, pos.x - 1]).Count() == 0) currentObjectPositions.ForEach(pos => pos.x -= 1);
        }
    }
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

    public static class FastConsole
    {
        static readonly BufferedStream str;

        static FastConsole()
        {
            Console.OutputEncoding = Encoding.Unicode;  // crucial

            // avoid special "ShadowBuffer" for hard-coded size 0x14000 in 'BufferedStream' 
            str = new BufferedStream(Console.OpenStandardOutput(), 0x15000);
        }

        public static void WriteLine(String s) => Write(s + "\r\n");

        public static void Write(String s)
        {
            // avoid endless 'GetByteCount' dithering in 'Encoding.Unicode.GetBytes(s)'
            var rgb = new byte[s.Length << 1];
            Encoding.Unicode.GetBytes(s, 0, s.Length, rgb, 0);

            lock (str)   // (optional, can omit if appropriate)
                str.Write(rgb, 0, rgb.Length);
        }

        public static void Flush() { lock (str) str.Flush(); }
    };
}
