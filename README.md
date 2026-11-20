# StarCasa
_A Windows command line tool that scans your Picasa photo folders and generates one or more plain text lists of your starred images, filtered by orientation (portrait, landscape, or square)_

>[!TIP]
>**Looking to showcase your starred portraits?** Use StarCasa to list them, then feed that list into [SideBySide](https://github.com/mrsilver76/sidebyside) to combine pairs into perfect landscape shots - ideal for digital photo frames that hate vertical images.

## 🧰 Features
- ⭐ Detects and lists starred photos (`.jpg` and `.jpeg`) from Picasa's `.picasa.ini` files.
- 🖼️ Classifies images by orientation: _portrait_, _landscape_ or _square_.
- 📁 Recursively scans multiple input directories.
- 🚫 Automatically ignores `.picasaoriginals` folders and everything inside them.
- 📝 Outputs results to customizable text files per orientation.
- 📂 Supports single combined output file with `--all` option.
- ⚡ Uses fast native Windows image APIs for orientation detection.
- ✅ Skips starred entries whose image file no longer exists.
- 📊 Generates logs to aid in troubleshooting.

## 📦 Download

Get the latest version from https://github.com/mrsilver76/starcasa/releases. If you don't want the source code then you should download the exe file. 

You may need to install the [.NET 10.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime) first. 

This program has been tested extensively on Windows 11, but should also work on Windows 10.

## 🚀 Quick start guide

Below are a couple of command scenarios for using StarCasa:

```
StarCasa.exe "D:\Photos" -a "D:\Exports\all-starred.txt"

StarCasa.exe "D:\Photos" --all "D:\Exports\all-starred.txt"
```
- Look at all photos in `D:\Photos`
- Report all starred images to `D:\Exports\all-starred.txt`

```
StarCasa.exe "D:\Photos" "E:\Pictures" -p "D:\Exports\portrait.txt" -l "D:\Exports\landscape.txt"

StarCasa.exe "D:\Photos" "E:\Pictures" --portrait "D:\Exports\portrait.txt" --landscape "D:\Exports\landscape.txt"
```
- Look at all photos in `D:\Photos` and `E:\Pictures`
- Report portrait starred images to `D:\Exports\portrait.txt`
- Report landscape starred images to `D:\Exports\landscape.txt`

```
StarCasa.exe "D:\Photos" -l "D:\Exports\landscape-and-square.txt" -s "D:\Exports\landscape-and-square.txt"

StarCasa.exe "D:\Photos" --landscape "D:\Exports\landscape-and-square.txt" --square "D:\Exports\landscape-and-square.txt"
```
- Look at all photos in `D:\Photos`
- Report both landscape and square images to `D:\Exports\landscape-and-square.txt`

## 💻 Command line options

```
StarCasa.exe <inputDir>... [options]
```
`<inputDir>...` is one or more directories to recursively scan for starred photos.

`[options]` can be 1 or more of the following:

- **`/p <file>`, `-p <file>`, `--portrait <file>`**   
  Output a list of starred portrait images to `<file>`

- **`/l <file>`, `-l <file>`, `--landscape <file>`**   
  Output a list of starred landscape images to `<file>`

- **`/s <file>`, `-s <file>`, `--square <file>`**   
  Output a list of starred square images to `<file>`

- **`/a <file>`, `-a <file>`, `--all <file>`**   
  Output a list of all starred images to `<file>`. You can combine this option with `--portrait`, `--landscape` and `--square`. 
  
>[!NOTE]
>The use of `--all` on its own does not require image orientation to be checked. As a result, it is significantly faster than combining with `--portrait`, `--landscape` and/or `--square`.
  
- **`-nc`, `--no-check`**  
  Disables GitHub version checks for StarCasa.
  
  Version checks occur at most once every 7 days. StarCasa connects only to [this URL](https://api.github.com/repos/mrsilver76/starcasa/releases/latest) to retrieve version information. No data about you or your Picasa library is shared with the author or GitHub - you can verify this yourself by reviewing `GitHubVersionChecker.cs`

- **`-v`, `--verbose`**  
  Show the detailed log lines on the console as well as in the log file.

- **`/?`, `-h`, `--help`**  
  Displays the full help text with all available options, credits and the location of the log files.

Logs are written to `%APPDATA%\StarCasa\Logs` and one file per day in the format `log-yyyy-MM-dd.log`. Files older than 14 days are deleted at startup.

## 🛟 Questions/problems?

Please raise an issue at https://github.com/mrsilver76/starcasa/issues.

## 💡 Future development: open but unplanned

StarCasa currently meets the needs it was designed for, and no major new features are planned at this time. However suggestions are welcome as a [feature request](https://github.com/mrsilver76/starcasa/issues).

## 📝 Attribution

- Picasa is a trademark of Google LLC. This tool is not affiliated with or endorsed by Google LLC.
- Picture icons created by Freepik - Flaticon (https://www.flaticon.com/free-icons/picture)

## Version history

The full list of changes for every version is in [CHANGELOG.md](CHANGELOG.md).
