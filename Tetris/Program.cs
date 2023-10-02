using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

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
        private static int HEIGHT = 30;

        static void Main(string[] args)
        {
            //SetWindowSize(32, 33);
            ShowWindow(ThisConsole, MAXIMIZE);


            Timer timer = new Timer(TickFunction, null, 0, 1000);
            Thread inputThread = new Thread(InputHandler);
            inputThread.Start();
        }

        static void TickFunction(object state)
        {
            SetCursorPosition(0, 0);
            printBoard();
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
                        // Handle up arrow key press here
                        break;

                    case ConsoleKey.DownArrow:
                        Console.WriteLine("Down Arrow Pressed");
                        // Handle down arrow key press here
                        break;

                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("Left Arrow Pressed");
                        // Handle left arrow key press here
                        break;

                    case ConsoleKey.RightArrow:
                        Console.WriteLine("Right Arrow Pressed");
                        // Handle right arrow key press here
                        break;

                    case ConsoleKey.Escape:
                        Console.WriteLine("ESC Key Pressed. Exiting...");
                        Environment.Exit(0); // Exit the program
                        break;
                }
            }
        }

        static void printBoard()
        {
            Console.Write("+");
            for (int j = 0; j < HEIGHT; j++)
            {
                Console.Write("-");
            }
            Console.Write("+\n");

            for (int i = 0; i < HEIGHT; i++)
            {
                Console.Write("|");
                for (int j = 0; j < WIDTH; j++)
                {
                    // instead of this write [] if block is ocupied
                    Console.Write("  ");
                }
                Console.Write("|\n");
            }

            Console.Write("+");
            for (int j = 0; j < HEIGHT; j++)
            {
                Console.Write("-");
            }
            Console.Write("+\n");
        }

        class tile
        {

        }
    }
}
