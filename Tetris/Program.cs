using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// make functions more pure
// CheckForLine has a bad name and should modify the MATRIX variable directly
// MoveBoard should create the next board and assign it to the main board
// updateMatrix shouldnt access currentObjectPositions directly and we should extract the collision check outside of it

namespace Tetris
{
    internal class Program
    {
        private static bool[,] IShape = new bool[1, 4] { { true, true, true, true } };
        private static bool[,] OShape = new bool[2, 2] { { true, true }, { true, true } };
        private static bool[,] ZShape = new bool[2, 3] { { false, true, true }, { true, true, false } };
        private static bool[,] LShape = new bool[2, 3] { { true, false, false }, { true, true, true } };
        private static bool[,] TShape = new bool[2, 3] { { false, true, false }, { true, true, true } };
        private static List<bool[,]> ShapeTypes = new List<bool[,]>() { IShape, OShape, ZShape, LShape, TShape };

        private static int WIDTH = 16;
        private static int HEIGHT = 26;
        private static int timerInterval = 500;
        private static Timer timer;

        private static volatile TickState tickState;
        public class TickState
        {
            public bool[,] Matrix { get; set; }
            public List<position> CurrentObjectPositions { get; set; }
        }
        public class position
        {
            public position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x { get; set; }
            public int y { get; set; }
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(WIDTH * 2 + 3, HEIGHT);
            Console.SetBufferSize(WIDTH * 2 + 3, HEIGHT);
            Console.CursorVisible = false;

            tickState = new TickState
            {
                Matrix = new bool[HEIGHT + 4, WIDTH],
                CurrentObjectPositions = getRandomObject(ShapeTypes)
            };

            timer = new Timer(TickFunction, null, 0, timerInterval);
            Thread inputThread = new Thread(InputHandler);
            inputThread.Start();
        }

        static void TickFunction(object state)
        {
            TickState tickState = Program.tickState;
            bool[,] MATRIX = tickState.Matrix;
            List<position> currentObjectPositions = tickState.CurrentObjectPositions;

            UpdateMatrix(ref currentObjectPositions, MATRIX);
            PrintBoard(MATRIX, currentObjectPositions);
        }

        static List<position> getRandomObject(List<bool[,]> ShapeTypes)
        {
            bool[,] randomShape;
            Random random = new Random();
            randomShape = ShapeTypes[random.Next(ShapeTypes.Count)];
            List<position> objectPositions = new List<position>();

            for (int i = 0; i < randomShape.GetLength(0); i++)
            {
                for (int j = 0; j < randomShape.GetLength(1); j++)
                {
                    if (!randomShape[i, j]) continue;
                    objectPositions.Add(new position(WIDTH / 2 + j, i + 1));
                }
            }
            return objectPositions;
        }

        static void UpdateMatrix(ref List<position> currentObjectPositions, bool[,] MATRIX)
        {
            if (currentObjectPositions.Any(pos => pos.y >= HEIGHT - 1 || MATRIX[pos.y + 1, pos.x]))
                HandleCollision(currentObjectPositions, MATRIX);

            currentObjectPositions.ForEach(pos => pos.y += 1);
        }

        static void HandleCollision(List<position> currentObjectPositions, bool[,] MATRIX)
        {
            currentObjectPositions.ForEach(pos => MATRIX[pos.y, pos.x] = true);
            currentObjectPositions.Clear();
            List<int> linesToRemove = CheckForLine(MATRIX);
            MoveBoard(MATRIX, linesToRemove);
            currentObjectPositions.AddRange(getRandomObject(ShapeTypes));
            timer.Change(0, timerInterval);
        }

        static List<int> CheckForLine(bool[,] MATRIX)
        {
            List<int> linesToRemove = new List<int>();
            for (int i = 0; i <= HEIGHT; i++)
            {
                if (Enumerable.Range(0, WIDTH).All(j => MATRIX[i, j]))
                {
                    linesToRemove.Add(i);
                    for (var j = 0; j < WIDTH; j++)
                    {
                        MATRIX[i, j] = false;
                    }
                }
            }
            return linesToRemove;
        }

        static void MoveBoard(bool[,] MATRIX, List<int> linesToRemove)
        {
            linesToRemove.ForEach(y =>
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

        static void PrintBoard(bool[,] MATRIX, List<position> currentObjectPositions)
        {
            Console.SetCursorPosition(0, 0);

            StringBuilder sb = new StringBuilder();

            sb.Append("+");
            sb.Append(new string('-', WIDTH * 2));
            sb.AppendLine("+");

            for (int i = 4; i < HEIGHT; i++)
            {
                sb.Append("|");

                for (int j = 0; j < WIDTH; j++)
                {
                    sb.Append(Occupied(MATRIX, currentObjectPositions, j, i) ? "[]" : "  ");
                }

                sb.AppendLine("|");
            }

            sb.Append("+");
            sb.Append(new string('-', WIDTH * 2));
            sb.AppendLine("+");

            FastConsole.WriteLine(sb.ToString());
            FastConsole.Flush();
        }

        static bool Occupied(bool[,] MATRIX, List<position> currentObjectPositions, int x, int y)
        {
            return currentObjectPositions.Any(pos => pos.x == x && pos.y == y) || MATRIX[y, x];
        }

        static void InputHandler( )
        {
            TickState tickState = Program.tickState;

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        tickState.CurrentObjectPositions = RotateShape(tickState.CurrentObjectPositions);
                        PrintBoard(tickState.Matrix, tickState.CurrentObjectPositions);
                        break;

                    case ConsoleKey.DownArrow:
                        TickFunction(tickState);
                        break;

                    case ConsoleKey.LeftArrow:
                        tickState.CurrentObjectPositions = moveXAxis(tickState.Matrix, tickState.CurrentObjectPositions, - 1);
                        PrintBoard(tickState.Matrix, tickState.CurrentObjectPositions);
                        break;

                    case ConsoleKey.RightArrow:
                        tickState.CurrentObjectPositions = moveXAxis(tickState.Matrix, tickState.CurrentObjectPositions, 1);
                        PrintBoard(tickState.Matrix, tickState.CurrentObjectPositions);
                        break;

                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }
        }
        static List<position> moveXAxis(bool[,] MATRIX, List<position> positions, int n)
        {
            if (positions.All(pos => pos.x + n >= 0 && pos.x + n <= WIDTH - 1 && !MATRIX[pos.y, pos.x + n]))
                positions.ForEach(pos => pos.x += n);
            return positions;
        }

        static List<position> RotateShape(List<position> positions)
        {
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
