/* DIRECTIVES */
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Drawing;
using static HatchOS.HelperFunctions;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    internal class PowerFunctions
    {
        /* FUNCTIONS */
        // Draw the power menu and the selected option
        public static void DrawPowerMenu(Canvas canvas, int Option)
        {
            canvas.DrawImage(Kernel.PowerGradientBG, 0, 0);
            canvas.DrawString("[== CHOOSE A POWER OPTION ==]", PCScreenFont.Default, Color.White, 0, 0);
            canvas.DrawString("Press ESCAPE to return to the desktop", PCScreenFont.Default, Color.White, 0, Kernel.ScreenHeight - 16);

            if (Option == 0)
            {
                canvas.DrawFilledRectangle(Color.White, 0, 16, 128, 16);
                canvas.DrawString("1. Shut down", PCScreenFont.Default, Color.Black, 0, 16);
                canvas.DrawString("2. Reboot", PCScreenFont.Default, Color.White, 0, 32);
                canvas.DrawString("3. ACPI", PCScreenFont.Default, Color.White, 0, 48);
            }

            else if (Option == 1)
            {
                canvas.DrawFilledRectangle(Color.White, 0, 32, 128, 16);
                canvas.DrawString("1. Shut down", PCScreenFont.Default, Color.White, 0, 16);
                canvas.DrawString("2. Reboot", PCScreenFont.Default, Color.Black, 0, 32);
                canvas.DrawString("3. ACPI", PCScreenFont.Default, Color.White, 0, 48);
            }
            else
            {
                canvas.DrawFilledRectangle(Color.White, 0, 48, 128, 16);
                canvas.DrawString("1. Shut down", PCScreenFont.Default, Color.White, 0, 16);
                canvas.DrawString("2. Reboot", PCScreenFont.Default, Color.White, 0, 32);
                canvas.DrawString("3. ACPI", PCScreenFont.Default, Color.Black, 0, 48);
            }

            canvas.Display();
        }

        // Show the power menu and let the user make a selection
        public static void PowerOff(Canvas canvas, string mode)
        {
            // Display the power menu
            if(mode == "-sr")
            {
                int Option = 0;

                DrawPowerMenu(canvas, Option);

                // Wait for the user to select an option
                while (true)
                {
                    try
                    {
                        // Try to read a key frm the keyboard
                        KeyboardManager.TryReadKey(out var key);

                        // Call the garbage collector so we don't have as many memory leaks
                        Cosmos.Core.Memory.Heap.Collect();

                        // If the up arrow kes is pressed, change the power option
                        if (key.Key == ConsoleKeyEx.UpArrow)
                        {
                            Option--;
                            if (Option < 0)
                            {
                                Option = 2;
                            }

                            DrawPowerMenu(canvas, Option);
                        }

                        // If the down arrow kes is pressed, change the power option
                        if (key.Key == ConsoleKeyEx.DownArrow)
                        {
                            // Draw the power menu
                            Option++;
                            if(Option > 2)
                            {
                                Option = 0;
                            }

                            DrawPowerMenu(canvas, Option);
                        }

                        // If the enter key is pressed, shut down or reboot the system
                        if (key.Key == ConsoleKeyEx.Enter)
                        {
                            if (Option == 0)
                            {
                                PowerOff(canvas, "-s");
                            }
                            else if(Option == 1)
                            {
                                PowerOff(canvas, "-r");
                            }
                            else
                            {
                                PowerOff(canvas, "-a");
                            }
                        }

                        // If the escape key is pressed, close the power menu
                        if(key.Key == ConsoleKeyEx.Escape)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        canvas.Clear(Color.Black);
                        canvas.DrawImage(Kernel.PowerGradientBG, 0, 0);
                        canvas.DrawImageAlpha(Kernel.OSLogo, (int)(800 / 2 - Kernel.OSLogo.Width / 2), (int)(Kernel.ScreenHeight / 2 - (Kernel.OSLogo.Height / 2)));
                        canvas.DrawString("HatchOS is shutting down...", PCScreenFont.Default, Color.White, Kernel.ScreenHeight / 2 - ((27 * 8) / 2), (Kernel.ScreenHeight - 2) + 20);
                        canvas.Display();
                        var Timer = new PIT.PITTimer(Shutdown, SecondsToNanoseconds(1), true);
                        Cosmos.HAL.Global.PIT.RegisterTimer(Timer);
                        while (true) ;
                    }
                }
            }

            // Restart the computer
            else if (mode == "-r")
            {
                canvas.Clear(Color.Black);
                canvas.DrawImage(Kernel.PowerGradientBG, 0, 0);
                canvas.DrawImageAlpha(Kernel.OSLogo, (int)(Kernel.ScreenWidth / 2 - Kernel.OSLogo.Width / 2), (int)(Kernel.ScreenHeight / 2 - (Kernel.OSLogo.Height / 2)));
                canvas.DrawString("HatchOS is restarting...", PCScreenFont.Default, Color.White, Kernel.ScreenWidth / 2 - ((24 * 8) / 2), (Kernel.ScreenHeight / 2) + 20);
                canvas.Display();
                var Timer = new PIT.PITTimer(Restart, SecondsToNanoseconds(1), true);
                Cosmos.HAL.Global.PIT.RegisterTimer(Timer);
                while (true) ;
            }

            // Restart the computer using ACPI
            else if (mode == "-a")
            {
                canvas.Clear(Color.Black);
                canvas.DrawImage(Kernel.PowerGradientBG, 0, 0);
                canvas.DrawImageAlpha(Kernel.OSLogo, (int)(Kernel.ScreenWidth / 2 - Kernel.OSLogo.Width / 2), (int)(Kernel.ScreenHeight / 2 - (Kernel.OSLogo.Height / 2)));
                canvas.DrawString("HatchOS is restarting (ACPI)...", PCScreenFont.Default, Color.White, Kernel.ScreenWidth / 2 - ((31 * 8) / 2), (Kernel.ScreenHeight / 2) + 20);
                canvas.Display();
                var Timer = new PIT.PITTimer(RebootACPI, SecondsToNanoseconds(1), true);
                Cosmos.HAL.Global.PIT.RegisterTimer(Timer);
                while (true) ;
            }

            // Turn off the computer
            else if (mode == "-s")
            {
                canvas.Clear(Color.Black);
                canvas.DrawImage(Kernel.PowerGradientBG, 0, 0);
                canvas.DrawImageAlpha(Kernel.OSLogo, (int)(Kernel.ScreenWidth / 2 - Kernel.OSLogo.Width / 2), (int)(Kernel.ScreenHeight / 2 - (Kernel.OSLogo.Height / 2)));
                canvas.DrawString("HatchOS is shutting down...", PCScreenFont.Default, Color.White, Kernel.ScreenWidth / 2 - ((27 * 8) / 2), (Kernel.ScreenHeight / 2) + 20);
                canvas.Display();
                var Timer = new PIT.PITTimer(Shutdown, SecondsToNanoseconds(1), true);
                Cosmos.HAL.Global.PIT.RegisterTimer(Timer);
                while (true) ;
            }
        }

        // Restart the computer
        public static void Restart()
        {
            Cosmos.System.Power.Reboot();
        }

        // Turn off the computer
        public static void Shutdown()
        {
            Cosmos.System.Power.Shutdown();
        }

        // Restart the computer with ACPI
        public static void RebootACPI()
        {
            Cosmos.Core.ACPI.Reboot();
        }
    }
}
