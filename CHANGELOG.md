# Changelog

All notable changes to StarCasa are listed here, newest first.

## 1.1.0 (xx November 2026)

- StarCasa now requires the .NET 10 Runtime, as support for .NET 8 ended on 10th November 2026.
- Significant changes have been made to the way scanning is performed. This can be up to 4x faster than previously.
- `--check` has been removed. Entries for photos that no longer exist are now always left out of the lists (including `--all`) as there is no speed penalty.
- `--all` can now be used with `--portrait`, `--landscape` and `--square`.
- Added `-v` (`--verbose`) to show the detailed log lines on the console as well as in the log file.
- Added `-nc` (`--no-check`) to skip the check for a newer version on GitHub.
- A progress bar is displayed whilst the orientation of photos is being determined. This is useful on slow network drives.
- Photos stored sideways with an EXIF rotation tag (typical of phone portraits) are now classed by how they display. Before, they were listed as landscape.
- A relative input folder is now correctly turned into an absolute path in the output list. This ensures that other programs using the list can correctly find the files.
- Anything below a `.picasaoriginals` folder is now ignored, not just the folder itself.
- Fixed a bug which meant that if the same filename was used for different orientations (e.g. `-l landscape-and-square.txt -s landscape-and-square.txt`) then one would incorrectly overwrite the other.
- Exit codes are now 0 on success and 1 on any failure, so scripts can tell whether a run worked. Ctrl+C now logs a line before stopping.
- Folders that can't be opened, such as `$RECYCLE.BIN` on a drive root, are skipped instead of stopping the scan.
- A `.picasa.ini` that can't be read is logged, and the scan carries on with the next folder.
- A missing or invalid output folder, or a file name Windows does not allow, is reported before the scan starts rather than after.
- Major refactoring of the code into separate classes, one per file. Also fixed several issues with command line parsing.
- If the same folder is given twice, then it will only be scanned once.
- Cleaned up the output formatting of the tool when running and displaying help.
- The version check gives up after 5 seconds if GitHub is slow or down, and waits 24 hours before trying again after a failure.
- Updated documentation.

## 1.0.0 (01 July 2025)

- 🏁 Initial release. Declared as stable.
