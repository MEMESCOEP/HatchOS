/* CREDITS */
// HatchOS, by Andrew Maney
// PrismAPI by Terminal.cs (https://github.com/Project-Prism/Prism-OS)
// Licensed under the MIT License
// Credits last updated 5-19-23

/* DIRECTIVES */
using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.HAL.Drivers.Audio;
using IL2CPU.API.Attribs;
using PrismAPI.Graphics;
using PrismAPI.Hardware.GPU;
using PrismAPI.Graphics.Animation;
using static HatchOS.HelperFunctions;
using Color = PrismAPI.Graphics.Color;
using System.IO;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class Kernel : Sys.Kernel
    {
        /* VARIABLES */
        // Embedded resources
        [ManifestResourceStream(ResourceName = "HatchOS.Resources.BG_1280x1024.bmp")]
        static byte[] WallpaperData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.mouse.bmp")]
        static byte[] MouseNormalData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.xp_link.bmp")]
        static byte[] MouseLinkData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.xp_move.bmp")]
        static byte[] MouseMoveData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.OS_Logo.bmp")]
        static byte[] LogoData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.close_window.bmp")]
        static byte[] CloseWindowData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.PowerGradient.bmp")]
        static byte[] PowerGradientData;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.BootSound.wav")]
        static byte[] BootSoundAudio;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.shutdown.wav")]
        public static byte[] ShutdownAudio;
        
        [ManifestResourceStream(ResourceName = "HatchOS.Resources.ExclamationSound.wav")]
        public static byte[] ExclamationAudio;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.ErrorSound.wav")]
        public static byte[] ErrorAudio;
        
        [ManifestResourceStream(ResourceName = "HatchOS.Resources.ErrorSymbol.bmp")]
        public static byte[] ErrorSymbol;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.WarningSymbol.bmp")]
        public static byte[] WarningSymbol;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.QuestionSymbol.bmp")]
        public static byte[] QuestionSymbol;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.ExclamationSymbol.bmp")]
        public static byte[] ExclamationSymbol;

        // Canvas
        public static Display canvas;

        // Points
        public static Point TimePoint;
        public static Point DatePoint;
        public static Point LogoPoint;

        // Virtual file system(s)
        public static Sys.FileSystem.CosmosVFS vfs;

        // Animation controllers
        public static AnimationController StartMenuController;

        // Bitmaps
        public static Canvas Wallpaper;
        public static Canvas Mouse;
        public static Canvas MouseNormal;
        public static Canvas MouseLink;
        public static Canvas MouseMove;
        public static Canvas OSLogo;
        public static Canvas CloseWindow;
        public static Canvas PowerGradientBG;

        // Color(s)
        public static Color TaskBarColor;
        public static Color LoadingColor;
        public static Color StartButtonColor;
        public static Color StartButtonColorPressed;

        // Gradient(s)
        public static Canvas TaskBarGradient;

        // Booleans
        public static bool ShowMenu = false;
        public static bool Debug = true;
        public static bool Booted = false;
        public static bool MouseMoved = false;
        private static bool EnableBootScreen = true;
        public static bool AudioEnabled = false;

        // Audio driver(s)
        public static AudioDriver audioDriver;

        // Unsigned integers
        public static uint MouseX, MouseY, BootTimer, LoadPos = 160, OldMouseX, OldMouseY, AvailableRam;

        // Floating point numbers
        public static float AudioVolume = 1f; // Max value should be 1f (or 100 percent volume. Values range from 0f to 1f, which means 0 - 100 percent.)

        // Random
        public static Random random = new Random();

        // Timer(s)
        public static Timer HeapCollectionTimer; // = new PIT.PITTimer(TimerCallback, MillisecondsToNanoseconds(250), true);

        // Lists
        public static List<Window> WindowList = new();
        public static List<Point> LogoHighlightPoints = new();

        // Integers
        public static int ScreenWidth = 800;
        public static int ScreenHeight = 600;
        public static int LoadPosSegment = ScreenWidth / 5;
        public static int ColorR = 0;
        public static int ColorG = 0;
        public static int ColorB = 0;
        public static int GCCount = 0;
        public static int FrameCount = 0;
        public static int offsetX = 0;
        public static int offsetY = 0;

        // String(s)
        public static string CPUName;
        public static string VersionString = "P_5-19-23 (836)";

        // Window(s)
        public static Window ActiveWindow;

        /* FUNCTIONS */
        // Executed once upon system startup
        protected override void BeforeRun()
        {
            if (Kernel.Debug)
                DisplayConsoleMsg($"[INFO] >> HatchOS {VersionString} is starting...");

            // Check to make sure there is enough ram to run HatchOS
            if (Kernel.Debug)
                DisplayConsoleMsg("[INFO] >> Checking memory requirements...");

            if (CPU.GetAmountOfRAM() + 2 <= 103)
            {
                canvas = Display.GetDisplay(640, 480);
                KernelPanic.Panic("INSUFFICIENT MEMORY", "0x0A0E");
            }

            // Attempt to create a canvas with the specified width and height
            if (Kernel.Debug) 
                DisplayConsoleMsg("[INFO] >> Initializing display...");

            try
            {
                canvas = Display.GetDisplay((ushort)ScreenWidth, (ushort)ScreenHeight);
                if (Kernel.Debug)
                    DisplayConsoleMsg($"[INFO] >> Using canvas type \"{canvas.GetName()}\", with a resolution of: \"{ScreenWidth}x{ScreenHeight}\"");
            }
            catch(Exception ex)
            {
                if (Kernel.Debug)
                    DisplayConsoleError($"\n[ERROR] Failed to initialize display!\n{ex.Message}");

                PowerFunctions.Restart();
            }

            // If the canvas is null, do the following:
            //      1. Print an error
            //      2. Start the backup shell
            if (canvas == null || canvas.Width <= 0 || canvas.Height <= 0)
            {
                if (Kernel.Debug)
                    DisplayConsoleError("\n[ERROR] >> Failed to create a canvas!");

                BackupShell.Init();
            }

            // Display the boot screen (if the "EnableBootScreen" boolean is set to true)
            if (EnableBootScreen)
            {
                try
                {
                    canvas.Clear(new(255, 63, 90, 89));
                    canvas.DrawString((ScreenWidth / 2), (ScreenHeight / 2) + 32, "[ Starting HatchOS ]", default, Color.White, true);
                    canvas.Update();
                    var TempLogo = Image.FromBitmap(LogoData);
                    canvas.DrawImage((ScreenWidth / 2 - TempLogo.Width / 2), (ScreenHeight / 2 - TempLogo.Height / 2), TempLogo);
                    canvas.Update();
                }
                catch(Exception ex)
                {
                    if (Kernel.Debug)
                        DisplayConsoleError("[BSC_ERROR] >> " + ex.Message);
                }
            }

            // Assign points
            if (Kernel.Debug)
                DisplayConsoleMsg("[INFO] >> Assigning variables...");

            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Assigning variables...", default, Color.White);
            canvas.Update();
            TimePoint.X = ScreenWidth - 70;
            TimePoint.Y = ScreenHeight - 38;
            DatePoint.X = ScreenWidth - 70;
            DatePoint.Y = ScreenHeight - 20;
            LogoPoint.X = 8;
            LogoPoint.Y = ScreenHeight - 36;
            LogoHighlightPoints.Add(new(4, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 6));
            LogoHighlightPoints.Add(new(44, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(45, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(45, ScreenHeight - 6));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 5));
            LogoHighlightPoints.Add(new(45, ScreenHeight - 5));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 6));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(44, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(44, ScreenHeight - 36));
            LogoHighlightPoints.Add(new(44, ScreenHeight - 6));
            LogoHighlightPoints.Add(new(4, ScreenHeight - 5));
            LogoHighlightPoints.Add(new(44, ScreenHeight - 6));

            // Assign bitmaps (using embedded resources)
            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Getting bitmap data...", default, Color.White);
            canvas.Update();
            PowerGradientBG = Image.FromBitmap(PowerGradientData);
            CloseWindow = Image.FromBitmap(CloseWindowData);
            MouseNormal = Image.FromBitmap(MouseNormalData);
            Wallpaper = Image.FromBitmap(WallpaperData);
            MouseLink = Image.FromBitmap(MouseLinkData);
            MouseMove = Image.FromBitmap(MouseMoveData);
            OSLogo = Image.FromBitmap(LogoData);
            CPUName = CPU.GetCPUBrandString();
            Mouse = MouseNormal;

            // Assign color(s)
            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Assigning colors...", default, Color.White);
            canvas.Update();
            TaskBarColor = new(255, 123, 150, 149);
            //TaskBarGradient = Gradient.GetGradient(1024, 40, TaskBarColor, Color.Blue);
            StartButtonColor = new(255, 63, 90, 89);
            StartButtonColorPressed = new(255, 33, 60, 59);

            // Assign unsigned integers
            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Assigning UINTs...", default, Color.White);
            canvas.Update();
            AvailableRam = (uint)GCImplementation.GetAvailableRAM() * 1024;

            // Create and register the virtual file system
            if (Kernel.Debug)
                DisplayConsoleMsg("[INFO] >> Creating and registering the virtual file system...");

            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Creating and registering the virtual file system...", default, Color.White);
            canvas.Update();
            PrismAPI.Filesystem.FilesystemManager.Init();
            Directory.SetCurrentDirectory("0:\\");
            vfs = new();
            VFSManager.RegisterVFS(vfs, false, true);

            // Create animation controllers
            StartMenuController = new(0, ScreenHeight / 2 + 40, new(0, 0, 0, 0, 250), AnimationMode.Ease);

            // Attempt to get an IP address if there is a usable NIC installed in the computer
            if (NetworkDevice.Devices.Count > 0)
            {
                DisplayConsoleMsg("[INFO] >> Retrieving IP address from DHCP server...");
                canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
                canvas.DrawString(0, 0, "Retrieving IP address...", default, Color.White);
                canvas.Update();
                using (var xClient = new DHCPClient())
                {
                    /** Send a DHCP Discover packet **/
                    //This will automatically set the IP config after DHCP response
                    xClient.SendDiscoverPacket();
                }

                if(NetworkConfiguration.CurrentAddress.ToString() != null || NetworkConfiguration.CurrentAddress.ToString() == "0.0.0.0")
                {
                    DisplayConsoleMsg("[INFO] >> Got IP address: " + NetworkConfiguration.CurrentAddress.ToString());
                }
                else
                {
                    DisplayConsoleError("[ERROR] >> Failed to retrieve IP address from a DHCP server!");
                }
            }
            else
            {
                DisplayConsoleMsg("[WARN] >> No usable network interface cards were detected.");
            }

            // Initialize the audio system
            try
            {
                DisplayConsoleMsg("[INFO] >> Initializing audio...");
                canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
                canvas.DrawString(0, 0, "Initializing audio...", default, Color.White);
                audioDriver = AC97.Initialize(4096);
                AudioEnabled = true;
            }
            catch (Exception ex)
            {
                DisplayConsoleError("[ERROR] >> Failed to initialize audio: " + ex.Message);
            }

            // Set mouse properties
            DisplayConsoleMsg("[INFO] >> Setting mouse properties...");
            MouseManager.MouseSensitivity = 1f;
            MouseManager.ScreenWidth = (uint)ScreenWidth;
            MouseManager.ScreenHeight = (uint)ScreenHeight;
            MouseManager.X = (uint)(ScreenWidth / 2) - (uint)(Mouse.Width / 2);
            MouseManager.Y = (uint)(ScreenHeight / 2) - (uint)(Mouse.Height / 2);
            MouseX = MouseManager.X;
            MouseY = MouseManager.Y;

            DisplayConsoleMsg("[INFO] >> Boot process finished!");
            canvas.DrawFilledRectangle(0, 0, (ushort)ScreenWidth, 24, 0, new(255, 63, 90, 89));
            canvas.DrawString(0, 0, "Done!", default, Color.White);
            canvas.Update();
            Booted = true;
        }

        // Executed every frame after BeforeRun() is finished running
        protected override void Run()
        {
            // Once the boot process is finished, show the desktop.
            // We'll only wait until the boot process is completed if the "EnableBootScreen" boolean
            // is set to false.
            if (Booted || !EnableBootScreen)
            {
                System.Console.Write("\n");

                // Try to play the startup sound. This will only work if there's an AC97 audio card (or compatible)
                // installed in the computer.
                PlayAudioFromMemory(BootSoundAudio, AudioVolume);

                // Show the desktop
                ShowGUI();
            }
        }

        // Draw everything
        public static void Draw()
        {
            try
            {
                // Draw the wallpaper
                canvas.DrawImage(0, 0, Wallpaper, false);

                // Draw each window in the window list
                foreach (Window window in WindowList)
                {
                    window.IsActiveWindow = (ActiveWindow == window);
                    window.DrawWindow();
                }

                // Draw the taskbar
                canvas.DrawFilledRectangle(0, (ushort)(ScreenHeight - 40), (ushort)ScreenWidth, 40, 0, TaskBarColor);
                canvas.DrawFilledRectangle(OSLogo.Width + 16, LogoPoint.Y - 2, 4, 36, 0, StartButtonColor);

                // If the OS menu should be visible, draw it
                if (ShowMenu)
                {                    
                    // Button background
                    canvas.DrawFilledRectangle(4, ScreenHeight - 36, 40, 32, 0, StartButtonColorPressed);
                    canvas.DrawLine(LogoHighlightPoints[0].X, LogoHighlightPoints[0].Y, LogoHighlightPoints[1].X, LogoHighlightPoints[1].Y, Color.Black);
                    canvas.DrawLine(LogoHighlightPoints[0].X, LogoHighlightPoints[0].Y, LogoHighlightPoints[2].X, LogoHighlightPoints[2].Y, Color.Black);
                    canvas.DrawLine(LogoHighlightPoints[3].X, LogoHighlightPoints[3].Y, LogoHighlightPoints[4].X, LogoHighlightPoints[4].Y, Color.White);
                    canvas.DrawLine(LogoHighlightPoints[5].X, LogoHighlightPoints[5].Y, LogoHighlightPoints[6].X, LogoHighlightPoints[6].Y, Color.White);

                    // Menu background
                    canvas.DrawFilledRectangle(0, (int)(ScreenHeight - 280), 170, 240, 0, new(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B));

                    // Shutdown button
                    canvas.DrawFilledRectangle(12, ScreenHeight - 78, 64, 20, 0, Color.LightGray);
                    canvas.DrawString(13, ScreenHeight - 78, "Exit OS", default, Color.Black);

                    // Restart button
                    canvas.DrawFilledRectangle(87, ScreenHeight - 78, 64, 20, 0, Color.LightGray);
                    canvas.DrawString(88, ScreenHeight - 78, "Restart", default, Color.Black);
                }
                else
                {
                    canvas.DrawFilledRectangle(4, ScreenHeight - 36, 40, 32, 0, StartButtonColor);
                    canvas.DrawLine(LogoHighlightPoints[7].X, LogoHighlightPoints[7].Y, LogoHighlightPoints[8].X, LogoHighlightPoints[8].Y, Color.White);
                    canvas.DrawLine(LogoHighlightPoints[9].X, LogoHighlightPoints[9].Y, LogoHighlightPoints[10].X, LogoHighlightPoints[10].Y, Color.White);
                    canvas.DrawLine(LogoHighlightPoints[11].X, LogoHighlightPoints[11].Y, LogoHighlightPoints[12].X, LogoHighlightPoints[12].Y, Color.Black);
                    canvas.DrawLine(LogoHighlightPoints[13].X, LogoHighlightPoints[13].Y, LogoHighlightPoints[14].X, LogoHighlightPoints[14].Y, Color.Black);
                }

                // If there is only 50 MB of ram left, enter emergency mode
                if(AvailableRam - GCImplementation.GetUsedRAM() / 1024 <= 51200)
                {
                    // Display a custom power screen
                    PowerFunctions.UsingCustomPowerMenu = true;
                    PowerFunctions.AllowEscapeKey = false;
                    PowerFunctions.CustomTitle = "[===== CRITICAL ERROR: LOW MEMORY =====]";
                    PowerFunctions.CustomMessage = "Please choose a power option.";
                    PowerFunctions.CustomTitleColor = Color.Red;
                    PowerFunctions.CustomMessageColor = Color.White;
                    PowerFunctions.PowerOff(canvas, "-sr");
                }

                // Draw task bar elements (Time, Date, volume control, and OS Logo)
                canvas.DrawImage(LogoPoint.X, LogoPoint.Y, OSLogo);
                canvas.DrawFilledRectangle((ushort)(ScreenWidth - 88), (ushort)(ScreenHeight - 38), (ushort)FindDifference(ScreenWidth - 88, ScreenWidth), 36, 0, StartButtonColor);
                canvas.DrawString(TimePoint.X, TimePoint.Y, RTC.Hour + ":" + RTC.Minute + ":" + RTC.Second, default, Color.Black);
                canvas.DrawString(DatePoint.X, DatePoint.Y, RTC.Month + ":" + RTC.DayOfTheMonth + ":" + RTC.Year, default, Color.Black);

                // If the debug text should be visible, draw it
                if (Debug)
                {
                    canvas.DrawFilledRectangle(0, 0, (ushort)(CPUName.Length * 10), 64, 0, Color.StackOverflowBlack);
                    canvas.DrawString(0, 0, "POS=(" + MouseX + "," + MouseY + ")", default, Color.StackOverflowWhite);
                    canvas.DrawString(0, 12, "RAM=" + GCImplementation.GetUsedRAM() / 1024 + "/" + AvailableRam + " KB", default, Color.StackOverflowWhite);
                    canvas.DrawString(0, 24, "FPS=" + canvas.GetFPS(), default, Color.StackOverflowWhite);
                    canvas.DrawString(0, 36, "FRC=" + FrameCount, default, Color.StackOverflowWhite);
                    canvas.DrawString(0, 48, "CPU=" + CPUName, default, Color.StackOverflowWhite);
                }

                // Draw the mouse cursor (using the DrawImageAlpha function makes sure pixels that
                // should not be visible are not drawn)
                canvas.DrawImage((int)MouseX, (int)MouseY, Mouse);

                // Since we're using a double-buffered driver, we need to copy the second (non-visible) framebuffer
                // into the first (visible) framebuffer. This ensures that there is no flickering or screen tearing,
                // and makes sure the graphics are visible to the user.
                //if(MouseMoved)
                canvas.Update();

                // Handle keyboard/mouse input
                Input.HandleInput();

                // Call the garbage collector so we don't have as many memory leaks. This also helps improve framerates.
                Heap.Collect();

                // Increment the frame count once a frame has finished drawing
                FrameCount++;
            }
            catch (Exception ex)
            {
                DisplayConsoleMsg("[ERROR] >> " + ex.Message);
            }
        }

        public static void ShowGUI()
        {
            // Create the welcome window
            WindowManager.CreateMessageBox("Welcome to HatchOS!", $"Welcome to HatchOS!\nLicense: MIT License\nVersion: {VersionString}\nGithub: https://github.com/memescoep/HatchOS", 2, true);

            if(NetworkConfiguration.CurrentAddress.ToString() == null || NetworkConfiguration.CurrentAddress.ToString() == "0.0.0.0")
            {
                WindowManager.CreateMessageBox("Invalid IP Address", "DHCP failed or the address was invalid.", 1);
            }

            Draw();

            // Display loop
            while (true)
            {
                // Attempt to draw (for stability reasons 🤓)
                try
                {
                    // Draw the screen
                    Draw();

                    // Iterate through the window list and get a reference to each window
                    foreach (Window window in WindowList)
                    {
                        // If the mouse cursor is over the window's title bar and the left mouse button is down,
                        // we can move it
                        if (window.IsMovable())
                        {
                            // Set the mouse cursor image
                            ChangeMouseCursor(MouseMove);

                            // Get the correct offset values for the X and Y coordinates
                            offsetX = (int)FindDifference(window.WindowLocation.X, MouseX);
                            offsetY = (int)FindDifference(window.WindowLocation.Y, MouseY);

                            // While the mouse's left button is pressed, move the window to the mouse's position (with the correct offsets)
                            // and draw the screen
                            while (MouseManager.MouseState == MouseState.Left)
                            {
                                // Set the mouse cursor image
                                ChangeMouseCursor(MouseMove);

                                // Move the window to the front if it isn't already on top of all other windows
                                if (WindowList.IndexOf(window) != WindowList.Count)
                                {
                                    MoveListItemToIndex(WindowList, WindowList.IndexOf(window), WindowList.Count);
                                    ActiveWindow = window;
                                }

                                // Set the window location
                                window.WindowLocation.X = (int)MouseX - offsetX;
                                window.WindowLocation.Y = (int)MouseY - offsetY;

                                // Draw the screen
                                Draw();

                                // Close the os menu if it's open
                                ShowMenu = false;
                            }

                            // Set the mouse cursor image
                            ChangeMouseCursor(MouseNormal);
                        }

                        // If the mouse cursor is over the window's title bar and the left mouse button is NOT down,
                        // we can change the mouse icon
                        if (window.IsHoveringOverTitlebar())
                        {
                            // Set the mouse cursor image
                            ChangeMouseCursor(MouseMove);
                        }

                        // If the mouse cursor is over the window's close button, then we can close it
                        if (window.IsClosable() && MouseManager.MouseState == MouseState.Left)
                        {
                            // Remove the window from the list
                            WindowList.Remove(window);
                            if (WindowList.Count > 0)
                            {
                                ActiveWindow = WindowList[WindowList.Count - 1];
                            }
                            else
                            {
                                ActiveWindow = null;
                            }
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    // If we encountered an error, panic and hang
                    KernelPanic.Panic(ex.Message, ex.HResult.ToString());
                }
            }
        }
    }
}
