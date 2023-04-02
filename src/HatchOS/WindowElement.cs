using System.Drawing;

namespace HatchOS
{
    public class WindowElement
    {
        public string ElementType = "StringElement";
        public string ElementData;
        public PrismGraphics.Color ElementColor = PrismGraphics.Color.Black;
        public Point ElementPosition = new Point(0, 0);
    }
}
