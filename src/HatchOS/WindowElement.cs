using System.Drawing;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;

namespace HatchOS
{
    public class WindowElement
    {
        public string ElementType = "StringElement";
        public string ElementData;
        public Color ElementColor = Color.Black;
        public Point ElementPosition = new Point(0, 0);
    }
}
