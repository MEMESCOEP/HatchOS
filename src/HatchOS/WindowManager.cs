/* DIRECTIVES */
using System.Drawing;
using System.Collections.Generic;
using static HatchOS.HelperFunctions;
using PrismGraphics;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class WindowManager
    {
        /* VARIABLES */
        public static Graphics TitlebarGradient = Gradient.GetGradient(5, 40, PrismGraphics.Color.Blue, PrismGraphics.Color.Blue);

        /* FUNCTIONS */
        // Create a new window with the specified location, size, colors, and title
        public static void CreateNewWindow(List<Window> WindowList, Point Location, Point Size, List<PrismGraphics.Color> Colors, string Title, bool UseGradient = false)
        {
            Window window = new Window();
            window.WindowTitle = Title;
            window.WindowColors = Colors;
            window.WindowSize = Size;
            window.WindowLocation = Location;
            Kernel.ActiveWindow = window;
            WindowList.Add(window);
            MoveListItemToIndex(WindowList, WindowList.IndexOf(window), WindowList.Count);
            DisplayConsoleMsg("[INFO] >> Created window \"" + Title + "\" with dimensions " + Size.X + "x" + Size.Y);
        }
    }
}
