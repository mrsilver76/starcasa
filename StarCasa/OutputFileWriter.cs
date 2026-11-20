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

namespace StarCasa
{
    /// <summary>
    /// Deletes any old output files and writes the lists of starred images.
    /// </summary>
    internal sealed class OutputFileWriter
    {
        /// <summary>
        /// Deletes any existing output files. This is done once up front, so that orientations sharing
        /// one file can each add their lines without the later one replacing the earlier one.
        /// </summary>
        public static void DeleteExistingFiles()
        {
            string?[] requested = [Globals.PortraitFile, Globals.LandscapeFile, Globals.SquareFile, Globals.AllFile];

            // Several outputs may share one file, so list each file once
            foreach (var path in requested.OfType<string>().Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!File.Exists(path))
                    continue;

                File.Delete(path);
                Logger.Write($"Deleted existing output file: {path}", true);
            }
        }

        /// <summary>
        /// Writes each requested output file, holding the starred images of its orientation. A file
        /// is only created when there is at least one image to put in it.
        /// </summary>
        public static void WriteOutputFiles()
        {
            WriteList("portrait", Globals.PortraitFile, Globals.Orientation.Portrait);
            WriteList("landscape", Globals.LandscapeFile, Globals.Orientation.Landscape);
            WriteList("square", Globals.SquareFile, Globals.Orientation.Square);
            WriteList("all", Globals.AllFile, null);
        }

        /// <summary>
        /// Writes one output file, if it was requested.
        /// </summary>
        /// <param name="label">The name used in messages: "portrait", "landscape", "square" or "all".</param>
        /// <param name="outputPath">The output file name given on the command line, or null if not requested.</param>
        /// <param name="orientation">The orientation of the images to list, or null to list every image that did not fail.</param>
        private static void WriteList(string label, string? outputPath, Globals.Orientation? orientation)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            // Find all starred images for this output
            List<string> paths =
            [
                .. Globals.StarredImages
                    .Where(image => image.Orientation != Globals.Orientation.Failed && (orientation == null || image.Orientation == orientation))
                    .Select(image => image.Path)
            ];

            // "all" is dropped from the wording: "Wrote out 56 starred images" rather than "56 all starred images"
            string prefix = "";
            if (label != "all")
                prefix = $"{label} ";

            if (paths.Count == 0)
            {
                Logger.Write($"No {prefix}starred images found.");
                return;
            }

            // Existing files were deleted first, so append: outputs sharing one file all keep their lines
            File.AppendAllLines(outputPath, paths);
            Logger.Write($"Wrote out {GrammarHelper.Pluralise(paths.Count, $"{prefix}starred image", $"{prefix}starred images")} to {outputPath}");
        }
    }
}
