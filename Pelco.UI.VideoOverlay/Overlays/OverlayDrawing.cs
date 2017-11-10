using System;
using System.Windows.Media;

namespace Pelco.UI.VideoOverlay.Overlays
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
        protected OverlayDrawing()
        {
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Returns the ID of the overlay
        /// </summary>
        public string ID { get; }

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
    }
}
