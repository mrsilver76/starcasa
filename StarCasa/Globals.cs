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

using System.Reflection;

namespace StarCasa
{
    /// <summary>
    /// All global variables used by the application.
    /// </summary>
    internal static class Globals
    {
        // Variables that can be set by the user via command line arguments
        #region User-defined variables

        /// <summary>List of input directories to scan for starred images</summary>
        public static List<string> InputDirs { get; set; } = [];

        /// <summary>Output file for starred portrait images (null if not requested)</summary>
        public static string? PortraitFile { get; set; }

        /// <summary>Output file for starred landscape images (null if not requested)</summary>
        public static string? LandscapeFile { get; set; }

        /// <summary>Output file for starred square images (null if not requested)</summary>
        public static string? SquareFile { get; set; }

        /// <summary>Output file for all starred images, whatever their orientation (null if not requested)</summary>
        public static string? AllFile { get; set; }

        /// <summary>Flag for verbose output</summary>
        public static bool VerboseMode { get; set; }

        /// <summary>Flag to check if there is a later version on GitHub</summary>
        public static bool GitHubVersionCheck { get; set; } = true;

        #endregion

        // Variables used internally by the application
        #region Internal variables

        /// <summary>Version of the application</summary>
        public static Version ProgramVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version!;

        /// <summary>Path to the app data folder</summary>
        public static string AppDataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "StarCasa");

        /// <summary>Starred images found, with the orientation of each once it has been worked out</summary>
        public static List<StarredImage> StarredImages { get; } = [];

        /// <summary>Number of starred images that could not be loaded and were left out. A field, so that several threads can add to it with Interlocked.</summary>
        public static int SkippedImages;

        #endregion

        /// <summary>
        /// The shape of an image, or why it has none.
        /// </summary>
        internal enum Orientation
        {
            /// <summary>Not worked out yet, or not needed</summary>
            Unknown,
            Portrait,
            Landscape,
            Square,
            /// <summary>The image file is missing or could not be read</summary>
            Failed
        }

        /// <summary>
        /// One starred image found in a ".picasa.ini" file.
        /// </summary>
        /// <param name="path">The full path of the image file.</param>
        internal sealed class StarredImage(string path)
        {
            /// <summary>Full path of the image file</summary>
            public string Path { get; } = path;

            /// <summary>Shape of the image. Unknown until it is worked out, and when the list of all images was requested. Failed if the file is missing or cannot be read.</summary>
            public Orientation Orientation { get; set; }
        }
    }
}
