/*
 * StarCasa - Generate a report of starred photos in Picasa
 * Copyright (C) 2025 Richard Lawrence
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see
 * <https://www.gnu.org/licenses/>.
 */

using System.Drawing;

namespace StarCasa
{
    /// <summary>
    /// Works out whether an image is portrait, landscape or square.
    /// </summary>
    internal sealed class ImageOrientation
    {
        /// <summary>EXIF tag that records how the camera was held</summary>
        private const int ExifOrientationTag = 0x0112;

        /// <summary>
        /// Determines the orientation of an image from its dimensions, allowing for the EXIF rotation tag.
        /// Safe to call from several threads at once.
        /// </summary>
        /// <param name="path">The full path of the image file.</param>
        /// <returns>The orientation of the image, or Failed if the image could not be loaded (the error is logged).</returns>
        public static Globals.Orientation GetOrientation(string path)
        {
            try
            {
                // Use validateImageData:false to read only the image header and not the whole file, which is faster
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var img = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);

                int width = img.Width;
                int height = img.Height;

                // GDI+ ignores the EXIF Orientation tag. Values 5 to 8 mean the photo is displayed turned a quarter turn,
                // so swap the width and height
                if (img.PropertyIdList.Contains(ExifOrientationTag))
                {
                    byte[]? tagValue = img.GetPropertyItem(ExifOrientationTag)?.Value;
                    if (tagValue is { Length: >= 2 } && BitConverter.ToUInt16(tagValue, 0) is >= 5 and <= 8)
                        (width, height) = (height, width);
                }

                if (width > height)
                    return Globals.Orientation.Landscape;
                if (width < height)
                    return Globals.Orientation.Portrait;
                return Globals.Orientation.Square;
            }
            catch (Exception ex)
            {
                Logger.Write($"Error loading '{path}': {ex.Message}");
                Interlocked.Increment(ref Globals.SkippedImages);
                return Globals.Orientation.Failed;
            }
        }
    }
}
