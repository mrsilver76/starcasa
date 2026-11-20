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
    /// Scans folders for Picasa ".picasa.ini" files and collects the starred images in them.
    /// </summary>
    internal sealed class PicasaScanner
    {
        /// <summary>File extensions of the images this program supports. Picasa can also star videos and other formats, which are skipped.</summary>
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg" };

        /// <summary>
        /// Scans each input directory and every subdirectory under it for starred images, and adds
        /// them to the global list. Ignores any directory named ".picasaoriginals" and everything under it. The images themselves
        /// are not opened here: see ResolveImages.
        /// </summary>
        public static void ScanFolders()
        {
            foreach (var root in Globals.InputDirs)
            {
                Logger.Write($"Processing directory: {root}");
                ProcessDirectory(root);

                // Skip folders we can't open (such as $RECYCLE.BIN). AttributesToSkip = 0 keeps hidden and system folders.
                var options = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    AttributesToSkip = 0
                };

                foreach (var dir in Directory.EnumerateDirectories(root, "*", options))
                {
                    // Skip .picasaoriginals and everything under it, whatever is in there
                    string[] parts = Path.GetRelativePath(root, dir).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (parts.Contains(".picasaoriginals", StringComparer.OrdinalIgnoreCase))
                        continue;

                    ProcessDirectory(dir);
                }
            }
        }

        /// <summary>
        /// Locates the ".picasa.ini" file in the specified directory and processes it to find starred images.
        /// </summary>
        /// <param name="dir">The directory to look in. It need not hold a ".picasa.ini" file.</param>
        private static void ProcessDirectory(string dir)
        {
            string iniPath = Path.Combine(dir, ".picasa.ini");

            if (!File.Exists(iniPath))
                return;

            string currentSection = "";
            int totalFound = 0;

            try
            {
                foreach (var line in File.ReadLines(iniPath))
                {
                    string trimmed = line.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(';'))
                        continue;

                    // Handle section headers
                    if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                    {
                        currentSection = trimmed[1..^1];
                        continue;
                    }

                    // If the section is empty or does not indicate a starred image, skip it
                    if (currentSection == "" || !trimmed.StartsWith("star=yes", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // We've found a starred image inside a section. An image may be listed twice, so we only want to keep one
                    string imagePath = Path.Combine(dir, currentSection);

                    if (!Globals.StarredImages.Any(image => string.Equals(image.Path, imagePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        Globals.StarredImages.Add(new Globals.StarredImage(imagePath));
                        totalFound++;
                    }
                }
            }
            catch (Exception ex)
            {
                // An unreadable .picasa.ini shouldn't stop the whole scan. Keep what was found before the error.
                Logger.Write($"Error reading '{iniPath}': {ex.Message}");
            }

            if (totalFound > 0)
                Logger.Write($"Found {GrammarHelper.Pluralise(totalFound, "starred image", "starred images")} in '{dir}'");
            else
                Logger.Write($"Found no starred images in '{dir}'", true);
        }

        /// <summary>
        /// Checks that every starred image found in the scan can be loaded and works out its orientation. The orientation is
        /// then updated within the StarredImage object. If an image cannot be loaded, it is marked as failed and will not be
        /// included in any output.
        /// </summary>
        public static void ResolveImages()
        {
            // We need the orientation of an image if the user requested anything but "all" on its own.
            bool needOrientation = !string.IsNullOrWhiteSpace(Globals.PortraitFile)
                || !string.IsNullOrWhiteSpace(Globals.LandscapeFile)
                || !string.IsNullOrWhiteSpace(Globals.SquareFile);
            var images = Globals.StarredImages;

            // Reading many images over a network share can take a minute or more, so say what is happening and show progress
            bool showProgress = needOrientation && images.Count > 0;
            if (showProgress)
            {
                Logger.Write($"Reading the orientation of {GrammarHelper.Pluralise(images.Count, "starred image", "starred images")}...");
                ProgressBar.Total = images.Count;
                ProgressBar.Start();
            }

            try
            {
                // Each task writes only to its own image
                Parallel.For(0, images.Count, i =>
                {
                    var image = images[i];

                    try
                    {
                        // Only JPEG images are supported
                        if (!SupportedExtensions.Contains(Path.GetExtension(image.Path)))
                        {
                            image.Orientation = Globals.Orientation.Failed;
                            Logger.Write($"Skipping '{image.Path}': not a JPEG image", true);
                            return;
                        }

                        // Skip entries for files that no longer exist
                        if (!File.Exists(image.Path))
                        {
                            image.Orientation = Globals.Orientation.Failed;
                            Logger.Write($"Skipping '{image.Path}': file does not exist", true);
                            return;
                        }

                        // If we need the orientation for this file, then get it
                        if (needOrientation)
                            image.Orientation = ImageOrientation.GetOrientation(image.Path);
                    }
                    finally
                    {
                        // Count every image, whether it was read or skipped, so the bar reaches 100%
                        ProgressBar.Add(1);
                    }
                });
            }
            finally
            {
                ProgressBar.Stop();
            }
        }
    }
}
