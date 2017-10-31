using Pelco.PDK.Metadata.UI.Overlays;
using System.Windows;
using System.Windows.Media;

namespace Pelco.Metadata.UI.Overlays
{
    public class EllipseOverlay : RectangleOverlay
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EllipseOverlay() : base()
        {
        }

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="translator">Point Translator used to translate the normalized point to the actual point</param>
        public override void Draw(DrawingContext context, IPointTranslator translator)
        {
            Point upperLeft = translator.TranslatePoint(UpperLeft);
            Point bottomRight = translator.TranslatePoint(BottomRight);
            Point center = new Point((bottomRight.X + upperLeft.X) / 2.0, (bottomRight.Y + upperLeft.Y) / 2.0);

            if (translator.IsPointInBounds(upperLeft))
            {

                double radiusX = (bottomRight.X - upperLeft.X) / 2.0;
                double radiusY = (bottomRight.Y - upperLeft.Y) / 2.0;

                context.DrawEllipse(null,
                                    new Pen(new SolidColorBrush(BorderColor), STROKE_THICKNESS),
                                    center,
                                    radiusX,
                                    radiusY);
            }
        }
    }
}
