/* DIRECTIVES */
using Cosmos.HAL;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Drawing;
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
                Kernel.canvas.Clear(Color.Red);
                Kernel.canvas.DrawString("[===== KERNEL PANIC =====]", PCScreenFont.Default, Color.Black, 0, 0);
                Kernel.canvas.DrawString("CODE: " + ErrCode, PCScreenFont.Default, Color.Black, 0, 16);
                Kernel.canvas.DrawString("MESSAGE: " + Message, PCScreenFont.Default, Color.Black, 0, 32);
                Kernel.canvas.DrawString("<PRESS ANY KEY TO REBOOT>", PCScreenFont.Default, Color.Black, 0, Kernel.ScreenHeight - 16);
                Kernel.canvas.Display();

                Console.ReadKey();
                Kernel.canvas.Clear(Color.Black);
                Kernel.canvas.DrawString("REBOOTING...", PCScreenFont.Default, Color.White, 0, 0);
                Kernel.canvas.Display();
                PowerFunctions.Restart();
            }
            catch(Exception EX2)
            {
                DisplayConsoleError($"[ERROR] >> PANIC DISP ERR: {EX2.Message}, {EX2.HResult}\n\n");
            }
        }
    }
}
