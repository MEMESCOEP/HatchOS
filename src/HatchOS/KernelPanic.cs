/* DIRECTIVES */
using Cosmos.HAL;
using System;
using static HatchOS.HelperFunctions;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    internal class KernelPanic
    {
        /* FUNCTIONS */
        // Panic and halt the system
        public static void Panic(string Message, string ErrCode)
        {
            // Make sure everything is defined
            if (string.IsNullOrEmpty(Message))
                Message = "Unknown Error";

            if (string.IsNullOrEmpty(ErrCode))
                ErrCode = "Unknown Error Code";

            // Send a panic message via the serial port
            SerialPort.SendString($"[ERROR] >> HatchOS KERNEL PANIC:\n\tMSG=\"{Message}\"\n\tCODE={ErrCode}\n\n", SerialPort.COM1);

            // Draw the panic screen
            try
            {
                Kernel.canvas.Clear(PrismGraphics.Color.Red);
                Kernel.canvas.DrawString(0, 0, "[===== KERNEL PANIC =====]", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                Kernel.canvas.DrawString(0, 16, "CODE: " + ErrCode, PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                Kernel.canvas.DrawString(0, 32, "MESSAGE: " + Message, PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                Kernel.canvas.DrawString(0, Kernel.ScreenHeight - 16, "<PRESS ANY KEY TO REBOOT>", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.Black);
                Kernel.canvas.Update();

                Console.ReadKey();
                Kernel.canvas.Clear(PrismGraphics.Color.Black);
                Kernel.canvas.DrawString(0, 0, "REBOOTING...", PrismGraphics.Fonts.Font.Fallback, PrismGraphics.Color.White);
                Kernel.canvas.Update();
                PowerFunctions.Restart();
            }
            catch(Exception EX2)
            {
                DisplayConsoleError($"[ERROR] >> PANIC DISP ERR: {EX2.Message}, {EX2.HResult}\n\n");
            }
        }
    }
}
