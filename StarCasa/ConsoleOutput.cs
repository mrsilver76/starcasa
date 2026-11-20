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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace StarCasa
{
    /// <summary>
    /// Writes the usage text, the run header and the update notice to the console.
    /// </summary>
    internal sealed class ConsoleOutput
    {
        /// <summary>
        /// Displays the usage information to the user and optional error message. This method never returns.
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        [DoesNotReturn]
        public static void ShowUsage(string errorMessage = "")
        {
            Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <inputDir>... [options]\n" +
                              $"Generate a report of starred photos in Picasa.\n");

            if (string.IsNullOrEmpty(errorMessage))
                Console.WriteLine($"This is version {VersionHelper.OutputVersion(Globals.ProgramVersion)}, copyright © 2025-{DateTime.Now.Year} Richard Lawrence.\n" +
                                  $"Picture icons created by Freepik - Flaticon (https://www.flaticon.com/free-icons/picture)\n");

            PrintOptionsWithDescriptions();

            Console.WriteLine($"Logs are written to {Path.Combine(Globals.AppDataPath, "Logs")}");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {errorMessage}");
                Environment.Exit(1);
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Prints each section of command line options with its descriptions. Descriptions are wrapped to the
        /// console width, with wrapped lines and sub-options indented.
        /// </summary>
        private static void PrintOptionsWithDescriptions()
        {
            // Define sections and their options + descriptions
            Dictionary<string, (string Option, string Description)[]> sections = new()
            {
                ["Mandatory"] =
                [
                    ("<inputDir>...", "One or more directories to scan, including their subdirectories, for starred photos.")
                ],
                ["Output files (at least one is needed)"] =
                [
                    ("/p, -p, --portrait <file>", "Write a list of starred portrait images to <file>."),
                    ("/l, -l, --landscape <file>", "Write a list of starred landscape images to <file>."),
                    ("/s, -s, --square <file>", "Write a list of starred square images to <file>."),
                    ("/a, -a, --all <file>", "Write a list of all starred images to <file>. Can be combined with --portrait, --landscape and --square.")
                ],
                ["Miscellaneous"] =
                [
                    ("-nc, --no-check", "Don't check GitHub for later versions."),
                    ("-v, --verbose", "Display detailed logging to the console."),
                    ("/?, -h, --help", "Display this help message and exit.")
                ]
            };

            // Determine max line width (at least 50 chars, or console width - 5).
            // Console.WindowWidth throws when output is redirected, so use 80 columns then.
            int consoleWidth = 80;
            if (!Console.IsOutputRedirected)
                consoleWidth = Console.WindowWidth;

            int maxLineWidth = Math.Max(consoleWidth, 50) - 5;

            // Find max option length across all sections for consistent column width
            int firstColWidth = 0;

            foreach (var section in sections.Values)
                foreach (var (option, _) in section)
                    firstColWidth = Math.Max(firstColWidth, option.Length);

            firstColWidth += 2;  // 2-character gap

            // Print each section followed by the options
            foreach (var (header, options) in sections)
            {
                // Section header
                Console.WriteLine($"{header}:");

                // Each option + description
                foreach (var (option, description) in options)
                {
                    // Wrap description
                    var wrapped = WrapText(description, maxLineWidth - firstColWidth - 1);  // -1 for extra indent
                    bool firstLine = true;

                    foreach (var line in wrapped)
                    {
                        if (firstLine)
                        {
                            // First line: 1-character indent, then the option, then the description.
                            Console.WriteLine($" {option.PadRight(firstColWidth)}{line}");
                            firstLine = false;
                        }
                        else
                        {
                            // Wrapped lines: 1-character indent, then firstColWidth spaces, 1 space and the text.
                            Console.WriteLine(new string(' ', firstColWidth + 2) + line);
                        }
                    }
                }

                Console.WriteLine();  // Blank line between sections.
            }

        }

        /// <summary>
        /// Displays the header information for the application, including version, copyright, and settings.
        /// </summary>
        public static void ShowHeader()
        {
            string line = new('─', 70);

            Console.WriteLine(line);
            Console.WriteLine($"StarCasa {VersionHelper.OutputVersion(Globals.ProgramVersion)}");
            Console.WriteLine($"Copyright © 2025-{DateTime.Now.Year} Richard Lawrence");
            Console.WriteLine("https://github.com/mrsilver76/starcasa");
            Console.WriteLine();
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY. This is free software,");
            Console.WriteLine("and you are welcome to redistribute it under certain conditions; see");
            Console.WriteLine("the documentation for details.");
            Console.WriteLine();

            // Prepare titles + content
            List<(string Title, string Value)> items = [];

            // Source directories
            if (Globals.InputDirs.Count == 1)
                items.Add(("Source dir:", Globals.InputDirs[0]));
            else
            {
                items.Add(("Sources:", Globals.InputDirs[0]));
                foreach (var folder in Globals.InputDirs.Skip(1))
                    items.Add(("", folder));  // continuation lines
            }

            // Output files, one per orientation that was asked for
            if (Globals.PortraitFile != null)
                items.Add(("Portrait file:", Globals.PortraitFile));
            if (Globals.LandscapeFile != null)
                items.Add(("Landscape file:", Globals.LandscapeFile));
            if (Globals.SquareFile != null)
                items.Add(("Square file:", Globals.SquareFile));
            if (Globals.AllFile != null)
                items.Add(("All file:", Globals.AllFile));

            // Find longest title length
            int pad = items.Max(i => i.Title.Length) + 2;

            // Print everything
            foreach (var (title, value) in items)
                Console.WriteLine($"{title.PadRight(pad)}{value}");

            Console.WriteLine(line);
            Console.WriteLine();

            // Log environment info
            LogEnvironmentInfo();
        }

        /// <summary>
        /// Output to the logs the environment information, such as .NET version, OS and architecture.
        /// Also includes the full command line.
        /// </summary>
        private static void LogEnvironmentInfo()
        {
            string dotnet = RuntimeInformation.FrameworkDescription;
            string os = RuntimeInformation.OSDescription.Trim();

            string archName = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();

            Logger.Write($"Running {VersionHelper.OutputVersion(Globals.ProgramVersion)} on {dotnet} ({os}, {archName})", true);

            Logger.Write($"Command line: {Environment.CommandLine}", true);
        }

        /// <summary>
        /// Checks if there is a later release of the application on GitHub and notifies the user.
        /// </summary>
        public static void CheckLatestRelease()
        {
            // Skip if disabled
            if (!Globals.GitHubVersionCheck)
                return;

            string gitRepo = "mrsilver76/starcasa";
            var result = GitHubVersionChecker.CheckLatestRelease(Globals.ProgramVersion, gitRepo, Path.Combine(Globals.AppDataPath, "versionCheck.ini"));

            if (result.UpdateAvailable)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  ℹ️ A new version ({VersionHelper.OutputVersion(result.LatestVersion)}) is available!");
                Console.ResetColor();
                Console.WriteLine($" You are using {VersionHelper.OutputVersion(Globals.ProgramVersion)}");
                Console.WriteLine($"     Get it from https://www.github.com/{gitRepo}/");
            }
        }

        /// <summary>
        /// Given a block of text and a maximum line width, yields lines of text wrapped at word boundaries.
        /// </summary>
        /// <param name="text">The text to wrap.</param>
        /// <param name="maxWidth">The longest line allowed, in characters.</param>
        /// <returns>The wrapped lines, without line breaks.</returns>
        private static IEnumerable<string> WrapText(string text, int maxWidth)
        {
            var words = text.Split(' ');
            var line = new StringBuilder();

            foreach (var word in words)
            {
                int separatorWidth = 0;
                if (line.Length > 0)
                    separatorWidth = 1;

                if (line.Length + word.Length + separatorWidth > maxWidth)
                {
                    yield return line.ToString();
                    line.Clear();
                }

                if (line.Length > 0)
                    line.Append(' ');

                line.Append(word);
            }

            if (line.Length > 0)
                yield return line.ToString();
        }
    }
}
