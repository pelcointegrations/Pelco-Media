using Pelco.PDK.Metadata.UI.Overlays;
using System.Windows;
using System.Windows.Media;

namespace Pelco.Metadata.UI.Overlays
{
    public class LineOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LineOverlay() : base()
        {
        }

        /// <summary>
        /// Gets and sets the line's start cooridiante point.  Points are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Gets and sets the line's end cooridinate point. Points are represented as
        /// normalized points withing the range 0-1.
        /// </summary>
        public Point EndPoint { get; set; }

        /// <summary>
        /// Gets and set the color used to draw the line
        /// </summary>
        public Color LineColor { get; set; } = Colors.Blue;

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="translator">Point Translator used to translate the normalized point to the actual point</param>
        public override void Draw(DrawingContext context, IPointTranslator translator)
        {
            Point startPoint = translator.TranslatePoint(StartPoint);
            Point endPoint = translator.TranslatePoint(EndPoint);

            context.DrawLine(new Pen(new SolidColorBrush(LineColor), STROKE_THICKNESS),
                             startPoint,
                             endPoint);
        }
    }
}
