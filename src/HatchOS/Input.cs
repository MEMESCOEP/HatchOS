﻿/* DIRECTIVES */
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.System;
using Cosmos.Core.Memory;
using static HatchOS.HelperFunctions;
using static HatchOS.PowerFunctions;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;

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
        public static int PromptLength = 4;

        /* FUNCTIONS */
        // Handle keyboard / mouse input
        public static void HandleInput()
        {
            // Update the mouse values
            Kernel.MouseX = MouseManager.X; //Math.Clamp(MouseManager.X, 0, (uint)Kernel.ScreenWidth - Kernel.Mouse.Width);
            Kernel.MouseY = MouseManager.Y; //Math.Clamp(MouseManager.Y, 0, (uint)Kernel.ScreenHeight - Kernel.Mouse.Height);

            // Test if the mouse has been moved
            Kernel.MouseMoved = !(Kernel.OldMouseX == Kernel.MouseX && Kernel.OldMouseY == Kernel.MouseY);

            Kernel.OldMouseX = Kernel.MouseX;
            Kernel.OldMouseY = Kernel.MouseY;

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
                Kernel.StartMenuController.Reset();
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

            // Start the backup shell
            if (key.Key == ConsoleKeyEx.F11)
            {
                Kernel.canvas.DrawImage(0, 0, Kernel.PowerGradientBG, false);
                Kernel.canvas.DrawString(0, 0, "THE SERIAL SHELL IS CURRENTLY RUNNING", default, Color.White);
                Kernel.canvas.Update();
                Kernel.Debug = true;
                BackupShell.Init();
            }

            // Send a string over serial that contains the current cpu information
            if (key.Key == ConsoleKeyEx.F2)
            {
                SerialPort.SendString($"[== CPU INFORMATION ==]\n\tBRAND: {CPU.GetCPUBrandString()}\n\tVENDOR: {CPU.GetCPUVendorName()}\n\tUPTIME: {CPU.GetCPUUptime()/ 1000000} ms\n");
            }

            // Display a 3D demo
            if (key.Key == ConsoleKeyEx.F5)
            {
                PS2Controller controller = new();
                PrismAPI.Graphics.Rasterizer.Engine engine = new(800, 600, 60);
                engine.Objects.Add(PrismAPI.Graphics.Rasterizer.Mesh.GetCube(250, 250, 250));
                engine.Objects.Add(PrismAPI.Graphics.Rasterizer.Mesh.GetCube(100, 100, 100));

                int ColorValue = 0;

                foreach(var Triangle in engine.Objects[0].Triangles)
                {
                    Triangle.Color = new(255, ColorValue, ColorValue, ColorValue);
                    ColorValue += engine.Objects[0].Triangles.Count;
                }

                ColorValue = 0;

                foreach (var Triangle in engine.Objects[1].Triangles)
                {
                    Triangle.Color = new(255, ColorValue, ColorValue, ColorValue);
                    ColorValue += engine.Objects[1].Triangles.Count + 32;
                }

                while (true)
                {
                    //Kernel.canvas.DrawString(0, 24, "ROT=" + engine.Objects[0].Rotation.ToString(), default, Color.White);
                    //Kernel.canvas.DrawString(0, 36, "POS=" + engine.Objects[0].Position.ToString(), default, Color.White);
                    //Kernel.canvas.DrawString(0, 48, "PS2=" + controller.ReadByteAfterAck().ToString(), default, Color.White);
                    //engine.Objects[0].Rotation.X += 0.01f;
                    //engine.Objects[0].Rotation.Y += 0.01f;
                    //engine.Objects[0].Rotation.Z += 0.01f;

                    KeyboardManager.TryReadKey(out var Key3D);
                    var PS2Key = controller.ReadByteAfterAck().ToString();
                    engine.Render();
                    Kernel.canvas.DrawImage(0, 0, engine);
                    Kernel.canvas.DrawString(0, 0, "Press ESCAPE to exit", default, Color.White);
                    Kernel.canvas.DrawString(0, 12, "FPS=" + Kernel.canvas.GetFPS(), default, Color.White);
                    Kernel.canvas.Update();

                    if(Key3D.Key != ConsoleKeyEx.NoName)
                    {
                        if (Key3D.Key == ConsoleKeyEx.Escape)
                        {
                            break;
                        }
                        else if (PS2Key == "17" || Key3D.Key == ConsoleKeyEx.UpArrow || Key3D.Key == ConsoleKeyEx.W)
                        {
                            engine.Objects[0].Position.Y -= 15;
                        }
                        else if (PS2Key == "31" || Key3D.Key == ConsoleKeyEx.DownArrow || Key3D.Key == ConsoleKeyEx.S)
                        {
                            engine.Objects[0].Position.Y += 15;
                        }
                        else if (PS2Key == "30" || Key3D.Key == ConsoleKeyEx.LeftArrow || Key3D.Key == ConsoleKeyEx.A)
                        {
                            engine.Objects[0].Position.X -= 15;
                        }
                        else if (PS2Key == "32" || Key3D.Key == ConsoleKeyEx.RightArrow || Key3D.Key == ConsoleKeyEx.D)
                        {
                            engine.Objects[0].Position.X += 15;
                        }
                        else if (PS2Key == "16" || Key3D.Key == ConsoleKeyEx.Q)
                        {
                            engine.Objects[0].Position.Z -= 15;
                        }
                        else if (PS2Key == "18" || Key3D.Key == ConsoleKeyEx.E)
                        {
                            engine.Objects[0].Position.Z += 15;
                        }
                        else if(PS2Key == "19" || Key3D.Key == ConsoleKeyEx.R)
                        {
                            engine.Objects[0].Rotation = new(0, 0, 0);
                            engine.Objects[0].Position = new(0, 0, 0);
                        }

                        engine.Objects[0].Rotation.X += -MouseManager.DeltaX * MouseManager.MouseSensitivity * 0.01f;
                        engine.Objects[0].Rotation.Y += MouseManager.DeltaY * MouseManager.MouseSensitivity * 0.01f;
                    }

                    Heap.Collect();
                }

            }

            // Simulate a mouse click when the F7 key is pressed
            if (key.Key== ConsoleKeyEx.F7)
            {
                MouseManager.HandleMouse((int)Kernel.MouseX * 0, (int)Kernel.MouseY * 0, 0x1, 0);
            }

            // Create a new debug window when the F8 key is pressed
            if (key.Key == ConsoleKeyEx.F8)
            {
                WindowManager.CreateNewWindow(Kernel.WindowList, new Point(64, 64), new Point(300, 200), new List<Color> { new(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), Color.LightGray, Color.Black, new(255, 40, 65, 65) }, "TEST WINDOW " + Kernel.WindowList.Count);
            }

            // Interpret a sample batch script
            if (key.Key == ConsoleKeyEx.F9)
            {
                WindowManager.CreateMessageBox("TEST ERROR", "This is a test message!\nThe message type is 0, which means ERROR.", 0);
                //WindowManager.CreateMessageBox("TEST WARNING", "This is a test message!\nThe message type is 0, which means WARNING.", 1);
                //WindowManager.CreateMessageBox("TEST EXCLAMATION", "This is a test message!\nThe message type is 0, which means EXCLAMATION.", 2);
                //WindowManager.CreateMessageBox("TEST QUESTION", "This is a test message!\nThe message type is 0, which means QUESTION.", 3);
            }

            // Create a terminal
            if (key.Key == ConsoleKeyEx.F10)
            {
                WindowManager.CreateNewWindow(Kernel.WindowList, new Point(64, 64), new Point(200, 120), new List<Color> { new(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), Color.Black, Color.Black, new(255, 40, 65, 65) }, "TERMINAL");
                WindowElement TerminalText = new WindowElement();
                TerminalText.ElementType = "StringElement";
                TerminalText.ElementData = ">> _";
                TerminalText.ElementColor = Color.White;
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
                    Kernel.StartMenuController.Reset();
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
                if (key.Key == ConsoleKeyEx.Backspace && Kernel.ActiveWindow.WindowElements[0].ElementData.Length > PromptLength)
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
                            PromptLength = 4;
                            UseNullCharacter = false;
                        }

                        if (ActiveTerminalLine.ToLower() == "exit")
                        {
                            Kernel.ActiveWindow.CloseWindow();
                            ActiveTerminalLine = "";
                        }

                        else if (ActiveTerminalLine.ToLower() == "ver")
                        {
                            StringBuilder sb = new(Kernel.ActiveWindow.WindowElements[0].ElementData);
                            sb.Append("\0");
                            sb.Append(Kernel.VersionString);
                            Kernel.ActiveWindow.WindowElements[0].ElementData = sb.ToString();
                            ActiveTerminalLine = "";
                        }

                        else if (ActiveTerminalLine.ToLower() == "help")
                        {
                            OpenHelpWindow();
                            foreach(var window in Kernel.WindowList)
                            {
                                if(window.WindowTitle == "Terminal")
                                {
                                    DisplayConsoleMsg("[INFO] >> " + window.WindowElements[0].ElementData.EndsWith(">> help_"));
                                    Kernel.ActiveWindow = window;
                                }
                            }
                        }

                        else
                        {
                            StringBuilder sb = new(Kernel.ActiveWindow.WindowElements[0].ElementData);
                            sb.Append("\0");
                            sb.Append("Bad cmd!");
                            Kernel.ActiveWindow.WindowElements[0].ElementData = sb.ToString();
                            if (!UseNullCharacter)
                            {
                                Kernel.ActiveWindow.WindowElements[0].ElementData = Kernel.ActiveWindow.WindowElements[0].ElementData.Substring(1, Kernel.ActiveWindow.WindowElements[0].ElementData.Length - 1);
                            }

                        }

                        Kernel.ActiveWindow.WindowElements[0].ElementData += "\0>> _";
                        PromptLength = Kernel.ActiveWindow.WindowElements[0].ElementData.Length;
                        ActiveTerminalLine = "";
                    }
                }
            }
        }

        public static void OpenHelpWindow()
        {
            // Create the welcome window
            WindowManager.CreateNewWindow(Kernel.WindowList, new Point(0, 0), new Point(640, 480), new List<Color> { new(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), Color.LightGray, Color.Black, new(255, 40, 65, 65) }, "HatchOS Help");
            WindowElement WelcomeText = new WindowElement();
            WelcomeText.ElementType = "StringElement";
            WelcomeText.ElementData = "Keyb. shortcuts:";
            WelcomeText.ElementPosition = new Point(5, 0);
            Kernel.WindowList[Kernel.WindowList.Count - 1].WindowElements.Add(WelcomeText);

            WindowElement AuthorText = new WindowElement();
            AuthorText.ElementType = "StringElement";
            AuthorText.ElementData = "  ESC: Power options";
            AuthorText.ElementPosition = new Point(5, 12);
            Kernel.WindowList[Kernel.WindowList.Count - 1].WindowElements.Add(AuthorText);
        }
    }
}
