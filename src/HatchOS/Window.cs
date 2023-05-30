/* DIRECTIVES */
using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class Window
    {
        /* VARIABLES */
        // Boolean(s)
        public bool IsActiveWindow = true;
        public bool UseGradient = false;

        // Integers
        int Count = 0;

        // List(s)
        public List<Color> WindowColors = new List<Color>();
        public List<WindowElement> WindowElements = new List<WindowElement>();

        // Gradients
        public Canvas TitlebarGradient;

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
                // Make sure the window colors list always has at least 4 values
                if(WindowColors.Count < 4)
                {
                    for(int i = WindowColors.Count; i < 4; i++)
                        WindowColors.Add(Color.Black);
                }

                // If the window is the active window, draw the original titlebar color
                // Otherwise, draw the titlebar color with 30 subtracted from it's R, G, and B values
                if (IsActiveWindow)
                {
                    Kernel.canvas.DrawFilledRectangle(WindowLocation.X, WindowLocation.Y, (ushort)WindowSize.X, 40, 0, WindowColors[0]);
                    if(UseGradient)
                        Kernel.canvas.DrawImage(WindowLocation.X, WindowLocation.Y, TitlebarGradient, false);
                }
                else
                {
                    Kernel.canvas.DrawFilledRectangle(WindowLocation.X, WindowLocation.Y, (ushort)WindowSize.X, 40, 0, new(Math.Max(WindowColors[0].A, 0), Math.Max(WindowColors[0].R - 30, 0), Math.Max(WindowColors[0].G - 30, 0), Math.Max(WindowColors[0].B - 30, 0)));
                }

                // Draw the window
                Kernel.canvas.DrawFilledRectangle(WindowLocation.X, WindowLocation.Y + 40, (ushort)WindowSize.X, (ushort)((ushort)WindowSize.Y - 40), 0, WindowColors[1]);
                Kernel.canvas.DrawFilledRectangle(WindowLocation.X + WindowSize.X - 42, WindowLocation.Y, 42, 40, 0, WindowColors[3]);
                Kernel.canvas.DrawImage(WindowLocation.X + WindowSize.X - (int)Kernel.CloseWindow.Width - 12, WindowLocation.Y + 20 - (int)Kernel.CloseWindow.Height / 2, Kernel.CloseWindow, false);
                Kernel.canvas.DrawString(WindowLocation.X + 25, WindowLocation.Y + 10, WindowTitle, default, WindowColors[2]);
                foreach(var Element in WindowElements)
                {
                    if(Element.ElementType == "StringElement")
                    {
                        Count = 0;
                        if (Element.ElementData.ToString().Contains('\0'))
                        {
                            foreach (string ConsolePart in Element.ElementData.ToString().Split('\0'))
                            {
                                if (ConsolePart != "\0")
                                {
                                    Kernel.canvas.DrawString(WindowLocation.X + Element.ElementPosition.X, WindowLocation.Y + 40 + Element.ElementPosition.Y + Count, ConsolePart, default, Element.ElementColor);
                                }

                                Count += 16;
                            }
                        }
                        else
                        {
                            Kernel.canvas.DrawString(WindowLocation.X + Element.ElementPosition.X, WindowLocation.Y + 40 + Element.ElementPosition.Y, Element.ElementData.ToString(), default, Element.ElementColor);
                        }
                    }

                    else if (Element.ElementType == "ImageElement")
                    {
                        Kernel.canvas.DrawImage(WindowLocation.X + Element.ElementPosition.X, WindowLocation.Y + 40 + Element.ElementPosition.Y, Image.FromBitmap(Convert.FromBase64String(Element.ElementData)), true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Send an error message over the serial port if an error occured
                if (Kernel.Debug)
                    SerialPort.SendString("[ERROR] >> Failed to draw window \"" + WindowTitle + "\":\n" + ex.Message + "\n\n", COMPort.COM1);

                CloseWindow();
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

        // Close the window
        public void CloseWindow()
        {
            // Remove the window from the list
            Kernel.WindowList.Remove(this);
            if (Kernel.WindowList.Count > 0)
            {
                Kernel.ActiveWindow = Kernel.WindowList[Kernel.WindowList.Count - 1];
            }
            else
            {
                Kernel.ActiveWindow = null;
            }
        }

        // Close the window without affecting the kernel's ActiveWindow property (only to be used when RAM is close to being full)
        public void CloseWindowEmergency()
        {
            // Remove the window from the list
            Kernel.WindowList.Remove(this);
        }
    }
}
