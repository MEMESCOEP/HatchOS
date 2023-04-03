/* CREDITS */
// HatchOS, by Andrew Maney
// Licensed under the MIT License
// Credits last updated 4-3-23

/* DIRECTIVES */
using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.System;
using Cosmos.Core.Memory;
using Sys = Cosmos.System;
using Cosmos.System.Audio;
using Cosmos.System.Audio.IO;
using Cosmos.HAL.Drivers.PCI.Audio;
using Cosmos.System.Audio.DSP.Processing;
using IL2CPU.API.Attribs;
using PrismGraphics;
using static HatchOS.HelperFunctions;

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

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.BootSound.wav")]
        static byte[] BootSoundAudio;

        [ManifestResourceStream(ResourceName = "HatchOS.Resources.PowerGradient.bmp")]
        static byte[] PowerGradientData;

        // Canvas
        public static PrismGraphics.Extentions.VMWare.SVGAIICanvas canvas;

        // Bitmaps
        public static Image Wallpaper;
        public static Image Mouse;
        public static Image MouseNormal;
        public static Image MouseLink;
        public static Image MouseMove;
        public static Image OSLogo;
        public static Image CloseWindow;
        public static Image PowerGradientBG;

        // Color(s)
        public static PrismGraphics.Color TaskBarColor;
        public static PrismGraphics.Color LoadingColor;

        // Audio mixer(s)
        public static AudioMixer mixer = new AudioMixer();

        // Booleans
        public static bool ShowMenu = false;
        public static bool Debug = true;
        public static bool Booted = false;
        public static bool MouseMoved = false;
        private static bool EnableBootScreen = false;

        // Unsigned integers
        public static uint MouseX, MouseY, BootTimer, LoadPos = 160, OldMouseX, OldMouseY, AvailableRam;

        // Floating point numbers
        public static float AudioVolume = 0.25f; // Max value should be 1f (or 100 percent volume; values range from 0f to 1f)

        // Random
        public static Random random = new Random();

        // Timer(s)
        public static Timer HeapCollectionTimer; // = new PIT.PITTimer(TimerCallback, MillisecondsToNanoseconds(250), true);

        // Lists
        public static List<Window> WindowList = new List<Window>();

        // Integers
        public static int ScreenWidth = 1024;
        public static int ScreenHeight = 768;
        public static int LoadPosSegment = ScreenWidth / 5;
        public static int ColorR = 0;
        public static int ColorG = 0;
        public static int ColorB = 0;
        public static int GCCount = 0;
        /*public static int RTCDay;
        public static int RTCMonth;
        public static int RTCYear;*/

        // Window(s)
        public static Window ActiveWindow;

        /* FUNCTIONS */
        // Executed once upon system startup
        protected override void BeforeRun()
        {
            System.Console.Write("Starting HatchOS...");

            if (CPU.GetAmountOfRAM() + 2 <= 107)
            {
                System.Console.WriteLine("\n[ERROR] Not enough RAM for HatchOS to run!");
                System.Console.Clear();
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("[================================ KERNEL PANIC ================================]WARNING!\nYou do not have enough RAM to run HatchOS!\nAt least 108 MB is required, but you only have " + CPU.GetAmountOfRAM() + 2 + "MB.\n");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.SetCursorPosition(0, 21);
                System.Console.Write("\n\n\nPress any key to shut down.");
                System.Console.ReadKey(true);
                PowerFunctions.Shutdown();
            }

            // Assign bitmaps (using embedded resources)
            PowerGradientBG = Image.FromBitmap(PowerGradientData);
            CloseWindow = Image.FromBitmap(CloseWindowData);
            MouseNormal = Image.FromBitmap(MouseNormalData);
            Wallpaper = Image.FromBitmap(WallpaperData);
            MouseLink = Image.FromBitmap(MouseLinkData);
            MouseMove = Image.FromBitmap(MouseMoveData);
            OSLogo = Image.FromBitmap(LogoData);

            // Set the mouse cursor icon
            Mouse = MouseNormal;

            // Assign color(s)
            TaskBarColor = PrismGraphics.Color.GetPacked(255, 123, 150, 149);

            // Assign unsigned integers
            AvailableRam = (uint)GCImplementation.GetAvailableRAM() * 1024;

            // If we aren't running in VMWare, print a warning, wait for the user to press a key
            if (!VMTools.IsVMWare)
            {
                System.Console.WriteLine("\n[WARN] HatchOS is running outside of VMWare.");
                System.Console.Clear();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("[================================= DISCLAIMER =================================]WARNING!\nYou are running HatchOS in an enviornment that is not virtualized, or\nthe vm you're using isn't VMWare. You will most likely run into\nperformance and/or stability issues!\nHatchOS may work under the current enviornment, or it may not.\nThere is no garuntee.\nThere is also a risk for data loss, so you'll need to be extremely careful\nwhen you run this OS.\n");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.SetCursorPosition(0, 21);
                System.Console.Write("\nPress C to accept this disclaimer and continue\n     or\nPress S to shut down.");
                while (true)
                {
                    var key = System.Console.ReadKey(true);
                    if (key.Key == ConsoleKey.S)
                        PowerFunctions.Shutdown();

                    if (key.Key == ConsoleKey.C)
                        break;
                }
                //System.Console.Clear();
            }

            // Attempt to create a canvas with the specified width and height
            try
            {
                canvas = new((ushort)ScreenWidth, (ushort)ScreenHeight);
            }
            catch
            {
                DisplayConsoleError("\n[ERROR] Failed to create a canvas!\nPress any key to reboot.");
                System.Console.ReadKey(true);
                PowerFunctions.Restart();
            }

            // If the canvas is null, do the following:
            //      1. Print an error
            //      2. Wait for the user to press a key
            //      3. Reboot the system
            if (canvas == null || canvas.Width <= 0 || canvas.Height <= 0)
            {
                DisplayConsoleError("\n[ERROR] Failed to create a canvas!\nPress any key to reboot.");
                System.Console.ReadKey(true);
                PowerFunctions.Restart();
            }

            // Set mouse properties
            DisplayConsoleMsg("[INFO] >> Setting mouse properties...");
            MouseManager.MouseSensitivity = 1f;
            MouseManager.ScreenWidth = (uint)ScreenWidth;
            MouseManager.ScreenHeight = (uint)ScreenHeight;
            MouseManager.X = ((uint)ScreenWidth / 2) - (uint)(Mouse.Width / 2);
            MouseManager.Y = ((uint)ScreenHeight / 2) - ((uint)Mouse.Height / 2);
            MouseX = ((uint)ScreenWidth / 2) - ((uint)Mouse.Width / 2);
            MouseY = ((uint)ScreenHeight / 2) - ((uint)Mouse.Height / 2);

            // Register the PIT timer
            //DisplayConsoleMsg("[INFO] >> Registering PIT timer...");
            //Cosmos.HAL.Global.PIT.RegisterTimer(Timer);

            // Assign timers and propereties
            //HeapCollectionTimer = new Timer((object? O) => Heap.Collect(), null, 0, 1000);

            // Instantiate the API
            //DisplayConsoleMsg("[INFO] >> Instantiating the HatchOS API...");
            //API api = new API();

            // Display the boot screen (if the "EnableBootScreen" boolean is set to true)
            if (EnableBootScreen)
            {
                canvas.DrawImage(0, 0, PowerGradientBG);
                canvas.DrawImage((int)(ScreenWidth / 2 - OSLogo.Width / 2), (int)(ScreenHeight / 2 - OSLogo.Height / 2), OSLogo);
                canvas.DrawString(ScreenWidth / 2 - (21 * 8) / 2, 320, "HatchOS is booting...", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.White);
                canvas.Update();
            }

            DisplayConsoleMsg("[INFO] >> Waiting for canvas to finish initializing...");

            // Try to play the startup sound. This will only work if there's an AC97 audio card (or compatible) in the system
            try
            {
                var driver = AC97.Initialize(4096);
                var mixer = new AudioMixer();
                var audioStream = MemoryAudioStream.FromWave(BootSoundAudio);
                audioStream.PostProcessors.Add(new GainPostProcessor(AudioVolume));
                mixer.Streams.Add(audioStream);
                var audioManager = new AudioManager()
                {
                    Stream = mixer,
                    Output = driver
                };
                audioManager.Enable();
            }
            catch (Exception ex)
            {
                // If the exception is not an invalid operation, return to console mode
                if (ex is not InvalidOperationException)
                {
                    canvas.Dispose();
                }
                DisplayConsoleError($"[ERROR] Failed to play the audio file! {ex.Message}");
            }
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
                ShowGUI();
            }
        }

        // Draw everything
        public void Draw()
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
                canvas.DrawFilledRectangle(0, ScreenHeight - 40, (ushort)ScreenWidth, 40, 0, TaskBarColor);

                // If the OS menu should be visible, draw it
                if (ShowMenu)
                {
                    // Menu background
                    canvas.DrawFilledRectangle(0, ScreenHeight / 2, 170, (ushort)Math.Abs(ScreenHeight / 2 - (ScreenHeight - 40)), 0, PrismGraphics.Color.GetPacked(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B));

                    // Shutdown button
                    canvas.DrawFilledRectangle(12, ScreenHeight - 78, 64, 20, 0, PrismGraphics.Color.LightGray);
                    canvas.DrawString(13, ScreenHeight - 78, "Exit OS", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);

                    // Restart button
                    canvas.DrawFilledRectangle(87, ScreenHeight - 78, 64, 20, 0, PrismGraphics.Color.LightGray);
                    canvas.DrawString(88, ScreenHeight - 78, "Restart", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                }

                /*if(GCImplementation.GetAvailableRAM() - GCImplementation.GetUsedRAM() / (1024 * 1024) <= 25 && Debug)
                {
                    // Error background
                    canvas.DrawFilledRectangle(ScreenWidth / 2 - 320 / 2, ScreenHeight / 2, 320, (ushort)Math.Abs(ScreenHeight / 2 - 40), 0, PrismGraphics.Color.GetPacked(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B));

                    // Shutdown button
                    canvas.DrawFilledRectangle(12, ScreenHeight - 78, 64, 20, 0, PrismGraphics.Color.LightGray);
                    canvas.DrawString(13, ScreenHeight - 78, "Exit OS", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);

                    // Restart button
                    canvas.DrawFilledRectangle(87, ScreenHeight - 78, 64, 20, 0, PrismGraphics.Color.LightGray);
                    canvas.DrawString(88, ScreenHeight - 78, "Restart", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                }*/

                // Draw task bar elements (Time, Date, and OS Logo)
                canvas.DrawImage(8, ScreenHeight - 36, OSLogo);
                canvas.DrawString(ScreenWidth - 70, ScreenHeight - 35, RTC.Hour + ":" + RTC.Minute + ":" + RTC.Second, PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                canvas.DrawString(ScreenWidth - 70, ScreenHeight - 20, RTC.Month + ":" + RTC.DayOfTheMonth + ":" + RTC.Year, PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);

                // If the debug text should be visible, draw it
                if (Debug)
                {
                    canvas.DrawString(0, 0, "POS=(X=" + MouseX + ", Y=" + MouseY + ")", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Red);
                    canvas.DrawString(0, 12, "RAM=" + GCImplementation.GetUsedRAM() / 1024 + "/" + AvailableRam + " KB", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Red);
                    canvas.DrawString(0, 24, "FPS=" + canvas.GetFPS(), PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Red);
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
            }
            catch
            {

            }
        }

        public void ShowGUI()
        {   
            // Create the welcome window
            WindowManager.CreateNewWindow(WindowList, new Point(ScreenWidth / 2 - 240 / 2, ScreenHeight / 2 - 130), new Point(240, 130), new List<PrismGraphics.Color> { PrismGraphics.Color.GetPacked(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), PrismGraphics.Color.LightGray, PrismGraphics.Color.Black, PrismGraphics.Color.GetPacked(255, 40, 65, 65) }, "Welcome to HatchOS!");
            WindowElement WelcomeText = new WindowElement();
            WelcomeText.ElementType = "StringElement";
            WelcomeText.ElementData = "Welcome to HatchOS!";
            WelcomeText.ElementPosition = new Point(5, 0);
            WindowList[0].WindowElements.Add(WelcomeText);

            WindowElement AuthorText = new WindowElement();
            AuthorText.ElementType = "StringElement";
            AuthorText.ElementData = "Made by Andrew Maney";
            AuthorText.ElementPosition = new Point(5, 12);
            WindowList[0].WindowElements.Add(AuthorText);

            WindowElement VersionText = new WindowElement();
            VersionText.ElementType = "StringElement";
            VersionText.ElementData = "ALPHA 4-3-23";
            VersionText.ElementPosition = new Point(5, 24);
            WindowList[0].WindowElements.Add(VersionText);
            Draw();
            Debug = false;

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
                            int offsetX = (int)FindDifference(window.WindowLocation.X, MouseX);
                            int offsetY = (int)FindDifference(window.WindowLocation.Y, MouseY);

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
