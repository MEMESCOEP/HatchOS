/* DIRECTIVES */
using Cosmos.HAL;
using PrismAPI.Graphics;
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
            // Stop the audio if it wasn't done playing
            if (Kernel.audioDriver.Enabled)
            {
                Kernel.audioDriver.Disable();
            }

            // Make sure everything is defined
            if (string.IsNullOrEmpty(Message))
                Message = "Unknown Error";

            if (string.IsNullOrEmpty(ErrCode))
                ErrCode = "Unknown Error Code";

            // Send a panic message via the serial port
            SerialPort.SendString($"\n\r[ERROR] >> HatchOS KERNEL PANIC:\n\r\tMSG=\"{Message}\"\n\r\tCODE={ErrCode}\n\n\r", COMPort.COM1);

            // Draw the panic screen
            try
            {
                Kernel.canvas.Clear(Color.Red);
                Kernel.canvas.DrawString(0, 0, "[===== KERNEL PANIC =====]", default, Color.Black);
                Kernel.canvas.DrawString(0, 16, "ERROR CODE: " + ErrCode, default, Color.Black);
                Kernel.canvas.DrawString(0, 32, "MESSAGE: " + Message, default, Color.Black);
                Kernel.canvas.DrawString(0, Kernel.ScreenHeight - 16, "<PRESS ANY KEY TO REBOOT>", default, Color.Black);
                Kernel.canvas.Update();

                Console.ReadKey();
                Kernel.canvas.Clear(Color.Black);
                Kernel.canvas.DrawString(0, 0, "REBOOTING...", default, Color.White);
                Kernel.canvas.Update();
                PowerFunctions.Restart();
            }
            catch(Exception EX2)
            {
                DisplayConsoleError($"[ERROR] >> PANIC DISPLAY ERR: {EX2.Message}, {EX2.HResult}\n\n");
            }
        }
    }
}
