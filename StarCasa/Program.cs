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
 * < https://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Reflection;
using static StarCasa.Helpers;
using static System.Net.Mime.MediaTypeNames;

namespace StarCasa
{
    class Program
    {
        public static List<string> inputDirs = [];  // List of input directories to scan for photos
        public static bool checkExists = false;  // Flag to check if starred images actually exist

        // Internal globals
        public static Version version = Assembly.GetExecutingAssembly().GetName().Version!;
        public static string appDataPath = ""; // Path to the app data folder
        public static Dictionary<string, string> outputFiles = new(StringComparer.OrdinalIgnoreCase);  // Dictionary to hold output file paths by orientation
        public static Dictionary<string, string> starredImages = new(StringComparer.OrdinalIgnoreCase);  // Dictionary to hold lists of starred images by orientation

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Set up various paths and prepare logging
            InitialiseLogger();

            // Parse the arguments
            ParseArguments(args);

            Console.WriteLine($"StarCasa v{OutputVersion(version)}, Copyright © 2025-{DateTime.Now.Year} Richard Lawrence");
            Console.WriteLine($"Generate a report of starred photos in Picasa.");
            Console.WriteLine($"https://github.com/mrsilver76/starcasa\n");
            Console.WriteLine($"This program comes with ABSOLUTELY NO WARRANTY. This is free software,");
            Console.WriteLine($"and you are welcome to redistribute it under certain conditions; see");
            Console.WriteLine($"the documentation for details.");
            Console.WriteLine();

            Logger($"Starting StarCasa...");

            // Don't call any earlier, otherwise plexToken and machineIdentifier will be in the logs
            Logger($"Parsed arguments: {string.Join(" ", args)}", true);

            // Output report summary to user
            foreach (var kvp in outputFiles)
            {
                string orientation = kvp.Key.ToLowerInvariant();
                string path = kvp.Value;

                if (!string.IsNullOrWhiteSpace(path))
                    Logger($"Writing out {orientation} starred images to {path}");
            }

            // Now scan the input directories
            ScanImages();

            // We now have a collection of starred images with their orientations. So time to
            // write these out to the specified output files.

            // Delete any existing output files first
            foreach (var path in outputFiles.Values.Distinct(StringComparer.OrdinalIgnoreCase))
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);

            // Write out the results to the output files
            foreach (var kvp in outputFiles)
            {
                string orientation = kvp.Key;
                string filePath = kvp.Value;

                if (string.IsNullOrWhiteSpace(filePath))
                    continue;

                // Find all starred images for this orientation
                var matches = starredImages
                    .Where(pair => string.Equals(pair.Value, orientation, StringComparison.OrdinalIgnoreCase))
                    .Select(pair => pair.Key)
                    .ToList();

                if (matches.Count == 0)
                    Logger($"No {orientation} starred images found.");
                else
                {
                    File.WriteAllLines(filePath, matches);
                    Logger($"Wrote out {orientation} starred images ({matches.Count}) to {filePath}");
                }    
            }

            // Output the total number of starred images found
            Logger($"StarCasa finished. Total starred images found: {starredImages.Count}");
            CheckLatestRelease();
            Environment.Exit(0);
        }

        /// <summary>
        /// Scans the specified input directories for subdirectories to process. Ignores any
        /// directories named ".picasaoriginals".
        /// </summary>
        static void ScanImages()
        {
            foreach (var root in inputDirs)
            {
                Logger($"Processing directory: {root}");
                ProcessDirectory(root);

                foreach (var dir in System.IO.Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
                {
                    if (string.Equals(Path.GetFileName(dir), ".picasaoriginals", StringComparison.OrdinalIgnoreCase))
                        continue;

                    ProcessDirectory(dir);
                }                
            }
        }

        /// <summary>
        /// Locates the ".picasa.ini" file in the specified directory and processes it to find starred images.
        /// </summary>
        /// <param name="dir"></param>
        static void ProcessDirectory(string dir)
        {
            string iniPath = Path.Combine(dir, ".picasa.ini");
            if (!File.Exists(iniPath))
                return;

            string currentSection = "";
            int totalFound = 0;

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

                // We've found a starred image inside a section, so now we need to check if the file
                // exists and what orientation it has.

                // If the section is not a valid image file, skip it
                string fullPath = Path.Combine(dir, currentSection);
                if (checkExists && !File.Exists(fullPath))
                    continue;

                // Get the image orientation
                string orientation = GetImageOrientation(fullPath);
                if (string.IsNullOrWhiteSpace(orientation))
                    continue;

                // Add to global collection
                starredImages[fullPath] = orientation;
                totalFound++;
            }

            if (totalFound > 0)
                Logger($"Found {totalFound} starred in '{dir}'");
            else
                Logger($"Found no starred in '{dir}'", true);

        }

        /// <summary>
        /// Determines the orientation of an image based on its dimensions.
        /// </summary>
        /// <remarks>This method attempts to load the image from the specified path. If the image cannot
        /// be loaded,  an error is logged and an empty string is returned.</remarks>
        /// <param name="path">The file path of the image to analyze. Must be a valid path to an image file.</param>
        /// <returns>A string representing the orientation of the image:  landscape if the width is greater than the height, 
        /// portrait if the height is greater than the width,  or square if the width and height are equal. Returns an
        /// empty string if the image cannot be loaded.</returns>
        static string GetImageOrientation(string path)
        {
            // If there is an "all" defined, then we should return the "all" path
            if (outputFiles.TryGetValue("all", out var allPath) && !string.IsNullOrWhiteSpace(allPath))
                return "all";
 
            try
            {
                using var img = System.Drawing.Image.FromFile(path, useEmbeddedColorManagement: false);

                if (img.Width > img.Height)
                    return "landscape";
                if (img.Width < img.Height)
                    return "portrait";
                return "square";
            }
            catch (Exception ex)
            {
                Logger($"Error loading '{path}': {ex}");
                return "";
            }
        }
    }
}
