using System.Windows;

namespace Pelco.PDK.Metadata.UI.Overlays
{
    /// <summary>
    /// Translates normalized points to actual points on the overlay drawing canvas.
    /// </summary>
    public interface IPointTranslator
    {
        /// <summary>
        /// Translates a normalized point to it's actual point.
        /// </summary>
        /// <param name="point">Normalized point to translate</param>
        /// <returns>Actual drawing point (translated point)</returns>
        Point TranslatePoint(Point point);

        /// <summary>
        /// Determines if a point is in bounds.  Meaning that it is viewable. This
        /// is usually applied when digital ptz is active.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>true if the point is in bounds, false otherwise</returns>
        bool IsPointInBounds(Point point);
    }
}
