/* DIRECTIVES */
using System;
using Cosmos.System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Core;
using static HatchOS.HelperFunctions;
using static HatchOS.PowerFunctions;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    internal class Input
    {
        /* VARIABLES */
        public static uint MouseChange = 10;
        public static StringBuilder KeybBuffer = new StringBuilder(31);
        public static string ActiveTerminalLine;

        /* FUNCTIONS */
        // Handle keyboard / mouse input
        public static void HandleInput()
        {
            // Update the mouse values
            Kernel.MouseX = Math.Clamp(MouseManager.X, 0, (uint)Kernel.ScreenWidth - Kernel.Mouse.Width);
            Kernel.MouseY = Math.Clamp(MouseManager.Y, 0, (uint)Kernel.ScreenHeight - Kernel.Mouse.Height);

            // Test if the mouse has been moved
            Kernel.MouseMoved = !(Kernel.OldMouseX == Kernel.MouseX && Kernel.OldMouseY == Kernel.MouseY);

            // Try to read a key from the keyboard
            KeyboardManager.TryReadKey(out var key);

            if(key != null)
            {
                KeybBuffer.Append(key.KeyChar);
                //KeybBuffer += key.KeyChar;
            }

            // If one of the Windows keys are pressed, toggle the menu
            if (key.Key == ConsoleKeyEx.LWin || key.Key == ConsoleKeyEx.RWin)
            {
                Kernel.ShowMenu = !Kernel.ShowMenu;
            }

            // If the F12 key is pressed, toggle debug info
            if (key.Key == ConsoleKeyEx.F12)
            {
                Kernel.Debug = !Kernel.Debug;
            }

            // If the F4 key is pressed, panic
            if (key.Key == ConsoleKeyEx.F4)
            {
                KernelPanic.Panic("USER FORCED PANIC", "0xFFFFFF");
            }

            // Mouse movement with arrow keys
            if (key.Key == ConsoleKeyEx.UpArrow)
            {
                MouseManager.Y -= MouseChange;
            }

            if (key.Key == ConsoleKeyEx.DownArrow)
            {
                MouseManager.Y += MouseChange;
            }

            if (key.Key == ConsoleKeyEx.LeftArrow)
            {
                MouseManager.X -= MouseChange;
            }

            if (key.Key == ConsoleKeyEx.RightArrow)
            {
                MouseManager.X += MouseChange;
            }

            // Send a string over serial that contains the current cpu information
            if(key.Key == ConsoleKeyEx.F2)
            {
                SerialPort.SendString($"[== CPU INFORMATION ==]\n\tBRAND: {CPU.GetCPUBrandString()}\n\tVENDOR: {CPU.GetCPUVendorName()}\n\tUPTIME: {CPU.GetCPUUptime()/ 1000000} ms\n");
            }

            // Simulate a mouse click when the F7 key is pressed
            if(key.Key== ConsoleKeyEx.F7)
            {
                MouseManager.HandleMouse((int)Kernel.MouseX * 0, (int)Kernel.MouseY * 0, 0x1, 0);
            }

            // Create a new debug window when the F8 key is pressed
            if (key.Key == ConsoleKeyEx.F8)
            {
                WindowManager.CreateNewWindow(Kernel.WindowList, new Point(64, 64), new Point(300, 200), new List<PrismGraphics.Color> { PrismGraphics.Color.GetPacked(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), PrismGraphics.Color.LightGray, PrismGraphics.Color.Black, PrismGraphics.Color.GetPacked(255, 40, 65, 65) }, "TEST WINDOW " + Kernel.WindowList.Count);
            }

            // Interpret a sample batch script
            if (key.Key == ConsoleKeyEx.F9)
            {
                BatchInterpreter.InterpretBatchScript("SET A=0\nmsgbox(\"bruh\", \"amoGnUS!!!111!1111!!11\")");
            }

            // Create a terminal
            if (key.Key == ConsoleKeyEx.F10)
            {
                WindowManager.CreateNewWindow(Kernel.WindowList, new Point(64, 64), new Point(200, 120), new List<PrismGraphics.Color> { PrismGraphics.Color.DeepGray, PrismGraphics.Color.Black, PrismGraphics.Color.Black, PrismGraphics.Color.GetPacked(255, 40, 65, 65) }, "TERMINAL");
                WindowElement TerminalText = new WindowElement();
                TerminalText.ElementType = "StringElement";
                TerminalText.ElementData = ">> _";
                TerminalText.ElementColor = PrismGraphics.Color.White;
                TerminalText.ElementPosition = new Point(0, 0);
                Kernel.ActiveWindow.WindowElements.Add(TerminalText);
            }

            // Toggle the power menu when the escape key is pressed
            if (key.Key == ConsoleKeyEx.Escape)
            {
                PowerOff(Kernel.canvas, "-sr");
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

            // If the active window is the terminal, send input to it
            if (Kernel.ActiveWindow != null && Kernel.ActiveWindow.WindowTitle == "TERMINAL" && key != null)
            {
                if (key.Key == ConsoleKeyEx.Backspace && Kernel.ActiveWindow.WindowElements[0].ElementData.Length > 4)
                {
                    Kernel.ActiveWindow.WindowElements[0].ElementData = RemoveCharsFromEnd(Kernel.ActiveWindow.WindowElements[0].ElementData, 2) + "_";
                    ActiveTerminalLine = TrimLastCharacter(ActiveTerminalLine);
                    KeybBuffer.Remove(KeybBuffer[KeybBuffer.Length - 1], 1);
                }

                else if(char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || key.KeyChar == ' ')
                {                    
                    if (ActiveTerminalLine.Length < 21)
                    {
                        ActiveTerminalLine += KeybBuffer[KeybBuffer.Length - 1].ToString();
                        Kernel.ActiveWindow.WindowElements[0].ElementData = TrimLastCharacter(Kernel.ActiveWindow.WindowElements[0].ElementData) + KeybBuffer[KeybBuffer.Length - 1].ToString() + "_";
                    }
                }

                else
                {
                    if(key.Key == ConsoleKeyEx.Enter)
                    {
                        bool UseNullCharacter = true;
                        Kernel.ActiveWindow.WindowElements[0].ElementData = TrimLastCharacter(Kernel.ActiveWindow.WindowElements[0].ElementData);

                        if (StringContainsCharNTimes(Kernel.ActiveWindow.WindowElements[0].ElementData, '\0') > 2)
                        {
                            Kernel.ActiveWindow.WindowElements[0].ElementData = null;
                            UseNullCharacter = false;
                        }

                        if (ActiveTerminalLine.ToLower() == "exit")
                        {
                            Kernel.ActiveWindow.CloseWindow();
                            ActiveTerminalLine = "";
                        }
                        else
                        {
                            Kernel.ActiveWindow.WindowElements[0].ElementData += "\0Invalid command!";
                            if (!UseNullCharacter)
                            {
                                Kernel.ActiveWindow.WindowElements[0].ElementData = Kernel.ActiveWindow.WindowElements[0].ElementData.Substring(1, Kernel.ActiveWindow.WindowElements[0].ElementData.Length - 1);
                            }

                        }

                        Kernel.ActiveWindow.WindowElements[0].ElementData += "\0>> _";
                        ActiveTerminalLine = "";
                    }
                }
            }
        }
    }
}
