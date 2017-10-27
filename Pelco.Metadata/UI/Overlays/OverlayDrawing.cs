using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pelco.PDK.Metadata.UI.Overlays
{
    /// <summary>
    /// Base class for all overlay drawings.
    /// </summary>
    public abstract class OverlayDrawing : IEquatable<OverlayDrawing>
    {
        protected const int STROKE_THICKNESS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        protected OverlayDrawing(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Returns the ID of the overlay
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Create a <c>Shape</c> from the overlay drawing.
        /// </summary>
        /// <returns>The corresponding <c>Shape</c>.</returns>
        public abstract Shape ToShape(Size viewSize);

        /// <summary>
        /// Draw the shapen into the context.
        /// </summary>
        /// <param name="context">Context in which to draw.</param>
        /// <param name="viewSize">Size of the view.</param>
        public abstract void Draw(DrawingContext context, Size viewSize);

        /// <summary>
        /// Returns true if the overlay id's are equal, false otherwise
        /// </summary>
        /// <param name="other">The other overlay base to compare</param>
        /// <returns></returns>
        public bool Equals(OverlayDrawing other)
        {
            if (other == null)
            {
                return false;
            }
            return ID.Equals(other.ID);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OverlayDrawing);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"ID={ID}, Type={GetType().Name}";
        }

        protected Point Translate(Size viewSize, Point point)
        {
            return new Point(TranslateX(viewSize, point.X),
                             TranslateY(viewSize, point.Y));
        }

        protected int TranslateX(Size viewSize, double xNorm)
        {
            return (int)(viewSize.Width * xNorm);
        }

        protected int TranslateY(Size viewSize, double xNorm)
        {
            return (int)(viewSize.Height * xNorm);
        }
    }
}
