/* DIRECTIVES */
using System.Drawing;
using System.Collections.Generic;
using static HatchOS.HelperFunctions;
using Cosmos.HAL;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class WindowManager
    {
        /* FUNCTIONS */
        // Create a new window with the specified location, size, colors, and title
        public static void CreateNewWindow(List<Window> WindowList, Point Location, Point Size, List<Color> Colors, string Title)
        {
            Window window = new Window();
            window.WindowTitle = Title;
            window.WindowColors = Colors;
            window.WindowSize = Size;
            window.WindowLocation = Location;
            Kernel.ActiveWindow = window;
            WindowList.Add(window);
            MoveListItemToIndex(WindowList, WindowList.IndexOf(window), WindowList.Count);
            SerialPort.SendString($"[INFO] >> Created window \"{Title}\" with dimensions {Size.X}x{Size.Y}\n", SerialPort.COM1);
        }
    }
}
