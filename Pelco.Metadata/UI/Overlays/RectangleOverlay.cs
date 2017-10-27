using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pelco.PDK.Metadata.UI.Overlays
{
    public class RectangleOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The id of the overlay</param>
        public RectangleOverlay(string id) : base(id)
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
        /// Gets and sets the color used to fill the rectangle
        /// </summary>
        public Color FillColor { get; set; } = Colors.Transparent;

        /// <summary>
        /// Creates a new <c>Rectangle</c> from the <see cref="RectangleOverlay"/>.
        /// </summary>
        /// <returns>The corresponding <c>Rectangle</c>.</returns>
        public override Shape ToShape(Size viewSize)
        {
            Point upperLeft = Translate(viewSize, UpperLeft);
            Point bottomRight = Translate(viewSize, BottomRight);

            var rect = new Rectangle()
            {
                Width = bottomRight.X - upperLeft.X,
                Height = bottomRight.Y - upperLeft.Y,
                Stroke = new SolidColorBrush(BorderColor),
                StrokeThickness = STROKE_THICKNESS,
                Fill = new SolidColorBrush(FillColor)
            };

            Canvas.SetLeft(rect, upperLeft.X);
            Canvas.SetTop(rect, upperLeft.Y);
            return rect;
        }

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="viewSize">Size of the view.</param>
        public override void Draw(DrawingContext context, Size viewSize)
        {
            Rect rectangle = new Rect(Translate(viewSize, UpperLeft), Translate(viewSize, BottomRight));
            context.DrawRectangle(new SolidColorBrush(FillColor),
                                  new Pen(new SolidColorBrush(BorderColor), STROKE_THICKNESS),
                                  rectangle);
        }
    }
}
