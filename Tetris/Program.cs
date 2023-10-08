using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using static System.Net.Mime.MediaTypeNames;

// reuse the rotateShape and randomly rotate the shape before placing an new object, 
// add remaning shapes, 
// add function to move down the upper boards when line breaks 
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

        private static bool[,] OShape = new bool[2, 2] { { true, true }, { true, true } };

        private static bool[,] ZShape = new bool[2, 3] { { false, true, true },{ true, true, false} };

        private static bool[,] LShape = new bool[2, 3] { { true, false, false }, { true, true, true} };
        private static bool[,] TShape = new bool[2, 3] { { false, true, false }, { true, true, true} };

        private static List<bool[,]> ShapeTypes = new List<bool[,]>() { IShape, OShape, ZShape, LShape, TShape };

        private static Timer timer;
        private static List<position> currentObjectPositions = new List<position>();
        private static int timerInterval = 500;
        static void Main(string[] args)
        {
            //ShowWindow(ThisConsole, MAXIMIZE);
            Console.SetWindowSize(WIDTH * 2 + 3, HEIGHT);
            Console.SetBufferSize(WIDTH * 2 + 3, HEIGHT);
            Console.CursorVisible = false;

            for (int i = 0; i < WIDTH - 1; i++)
            {
                MATRIX[HEIGHT - 1, i] = true;
            }

            addObject();
            timer = new Timer(TickFunction, null, 0, timerInterval);
            Thread inputThread = new Thread(InputHandler);
            inputThread.Start();
        }

        static void addObject()
        {
            bool[,] randomShape;
            Random random = new Random();
            randomShape = ShapeTypes[random.Next(ShapeTypes.Count)];

            for (int i = 0; i < randomShape.GetLength(0); i++)
            {
                for (int j = 0; j < randomShape.GetLength(1); j++)
                {
                    if (!randomShape[i, j]) continue;
                    currentObjectPositions.Add(new position(WIDTH/2 + j, i + 1));               
                }
            }
        }

        static void TickFunction(object state)
        {
            updateMatrix();
            printBoard();
        }

        static void updateMatrix()
        {
            if (currentObjectPositions.Any(pos => pos.y >= HEIGHT - 1 || MATRIX[pos.y + 1, pos.x]))
                handleCollision();

            currentObjectPositions.ForEach(pos => pos.y += 1);
        }

        static void handleCollision()
        {
            currentObjectPositions.ForEach(pos => MATRIX[pos.y, pos.x] = true);
            currentObjectPositions.Clear();
            moveBoard(checkForLine());
            addObject();
            timer.Change(0, timerInterval);
        }

        static List<int> checkForLine()
        {
            List<int> Lines = new List<int>();
            for (int i = 0; i <= HEIGHT; i++)
            {
                if (Enumerable.Range(0, WIDTH).All(j => MATRIX[i, j]))
                {
                    Lines.Add(i);
                    for (var j = 0; j < WIDTH; j++)
                    {
                        MATRIX[i, j] = false;
                    }
                }
            }
            return Lines;
        }

        static void moveBoard(List<int> yAxis)
        {
            yAxis.ForEach(y =>
            {
                for (int i = y; i > 0; i--)
                {
                    for (var j = 0; j < WIDTH; j++)
                    {
                        MATRIX[i, j] = MATRIX[i - 1, j];
                    }
                }
            });
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
                    sb.Append(occupied(j, i) ? "[]" : "  ");
                }

                sb.AppendLine("|");
            }

            sb.Append("+");
            sb.Append(new string('-', WIDTH * 2));
            sb.AppendLine("+");

            FastConsole.WriteLine(sb.ToString());
            FastConsole.Flush();
        }
        static bool occupied(int x, int y)
        {
            return currentObjectPositions.Any(pos => pos.x == x && pos.y == y) || MATRIX[y, x];
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
                        printBoard();
                        break;

                    case ConsoleKey.DownArrow:
                        TickFunction(null);
                        break;

                    case ConsoleKey.LeftArrow:
                        moveXAxis(-1);
                        printBoard();
                        break;

                    case ConsoleKey.RightArrow:
                        moveXAxis(1);
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

        static void moveXAxis(int n)
        {
            if (currentObjectPositions.All(pos => pos.x + n >= 0 && pos.x + n <= WIDTH - 1 && !MATRIX[pos.y, pos.x + n])) 
                currentObjectPositions.ForEach(pos => pos.x += n);
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
