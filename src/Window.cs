/* DIRECTIVES */
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Graphics.Fonts;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class Window
    {
        /* VARIABLES */
        // Boolean(s)
        public bool ActiveWindow = true;

        // List(s)
        public List<Color> WindowColors = new List<Color>();

        // Points
        public Point WindowLocation;
        public Point WindowSize;

        // Strings
        public string WindowTitle;

        /* FUNCTIONS */
        // Draw the window
        public void DrawWindow()
        {
            /* Window color order:
             1. Title bar color
             2. Window color
             3. Title text color
             4. Close box color
             */

            // Try to draw the window (for stability reasons)
            try
            {
                // If the window is the active window, draw the original titlebar color
                // Otherwise, draw the titlebar color with 30 subtracted from it's R, G, and B values
                if (ActiveWindow)
                {
                    Kernel.canvas.DrawFilledRectangle(WindowColors[0], WindowLocation.X, WindowLocation.Y, WindowSize.X, 40);
                }
                else
                {
                    Kernel.canvas.DrawFilledRectangle(Color.FromArgb(Math.Max(WindowColors[0].A - 30, 0), Math.Max(WindowColors[0].R - 30, 0), Math.Max(WindowColors[0].G - 30, 0), Math.Max(WindowColors[0].B - 30, 0)), WindowLocation.X, WindowLocation.Y, WindowSize.X, 40);
                }

                // Draw the window
                Kernel.canvas.DrawFilledRectangle(WindowColors[1], WindowLocation.X, WindowLocation.Y + 40, WindowSize.X, WindowSize.Y - 40);
                Kernel.canvas.DrawFilledRectangle(WindowColors[3], WindowLocation.X + WindowSize.X - 42, WindowLocation.Y, 42, 40);
                Kernel.canvas.DrawImage(Kernel.CloseWindow, WindowLocation.X + WindowSize.X - (int)Kernel.CloseWindow.Width - 12, WindowLocation.Y + 20 - (int)Kernel.CloseWindow.Height / 2);
                Kernel.canvas.DrawString(WindowTitle, PCScreenFont.Default, WindowColors[2], WindowLocation.X + 25, WindowLocation.Y + 10);
            }
            catch (Exception ex)
            {
                // Send an error message over the serial port if an error occured
                SerialPort.SendString($"[ERROR] >> Failed to draw window \"{WindowTitle}\":\n\t{ex.Message}\n\n", SerialPort.COM1);
            }
        }

        // Detect if the mouse is hovering over the titlebar
        public bool IsHoveringOverTitlebar()
        {
            try
            {
                return (HelperFunctions.IsBetween((int)Kernel.MouseX, WindowLocation.X, WindowLocation.X + WindowSize.X - 42) && HelperFunctions.IsBetween((int)Kernel.MouseY, WindowLocation.Y, WindowLocation.Y + 40));
            }
            catch
            {
                return false;
            }
        }

        // Detect if the window is able to move
        public bool IsMovable()
        {
            try
            {
                return (HelperFunctions.IsBetween((int)Kernel.MouseX, WindowLocation.X, WindowLocation.X + WindowSize.X - 42) && HelperFunctions.IsBetween((int)Kernel.MouseY, WindowLocation.Y, WindowLocation.Y + 40) && MouseManager.LastMouseState != MouseState.Left && MouseManager.LastMouseState != MouseState.Right);
            }
            catch
            {
                return false;
            }
        }

        // Detect if the window is able to be closed
        public bool IsClosable()
        {
            try
            {
                return (HelperFunctions.IsBetween((int)Kernel.MouseX, WindowLocation.X + WindowSize.X - 42, WindowLocation.X + WindowSize.X) && HelperFunctions.IsBetween((int)Kernel.MouseY, WindowLocation.Y, WindowLocation.Y + 40) && MouseManager.LastMouseState != MouseState.Left && MouseManager.LastMouseState != MouseState.Right);
            }
            catch
            {
                return false;
            }
        }
    }
}
