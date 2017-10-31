using Pelco.PDK.Metadata.UI.Overlays;
using System.Windows;
using System.Windows.Media;

namespace Pelco.Metadata.UI.Overlays
{
    public class RectangleOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RectangleOverlay() : base()
        {
        }

        /// <summary>
        /// Gets and sets the rectangle's point for the upper left corner. Points are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point UpperLeft { get; set; }

        /// <summary>
        /// Gets and sets the rectangle's point for the bottom right corner. oints are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point BottomRight { get; set; }

        /// <summary>
        /// Gets and sets the color used to draw the rectangle's border
        /// </summary>
        public Color BorderColor { get; set; } = Colors.Transparent;

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="translator">Point Translator used to translate the normalized point to the actual point</param>
        public override void Draw(DrawingContext context, IPointTranslator translator)
        {
            var upTrans = translator.TranslatePoint(UpperLeft);
            var brTrans = translator.TranslatePoint(BottomRight);

            if (translator.IsPointInBounds(upTrans))
            {
                context.DrawRectangle(null,
                                      new Pen(new SolidColorBrush(BorderColor), STROKE_THICKNESS),
                                      new Rect(upTrans, brTrans));
            }
        }
    }
}
