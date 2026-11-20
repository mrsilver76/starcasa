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

using System.Globalization;

namespace StarCasa
{
    /// <summary>
    /// Handles parsing of command line arguments.
    /// </summary>
    internal sealed class CommandLineParser
    {
        /// <summary>Command line options that must be followed by a value</summary>
        private static readonly HashSet<string> OptionsWithValue =
        [
            "/p", "-p", "--portrait",
            "/l", "-l", "--landscape",
            "/s", "-s", "--square",
            "/a", "-a", "--all"
        ];

        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void ParseArguments(string[] args)
        {
            if (args.Length == 0)
                ConsoleOutput.ShowUsage();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower(CultureInfo.InvariantCulture);

                // Every option that takes a value must have one following it

                if (OptionsWithValue.Contains(arg) && i + 1 >= args.Length)
                    ConsoleOutput.ShowUsage($"Option '{arg}' requires a value.");

                if (arg == "/?" || arg == "-h" || arg == "--help")
                    ConsoleOutput.ShowUsage();
                else if (arg == "/p" || arg == "-p" || arg == "--portrait")
                    Globals.PortraitFile = args[++i];
                else if (arg == "/l" || arg == "-l" || arg == "--landscape")
                    Globals.LandscapeFile = args[++i];
                else if (arg == "/s" || arg == "-s" || arg == "--square")
                    Globals.SquareFile = args[++i];
                else if (arg == "/a" || arg == "-a" || arg == "--all")
                    Globals.AllFile = args[++i];
                else if (arg == "-v" || arg == "--verbose")
                    Globals.VerboseMode = true;
                else if (arg == "-nc" || arg == "--no-check")
                    Globals.GitHubVersionCheck = false;
                else if (arg.StartsWith('-') || arg.StartsWith('/'))
                    ConsoleOutput.ShowUsage($"Unknown option: {arg}");
                else
                    Globals.InputDirs.Add(args[i]);  // args[i] to keep the original case
            }

            // Now validate the arguments
            ValidateArguments();
        }

        /// <summary>
        /// Returns the full path with any trailing directory separator removed, so that different
        /// spellings of the same folder (relative, trailing slash) compare equal. The path need not exist.
        /// </summary>
        /// <param name="path">The path to normalise.</param>
        /// <returns>The full path without a trailing directory separator.</returns>
        private static string NormalisePath(string path)
        {
            string full = Path.GetFullPath(path);
            string root = Path.GetPathRoot(full) ?? "";

            // "C:\" or "/" must keep their trailing separator
            if (full.Length <= root.Length)
                return full;

            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Validates the parsed command line arguments and displays usage information along with
        /// an error message if any are invalid.
        /// </summary>
        private static void ValidateArguments()
        {
            // Check that values supplied are valid and sane
            if (Globals.InputDirs.Count == 0)
                ConsoleOutput.ShowUsage("No input directories specified. Please provide at least one directory containing photos.");

            // An output option that was not given is null
            if (Globals.PortraitFile == null && Globals.LandscapeFile == null && Globals.SquareFile == null && Globals.AllFile == null)
                ConsoleOutput.ShowUsage("No output files specified. Please provide at least one output file for the report.");

            foreach (var dir in Globals.InputDirs)
                if (!Directory.Exists(dir))
                    ConsoleOutput.ShowUsage($"Input directory does not exist: {dir}");

            // Make every input folder a full path, so the output lines are full paths wherever the program is run
            // from, and drop repeats of the same folder (the same path given twice, or one relative and one full).
            // This compares path text only: Z:\ and a UNC path to the same share count as different folders.
            List<string> fullDirs = [];

            foreach (var dir in Globals.InputDirs)
            {
                string fullDir = NormalisePath(dir);

                if (fullDirs.Contains(fullDir, StringComparer.OrdinalIgnoreCase))
                    Logger.Write($"Ignoring duplicate input directory: {dir}", true);
                else
                    fullDirs.Add(fullDir);
            }

            Globals.InputDirs = fullDirs;

            // Check each output folder exists now, so a typo doesn't waste the scan, and make each
            // output a full path so the header and log show where the files really go
            Globals.PortraitFile = ValidateOutputPath("portrait", Globals.PortraitFile);
            Globals.LandscapeFile = ValidateOutputPath("landscape", Globals.LandscapeFile);
            Globals.SquareFile = ValidateOutputPath("square", Globals.SquareFile);
            Globals.AllFile = ValidateOutputPath("all", Globals.AllFile);

            // The all list repeats every image, so sharing a file with an orientation would write images twice.
            // This compares path text only: a mapped drive and a UNC path to the same file count as different.
            if (Globals.AllFile != null)
            {
                if (string.Equals(Globals.AllFile, Globals.PortraitFile, StringComparison.OrdinalIgnoreCase))
                    ConsoleOutput.ShowUsage($"The all and portrait output files must be different: {Globals.AllFile}");

                if (string.Equals(Globals.AllFile, Globals.LandscapeFile, StringComparison.OrdinalIgnoreCase))
                    ConsoleOutput.ShowUsage($"The all and landscape output files must be different: {Globals.AllFile}");

                if (string.Equals(Globals.AllFile, Globals.SquareFile, StringComparison.OrdinalIgnoreCase))
                    ConsoleOutput.ShowUsage($"The all and square output files must be different: {Globals.AllFile}");
            }
        }

        /// <summary>
        /// Checks one output path, showing usage information and exiting if it is empty, invalid or
        /// in a folder that does not exist. Does nothing if the output was not requested.
        /// </summary>
        /// <param name="label">The name used in messages: "portrait", "landscape", "square" or "all".</param>
        /// <param name="path">The output file name given on the command line, or null if not given.</param>
        /// <returns>The full path of the output file, or null if the output was not requested.</returns>
        private static string? ValidateOutputPath(string label, string? path)
        {
            // Wasn't specified, so nothing to check
            if (path == null)
                return null;

            if (string.IsNullOrWhiteSpace(path))
                ConsoleOutput.ShowUsage($"Empty output path specified for {label}");

            string fullPath = "";

            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                ConsoleOutput.ShowUsage($"Invalid output path specified for {label}: {path}");
            }

            string fileName = Path.GetFileName(fullPath);

            // GetFullPath accepts characters such as '|' and '*' that fail later at write time
            if (fileName == "" || fullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                ConsoleOutput.ShowUsage($"Invalid output path specified for {label}: {path}");

            string outputDir = Path.GetDirectoryName(fullPath) ?? "";

            if (outputDir != "" && !Directory.Exists(outputDir))
                ConsoleOutput.ShowUsage($"Output folder does not exist for {label}: {outputDir}");

            return fullPath;
        }
    }
}
