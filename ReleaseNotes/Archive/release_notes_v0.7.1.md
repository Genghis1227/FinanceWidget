# Release Notes - v0.7.1

## Fixes
- **GitHub Release Dependencies**: Fixed an issue where the distributed executable would not run due to missing native dependencies (WebView2). 
- **ZIP Distribution**: The application is now distributed as a ZIP archive containing all necessary components, ensuring a "just run" experience on all supported Windows systems.

## Improvements
- **Native Library Extraction**: Enabled native library extraction for the single-file executable to improve compatibility and runtime reliability.
- **Automated Workflow**: Updated the GitHub Actions pipeline to bundle and zip the full release package automatically.
