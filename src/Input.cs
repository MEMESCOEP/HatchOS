/* DIRECTIVES */
using System;
using Cosmos.System;
using System.Drawing;
using System.Collections.Generic;
using static HatchOS.HelperFunctions;
using static HatchOS.PowerFunctions;
using System.Threading;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    internal class Input
    {
        /* FUNCTIONS */
        // Handle keyboard / mouse input
        public static void HandleInput()
        {
            // Update the mouse values
            Kernel.MouseX = (Math.Clamp(MouseManager.X, 0, (uint)Kernel.ScreenWidth - Kernel.Mouse.Width));
            Kernel.MouseY = (Math.Clamp(MouseManager.Y, 0, (uint)Kernel.ScreenHeight - Kernel.Mouse.Height));

            // Test if the mouse has been moved
            //Kernel.MouseMoved = !(Kernel.OldMouseX == Kernel.MouseX && Kernel.OldMouseY == Kernel.MouseY);

            // Try to read a key from the keyboard
            KeyboardManager.TryReadKey(out var key);

            // If one of the Windows keys are pressed, toggle the menu
            if (key.Key == ConsoleKeyEx.LWin || key.Key == ConsoleKeyEx.RWin)
            {
                Kernel.ShowMenu = !Kernel.ShowMenu;
            }

            // If the F11 key is pressed, change the task bar color to a random ARGB color
            if (key.Key == ConsoleKeyEx.F11)
            {
                Kernel.TaskBarColor = Color.FromArgb(Kernel.random.Next(256), Kernel.random.Next(256), Kernel.random.Next(256));
            }

            // If the F12 key is pressed, toggle debug info
            if (key.Key == ConsoleKeyEx.F12)
            {
                Kernel.Debug = !Kernel.Debug;
            }

            // If the F4 key is pressed, panic
            if (key.Key == ConsoleKeyEx.F4)
            {
                KernelPanic.Panic("USER FORCED PANIC", "-99999");
            }

            // Mouse movement with arrow keys
            if (key.Key == ConsoleKeyEx.UpArrow)
            {
                MouseManager.Y -= 20;
            }

            if (key.Key == ConsoleKeyEx.DownArrow)
            {
                MouseManager.Y += 20;
            }

            if (key.Key == ConsoleKeyEx.LeftArrow)
            {
                MouseManager.X -= 20;
            }

            if (key.Key == ConsoleKeyEx.RightArrow)
            {
                MouseManager.X += 20;
            }

            // Simulate a mouse click when the F7 key is pressed
            if(key.Key== ConsoleKeyEx.F7)
            {
                MouseManager.HandleMouse((int)Kernel.MouseX * 0, (int)Kernel.MouseY * 0, 0x1, 0);
            }

            // Create a new debug window when the F8 key is pressed
            if (key.Key == ConsoleKeyEx.F8)
            {
                WindowManager.CreateNewWindow(Kernel.WindowList, new Point(0, 0), new Point(300, 200), new List<Color> { Color.DarkSlateGray, Color.DimGray, Color.Black, Color.FromArgb(255, 40, 65, 65) }, $"TEST WINDOW {Kernel.WindowList.Count}");
            }

            // Toggle the power menu when the escape key is pressed
            if (key.Key== ConsoleKeyEx.Escape)
            {
                PowerOff(Kernel.canvas, "-sr");
            }

            // Disable the canvas when the end key is pressed
            if(key.Key == ConsoleKeyEx.End)
            {
                Kernel.canvas.Disable();
            }

            // If the mouse is over the menu icon and the left button is pressed, toggle the menu
            if (IsBetween((int)Kernel.MouseX, 8, 36) && IsBetween((int)Kernel.MouseY, Kernel.ScreenHeight - 55, Kernel.ScreenHeight) && MouseManager.LastMouseState != MouseState.Left && MouseManager.LastMouseState != MouseState.Right)
            {
                ChangeMouseCursor(Kernel.MouseLink);
                if (MouseManager.MouseState == MouseState.Left)
                {
                    MouseManager.HandleMouse((int)Kernel.MouseX * 0, (int)Kernel.MouseY * 0, 1, 0);
                    Kernel.ShowMenu = !Kernel.ShowMenu;
                }
            }

            // Handle the exit button
            else if (IsBetween((int)Kernel.MouseX, 8, 79) && IsBetween((int)Kernel.MouseY, Kernel.ScreenHeight - 80, Kernel.ScreenHeight - 60) && Kernel.ShowMenu && MouseManager.LastMouseState != MouseState.Left && MouseManager.LastMouseState != MouseState.Right)
            {
                ChangeMouseCursor(Kernel.MouseLink);
                if (MouseManager.MouseState == MouseState.Left)
                    PowerOff(Kernel.canvas, "-s");
            }

            // Handle the restart button
            else if (IsBetween((int)Kernel.MouseX, 85, 154) && IsBetween((int)Kernel.MouseY, Kernel.ScreenHeight - 80, Kernel.ScreenHeight - 60) && Kernel.ShowMenu && MouseManager.LastMouseState != MouseState.Left && MouseManager.LastMouseState != MouseState.Right)
            {
                ChangeMouseCursor(Kernel.MouseLink);
                if (MouseManager.MouseState == MouseState.Left)
                    PowerOff(Kernel.canvas, "-r");
            }

            else
            {
                ChangeMouseCursor(Kernel.MouseNormal);
            }
        }
    }
}
