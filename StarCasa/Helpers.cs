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

using IniParser;
using IniParser.Model;
using System.Text.RegularExpressions;
using static StarCasa.Program;

namespace StarCasa
{
    public static class Helpers
    {
        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void ParseArguments(string[] args)
        {
            if (args.Length == 0)
                DisplayUsage();

            // Loop through all arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if (arg == "/?" || arg == "-h" || arg == "--help")
                    DisplayUsage();
                else if (arg == "/p" || arg == "-p" || arg == "--portrait" && i + 1 < args.Length)
                    outputFiles["portrait"] = args[++i];
                else if (arg == "/l" || arg == "-l" || arg == "--landscape" && i + 1 < args.Length)
                    outputFiles["landscape"] = args[++i];
                else if (arg == "/s" || arg == "-s" || arg == "--square" && i + 1 < args.Length)
                    outputFiles["square"] = args[++i];
                else if (arg == "/a" || arg == "-a" || arg == "--all" && i + 1 < args.Length)
                    outputFiles["all"] = args[++i];
                else if (arg == "/c" || arg == "-c" || arg == "--check")
                    checkExists = true;  // Enable checking if starred images actually exist
                else if (arg.StartsWith('-') || arg.StartsWith('/'))
                    DisplayUsage($"Unknown option: {arg}");
                else  // This should be a directory
                    inputDirs.Add(args[i]);  // args[i] to ensure correct case
            }

            // Sanity checks here

            // Validate that at least one input directory is provided

            if (inputDirs.Count == 0)
                DisplayUsage("No input directories specified. Please provide at least one directory containing photos.");

            // Validate that at least one output file is provided

            if (outputFiles.Count == 0)
                DisplayUsage("No output files specified. Please provide at least one output file for the report.");

            // Validate that --all is not used with other output files

            if (outputFiles.TryGetValue("all", out var allDefined) && !string.IsNullOrWhiteSpace(allDefined) && outputFiles.Count > 1)
                DisplayUsage("--all cannot be used alongside --portrait, --landscape, or --square");
                
            // Validate that all the directories provided actually exist

            foreach (var dir in inputDirs)
                if (!Directory.Exists(dir))
                    DisplayUsage($"Input directory does not exist: {dir}");

            // Validate that any output files are specified and not empty

            foreach (var kvp in outputFiles)
                if (string.IsNullOrWhiteSpace(kvp.Value))
                    DisplayUsage($"Empty output path specified for {kvp.Key}");

        }

        /// <summary>
        /// Displays the usage information to the user and optional error message.
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        public static void DisplayUsage(string errorMessage = "")
        {
            Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} StarCasa <inputDir1> [<inputDir2> ...] [options]\n" +
                                "Generate a report of starred photos in Picasa.\n");


            if (string.IsNullOrEmpty(errorMessage))
                Console.WriteLine($"This is version {OutputVersion(version)}, copyright © 2025-{DateTime.Now.Year} Richard Lawrence.\n" +
                                    "Picture icons created by Freepik - Flaticon (https://www.flaticon.com/free-icons/picture)\n");

            Console.WriteLine(  "Options:\n" +
                                "  /p, -p, --portrait <file>    Output file path for portrait images.\n" +
                                "  /l, -l, --landscape <file>   Output file path for landscape images.\n" + 
                                "  /s, -s, --square <file>      Output file path for square images.\n" +
                                "  /a, -a, --all <file>         Output file path for all starred images.\n" +
                                "  /c, -c, --check              Check if starred images exist (much slower)\n" +
                                "  /?, -h, --help               Show this help message and exit.\n" +
                                "\n" +
                               $"Logs are written to {Path.Combine(appDataPath, "Logs")}");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {errorMessage}");
                Environment.Exit(-1);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Defines the location for logs and deletes any old log files
        /// </summary>
        public static void InitialiseLogger()
        {
            // Set the path for the application data folder
            appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StarCasa");

            // Set the log folder path to be inside the application data folder
            string logFolderPath = Path.Combine(appDataPath, "Logs");

            // Create the folder if it doesn't exist
            Directory.CreateDirectory(logFolderPath);

            // Delete log files older than 14 days
            var logFiles = Directory.GetFiles(logFolderPath, "*.log");
            foreach (var file in logFiles)
            {
                DateTime lastModified = File.GetLastWriteTime(file);
                if ((DateTime.Now - lastModified).TotalDays > 14)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger($"Error deleting log file {file}: {ex.Message}", true);
                    }
                }
            }
        }

        /// <summary>
        /// Writes a message to the log file for debugging.
        /// </summary>
        /// <param name="message">Message to output</param>
        /// <param name="verbose">Verbose output, only for the logs</param>

        public static void Logger(string message, bool verbose = false)
        {
            // Define the path and filename for this log
            string logFile = DateTime.Now.ToString("yyyy-MM-dd");
            logFile = Path.Combine(appDataPath, "Logs", $"log-{logFile}.log");

            // Define the timestamp
            string tsTime = DateTime.Now.ToString("HH:mm:ss");
            string tsDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Write to file
            File.AppendAllText(logFile, $"[{tsDate} {tsTime}] {message}{Environment.NewLine}");

            // If this isn't verbose output for the logfiles, then output to the console
            if (!verbose)
                Console.WriteLine($"[{tsTime}] {message}");
        }

        /// <summary>
        /// Checks if there is a later release of the application on GitHub and notifies the user.
        /// </summary>
        public static void CheckLatestRelease()
        {
            string gitHubRepo = "mrsilver76/starcasa";
            string iniPath = Path.Combine(appDataPath, "versionCheck.ini");

            var parser = new FileIniDataParser();
            IniData ini = File.Exists(iniPath) ? parser.ReadFile(iniPath) : new IniData();

            if (NeedsCheck(ini, out Version? cachedVersion))
            {
                var latest = TryFetchLatestVersion(gitHubRepo);
                if (latest != null)
                {
                    ini["Version"]["LatestReleaseChecked"] = latest.Value.Timestamp;

                    if (!string.IsNullOrEmpty(latest.Value.Version))
                    {
                        ini["Version"]["LatestReleaseVersion"] = latest.Value.Version;
                        cachedVersion = ParseSemanticVersion(latest.Value.Version);
                    }

                    parser.WriteFile(iniPath, ini); // Always write if we got any response at all
                }
            }

            if (cachedVersion != null && cachedVersion > version)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($" ℹ️ A new version ({OutputVersion(cachedVersion)}) is available!");
                Console.ResetColor();
                Console.WriteLine($" You are using {OutputVersion(version)}");
                Console.WriteLine($"    Get it from https://www.github.com/{gitHubRepo}/");
            }
        }

        /// <summary>
        /// Takes a semantic version string in the format "major.minor.revision" and returns a Version object in
        /// the format "major.minor.0.revision"
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public static Version? ParseSemanticVersion(string versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString))
                return null;

            var parts = versionString.Split('.');
            if (parts.Length != 3)
                return null;

            if (int.TryParse(parts[0], out int major) &&
                int.TryParse(parts[1], out int minor) &&
                int.TryParse(parts[2], out int revision))
            {
                try
                {
                    return new Version(major, minor, 0, revision);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Compares the last checked date and version in the INI file to determine if a check is needed.
        /// </summary>
        /// <param name="ini"></param>
        /// <param name="cachedVersion"></param>
        /// <returns></returns>
        private static bool NeedsCheck(IniData ini, out Version? cachedVersion)
        {
            cachedVersion = null;

            string dateStr = ini["Version"]["LatestReleaseChecked"];
            string versionStr = ini["Version"]["LatestReleaseVersion"];

            bool hasTimestamp = DateTime.TryParse(dateStr, out DateTime lastChecked);
            bool isExpired = !hasTimestamp || (DateTime.UtcNow - lastChecked.ToUniversalTime()).TotalDays >= 7;

            cachedVersion = ParseSemanticVersion(versionStr);

            return isExpired;
        }

        /// <summary>
        /// Fetches the latest version from the GitHub repo by looking at the releases/latest page.
        /// </summary>
        /// <param name="repo">The name of the repo</param>
        /// <returns>Version and today's date and time</returns>
        private static (string? Version, string Timestamp)? TryFetchLatestVersion(string repo)
        {
            string url = $"https://api.github.com/repos/{repo}/releases/latest";
            using var client = new HttpClient();

            string ua = repo.Replace('/', '.') + "/" + OutputVersion(version);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(ua);

            try
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                if (!response.IsSuccessStatusCode)
                {
                    // Received response, but it's a client or server error (e.g., 404, 500)
                    return (null, timestamp);  // Still update "last checked"
                }

                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var match = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                if (!match.Success)
                {
                    return (null, timestamp);  // Response body not as expected
                }

                string version = match.Groups[1].Value.TrimStart('v', 'V');
                return (version, timestamp);
            }
            catch
            {
                // This means we truly couldn't reach GitHub at all
                return null;
            }
        }

        /// <summary>
        /// Pluralises a string based on the number provided.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="singular"></param>
        /// <param name="plural"></param>
        /// <returns></returns>
        public static string Pluralise(int number, string singular, string plural)
        {
            return number == 1 ? $"{number} {singular}" : $"{number:N0} {plural}";
        }

        /// <summary>
        /// Given a .NET Version object, outputs the version in a semantic version format.
        /// If the build number is greater than 0, it appends `-preX` to the version string.
        /// </summary>
        /// <returns></returns>
        public static string OutputVersion(Version? netVersion)
        {
            if (netVersion == null)
                return "0.0.0";

            // Use major.minor.revision from version, defaulting patch to 0 if missing
            int major = netVersion.Major;
            int minor = netVersion.Minor;
            int revision = netVersion.Revision >= 0 ? netVersion.Revision : 0;

            // Build the base semantic version string
            string result = $"{major}.{minor}.{revision}";

            // Append `-preX` if build is greater than 0
            if (netVersion.Build > 0)
                result += $"-pre{netVersion.Build}";

            return result;
        }
    }
}