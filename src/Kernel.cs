/* DIRECTIVES */
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.System;
using Sys = Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Audio;
using Cosmos.System.Audio.IO;
using Cosmos.HAL.Drivers.PCI.Audio;
using IL2CPU.API.Attribs;
using static HatchOS.HelperFunctions;
using Cosmos.System.Audio.DSP.Processing;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class Kernel : Sys.Kernel
    {
        /* VARIABLES */
        // Embedded resources
        [ManifestResourceStream(ResourceName = "HatchOS.Art.Wallpapers.BG_1280x1024.bmp")]
        static byte[] WallpaperData;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Icons.mouse.bmp")]
        static byte[] MouseNormalData;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Icons.xp_link.bmp")]
        static byte[] MouseLinkData;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Icons.xp_move.bmp")]
        static byte[] MouseMoveData;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Icons.OS_Logo.bmp")]
        static byte[] LogoData;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Icons.close_window.bmp")]
        static byte[] CloseWindowData;

        [ManifestResourceStream(ResourceName = "HatchOS.Audio.BootSound.wav")]
        static byte[] BootSoundAudio;

        [ManifestResourceStream(ResourceName = "HatchOS.Art.Wallpapers.PowerGradient.bmp")]
        static byte[] PowerGradientData;

        // Canvas
        public static Canvas canvas;

        // Bitmaps
        public static Bitmap Wallpaper;
        public static Bitmap Mouse;
        public static Bitmap MouseNormal;
        public static Bitmap MouseLink;
        public static Bitmap MouseMove;
        public static Bitmap OSLogo;
        public static Bitmap CloseWindow;
        public static Bitmap PowerGradientBG;

        // Color(s)
        public static Color TaskBarColor;
        public static Color LoadingColor;

        // Audio mixer(s)
        public static AudioMixer mixer = new AudioMixer();

        // Booleans
        public static bool ShowMenu = false;
        public static bool Debug = false;
        public static bool Booted = false;
        public static bool MouseMoved = false;

        // Unsigned integers
        public static uint MouseX, MouseY, BootTimer, LoadPos = 160, OldMouseX, OldMouseY;

        // Floating point numbers
        public static float AudioVolume = 0.5f; // Max value is 1f (or 100 percent volume)

        // Random
        public static Random random = new Random();

        // PIT Timer(s)
        public static PIT.PITTimer Timer = new PIT.PITTimer(TimerCallback, MillisecondsToNanoseconds(250), true);

        // Lists
        public static List<Window> WindowList = new List<Window>();

        // Integers
        public static int ScreenWidth = 1024, ScreenHeight = 768, LoadPosSegment = ScreenWidth / 5, ColorR = 0, ColorG = 0, ColorB = 0;

        // Window(s)
        public static Window ActiveWindow;

        /* FUNCTIONS */
        // Callback funtion for when the PIT timer ticks; Just a simple loading bar
        public static void TimerCallback()
        {
            LoadingColor = Color.FromArgb(ColorR, ColorG, ColorB);
            BootTimer++;
            canvas.DrawFilledRectangle(LoadingColor, 0, ScreenHeight / 2 + 200, (int)LoadPos, 20);
            canvas.DrawString("###############", PCScreenFont.Default, LoadingColor, (int)LoadPos - LoadPosSegment, ScreenHeight / 2 + 200);
            LoadPos += (uint)LoadPosSegment;

            if (BootTimer >= 5)
            {
                Timer.Recurring = false;
                Timer = null;
                Booted = true;
            }

            ColorR += 51;
            ColorG += 51;
            ColorB += 51;

            canvas.Display();
        }

        // Executed once upon system startup
        protected override void BeforeRun()
        {
            System.Console.Write("Starting HatchOS...");

            // Assign bitmaps using embedded resources
            Wallpaper = new Bitmap(WallpaperData);
            MouseNormal = new Bitmap(MouseNormalData);
            MouseLink = new Bitmap(MouseLinkData);
            MouseMove = new Bitmap(MouseMoveData);
            OSLogo = new Bitmap(LogoData);
            CloseWindow = new Bitmap(CloseWindowData);
            PowerGradientBG = new Bitmap(PowerGradientData);

            // Set the mouse cursor icon
            Mouse = MouseNormal;

            // Assign color(s)
            TaskBarColor = Color.FromArgb(255, 123, 150, 149);

            // If we aren't running in VMWare, print a warning, wait for the user to press a key, and continue
            if (!VMTools.IsVMWare)
            {
                System.Console.WriteLine("\n[WARN] HatchOS is running outside of VMWare.");
                System.Console.Clear();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("[================================= DISCLAIMER =================================]WARNING!\nYou are running HatchOS in an enviornment that is not virtualized, or\nthe enviornment you're using is not VMWare. You will most likely run into\nperformance and/or stability issues!\nHatchOS may work under the current enviornment, or it may not.\nThere is no garuntee that HatchOS will work.\nThere is also a risk for data loss, so you'll need to be extremely careful\nwhen you run this OS.\n");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.SetCursorPosition(0, 21);
                System.Console.Write("\n\n\nPress any key to accept this disclaimer and continue...");
                System.Console.ReadKey();
                System.Console.Clear();
            }

            // Attempt to create a canvas with the specified width, height, and color depth
            FullScreenCanvas.TryGetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32), out canvas);

            // If the canvas is null, print an error, wait for the user to press a key, and reboot the system
            if (canvas == null)
            {
                System.Console.Write("\n[ERROR] Failed to create a canvas!\nPress any key to reboot.");
                System.Console.ReadKey();
                PowerFunctions.Restart();
            }

            // If the canvas type is VGACanvas, print an error, wait for the user to press a key, and reboot the system.
            // VGA is far too slow currently, so it's not worth using.
            else if (canvas.Name() == "VGACanvas")
            {
                canvas.Disable();
                System.Console.Write("\n[ERROR] Invalid canvas type 'VGACanvas'!\nPress any key to reboot.");
                System.Console.ReadKey();
                PowerFunctions.Restart();
            }

            // Print the canvas type
            else
            {
                System.Console.WriteLine($"\n[INFO] >> Using canvas: '{canvas.Name()}'");
                SerialPort.SendString($"[INFO] >> Using canvas: \"{canvas.Name()}\"\n", SerialPort.COM1);
            }

            // Set mouse properties
            SerialPort.SendString("[INFO] >> Setting mouse properties...\n", SerialPort.COM1);
            MouseManager.MouseSensitivity = 1;
            MouseManager.ScreenWidth = (uint)ScreenWidth;
            MouseManager.ScreenHeight = (uint)ScreenHeight;
            MouseManager.X = (uint)ScreenWidth / 2 - Mouse.Width;
            MouseManager.Y = (uint)ScreenHeight / 2 - Mouse.Height;
            MouseX = (uint)ScreenWidth / 2 - Mouse.Width;
            MouseY = (uint)ScreenHeight / 2 - Mouse.Height;

            // Register the PIT timer
            SerialPort.SendString("[INFO] >> Registering PIT timer...\n", SerialPort.COM1);
            Cosmos.HAL.Global.PIT.RegisterTimer(Timer);

            // Display the boot screen
            canvas.DrawImage(PowerGradientBG, 0, 0);
            canvas.DrawImageAlpha(OSLogo, (int)(ScreenWidth / 2 - OSLogo.Width / 2), (int)(ScreenHeight / 2 - OSLogo.Height / 2));
            canvas.DrawString("HatchOS is booting...", PCScreenFont.Default, Color.White, ScreenWidth / 2 - (21 * 8) / 2, 320);
            SerialPort.SendString("[INFO] >> HatchOS is booting...\n", SerialPort.COM1);
            canvas.Display();

            // Try to play the startup sound (Note that this will only work if there's an AC97 audio card in the system)
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
                    canvas.Disable();
                }
                DisplayConsoleError($"\n[ERROR] Failed to play the audio file! {ex.Message}");
            }
        }

        // Executed every frame after BeforeRun() is finished
        protected override void Run()
        {
            // Once the boot process is finished, show the desktop
            if (Booted)
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
                canvas.DrawImage(Wallpaper, 0, 0);

                // Draw each window in the window list
                foreach (Window window in WindowList)
                {
                    window.ActiveWindow = (ActiveWindow == window);
                    window.DrawWindow();
                }

                // Draw the taskbar
                canvas.DrawFilledRectangle(TaskBarColor, 0, ScreenHeight - 40, ScreenWidth, 40);

                // If the menu should be visible, draw it
                if (ShowMenu)
                {
                    // Menu background
                    canvas.DrawFilledRectangle(Color.DarkSlateGray, 0, ScreenHeight / 2, 170, Math.Abs(ScreenHeight / 2 - (ScreenHeight - 40)));

                    // Shutdown button
                    canvas.DrawFilledRectangle(Color.SlateGray, 12, ScreenHeight - 78, 64, 20);
                    canvas.DrawString("Exit OS", PCScreenFont.Default, Color.Black, 13, ScreenHeight - 78);

                    // Restart button
                    canvas.DrawFilledRectangle(Color.SlateGray, 87, ScreenHeight - 78, 64, 20);
                    canvas.DrawString("Restart", PCScreenFont.Default, Color.Black, 88, ScreenHeight - 78);
                }

                // Draw task bar elements
                canvas.DrawImageAlpha(OSLogo, 8, ScreenHeight - 36);
                canvas.DrawString($"{RTC.Hour}:{RTC.Minute}:{RTC.Second}", PCScreenFont.Default, Color.Black, ScreenWidth - 70, ScreenHeight - 35);
                canvas.DrawString($"{RTC.Month}:{RTC.DayOfTheMonth}:{RTC.Year}", PCScreenFont.Default, Color.Black, ScreenWidth - 70, ScreenHeight - 20);

                // If the debug text should be visible, draw it
                if (Debug)
                {
                    canvas.DrawString($"{MouseX},{MouseY} || {GCImplementation.GetUsedRAM() / (1024 * 1024)} MB / {GCImplementation.GetAvailableRAM()} MB", PCScreenFont.Default, Color.Red, 0, 0);
                }

                // Draw the mouse cursor
                canvas.DrawImageAlpha(Mouse, (int)MouseX, (int)MouseY);

                // Since we're using a double-buffered driver, we need to copy the second (non-visible) framebuffer
                // into the first (visible) framebuffer. This ensures that there is no flickering and displays
                // the contents of the first framebuffer on screen
                canvas.Display();

                // Handle keyboard/mouse input
                Input.HandleInput();

                // Call the garbage collector so we don't have as many memory leaks, which also
                // helps improve framerates
                Cosmos.Core.Memory.Heap.Collect();
            }
            catch
            {

            }
        }

        public void ShowGUI()
        {   
            // Create the welcome window
            WindowManager.CreateNewWindow(WindowList, new Point(0, 0), new Point(320, 200), new List<Color> { Color.DarkSlateGray, Color.DimGray, Color.Black, Color.FromArgb(255, 40, 65, 65) }, "Welcome to HatchOS!");
            
            // Display loop
            while (true)
            {
                // Attempt to draw (for stability reasons)
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
                                window.WindowLocation.X = ((int)MouseX - offsetX);
                                window.WindowLocation.Y = ((int)MouseY - offsetY);

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
