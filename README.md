# StarCasa
_A Windows command line tool that scans your Picasa photo folders and generates one or more plain text lists of your starred images, filtered by orientation (portrait, landscape, or square)_

>[!TIP]
>Looking to showcase your starred portraits? Use StarCasa to list them, then feed that list into [SideBySide](https://github.com/mrsilver76/sidebyside) to combine pairs into perfect landscape shots - ideal for digital photo frames that hate vertical images.

## 🧰 Features
- ⭐ Detects and lists starred photos from Picasa `.picasa.ini` files
- 🖼️ Classifies images by orientation: portrait, landscape, square
- 📁 Recursively scans multiple input directories
- 🚫 Automatically ignores `.picasaoriginals` folders
- 📝 Outputs results to customizable text files per orientation
- 📂 Supports single combined output file with `--all` option
- ⚡ Uses fast native Windows image APIs for orientation detection
- ✅ Optional file existence checking before reporting (`--check`)
- 📊 Logs summary of files found and written per orientation

## 📦 Download

Get the latest version from https://github.com/mrsilver76/starcasa/releases. If you don't want the source code then you should download the exe file. 

You may need to install the [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) first. 

This program has been tested extensively on Windows 11, but should also work on Windows 10.

## 🚀 Quick start guide

Below are a couple of command scenarios for using StarCasa:

```
StarCasa.exe "D:\Photos" -a "D:\Exports\all-starred.txt" -c

StarCasa.exe "D:\Photos" --all "D:\Exports\all-starred.txt" --check
```
- Look at all photos in `D:\Photos`
- Export all starred images to `D:\Exports\all-starred.txt`
- Skip checking if the files exist

```
StarCasa.exe "D:\Photos" "E:\Pictures" -p "D:\Exports\portrait.txt" -l "D:\Exports\landscape.txt"

StarCasa.exe "D:\Photos" "E:\Pictures" --portrait "D:\Exports\portrait.txt" --landscape "D:\Exports\landscape.txt"
```
- Look at all photos in `D:\Photos` and `E:\Pictures`
- Export portrait starred images to `D:\Exports\portrait.txt`
- Export landscape starred images to `D:\Exports\landscape.txt`

```
StarCasa.exe "D:\Photos" -l "D:\Exports\landscape-and-square.txt" -s "D:\Exports\landscape-and-square.txt"

StarCasa.exe "D:\Photos" --landscape "D:\Exports\landscape-and-square.txt" --square "D:\Exports\landscape-and-square.txt"
```
- Look at all photos in `D:\Photos`
- Export both portrait and square images to `D:\Exports\landscape-and-square.txt`

## 💻 Command line options

```
StarCasa.exe <inputDir1> [<inputDir2> ...] [options]
```
`<inputDirN>` specifies one or more directories to recursively scan for starred photos.

`[options]` can be 1 or more of the following:

- **`/p <file>`, `-p <file>`, `--portrait <file>`**   
  Output a list of starred portrait images to `<file>`

- **`/l <file>`, `-l <file>`, `--landscape <file>`**   
  Output a list of starred landscape images to `<file>`

- **`/s <file>`, `-s <file>`, `--square <file>`**   
  Output a list of starred square images to `<file>`

- **`/a <file>`, `-a <file>`, `--all <file>`**   
  Output a list of all starred images to `<file>`

- **`/c`, `-c`, `--check`**   
  Only include images that physically exist on disk. This will slow down scanning.

- **`/?`, `-h`, `--help`**  
  Displays the full help text with all available options, credits and the location of the log files.

## 🛟 Questions/problems?

Please raise an issue at https://github.com/mrsilver76/starcasa/issues.

## 💡 Future development: open but unplanned

StarCasa currently meets the needs it was designed for, and no major new features are planned at this time. However, the project remains open to community suggestions and improvements. If you have ideas or see ways to enhance the tool, please feel free to submit a [feature request](https://github.com/mrsilver76/starcasa/issues).

## 📝 Attribution

- Picasa is a trademark of Google LLC. This tool is not affiliated with or endorsed by Google LLC.
- Picture icons created by Freepik - Flaticon (https://www.flaticon.com/free-icons/picture)

## Version history

### 1.0.0 (xx June 2025)
- 🏁 Initial release. Declared as stable.
