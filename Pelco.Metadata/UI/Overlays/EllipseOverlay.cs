using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pelco.PDK.Metadata.UI.Overlays
{
    public class EllipseOverlay : RectangleOverlay
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The id of the overlay</param>
        public EllipseOverlay(string id) : base(id)
        {
        }

        /// <summary>
        /// Creates a new <c>Ellipse</c> from the <see cref="EllipseOverlay"/>.
        /// </summary>
        /// <returns>The corresponding <c>Ellipse</c>.</returns>
        public override Shape ToShape(Size viewSize)
        {
            Point upperLeft = Translate(viewSize, UpperLeft);
            Point bottomRight = Translate(viewSize, BottomRight);

            var ellipse = new Ellipse()
            {
                Width = bottomRight.X - upperLeft.X,
                Height = bottomRight.Y - upperLeft.Y,
                Stroke = new SolidColorBrush(BorderColor),
                StrokeThickness = STROKE_THICKNESS,
                Fill = new SolidColorBrush(FillColor)
            };

            Canvas.SetLeft(ellipse, upperLeft.X);
            Canvas.SetTop(ellipse, upperLeft.Y);
            return ellipse;
        }

        /// <summary>
        /// Draws the shape.
        /// </summary>
        /// <param name="context">The <c>DrawingContext</c> in which to draw the shape.</param>
        /// <param name="viewSize">Size of the view.</param>
        public override void Draw(DrawingContext context, Size viewSize)
        {
            Point upperLeft = Translate(viewSize, UpperLeft);
            Point bottomRight = Translate(viewSize, BottomRight);
            Point center = new Point((bottomRight.X + upperLeft.X) / 2.0, (bottomRight.Y + upperLeft.Y) / 2.0);

            double radiusX = (bottomRight.X - upperLeft.X) / 2.0;
            double radiusY = (bottomRight.Y - upperLeft.Y) / 2.0;

            context.DrawEllipse(new SolidColorBrush(FillColor),
                                new Pen(new SolidColorBrush(BorderColor), STROKE_THICKNESS),
                                center,
                                radiusX,
                                radiusY);
        }
    }
}
