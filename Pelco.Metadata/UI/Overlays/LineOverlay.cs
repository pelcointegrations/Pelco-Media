using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pelco.PDK.Metadata.UI.Overlays
{
    public class LineOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The id of the overlay</param>
        public LineOverlay(string id) : base(id)
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
        /// Create a new Line based on the <see cref="LineOverlay"/>.
        /// </summary>
        /// <returns>The new line.</returns>
        public override Shape ToShape(Size viewSize)
        {
            Point startPoint = Translate(viewSize, StartPoint);
            Point endPoint = Translate(viewSize, EndPoint);

            var line = new Line()
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = new SolidColorBrush(LineColor),
                StrokeThickness = STROKE_THICKNESS
            };

            return line;
        }

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="viewSize">Size of the view.</param>
        public override void Draw(DrawingContext context, Size viewSize)
        {
            Point startPoint = Translate(viewSize, StartPoint);
            Point endPoint = Translate(viewSize, EndPoint);

            context.DrawLine(new Pen(new SolidColorBrush(LineColor), STROKE_THICKNESS),
                             startPoint,
                             endPoint);
        }
    }
}
