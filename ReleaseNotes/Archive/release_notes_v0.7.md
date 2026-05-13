# Finance Widget v0.7.1 - Release Notes

## What's New
This update addresses distribution issues and improves compatibility for the standalone executable.

### 🐛 Bug Fixes
- **GitHub Release Dependencies**: Fixed an issue where the distributed executable would not run due to missing native dependencies (WebView2). 
- **ZIP Distribution**: The application is now distributed as a ZIP archive containing all necessary components, ensuring a "just run" experience.

### 🛠️ Improvements
- **Native Library Extraction**: Enabled native library extraction for the single-file executable to improve compatibility and runtime reliability.
- **Automated Workflow**: Updated the GitHub Actions pipeline to bundle and zip the full release package automatically.

---

# Finance Widget v0.7.0 - Release Notes

## What's New
This release adds tray icon interactions and improved window management.

### ✨ Features
- **Tray Icon Double-Click**: Added a highly requested feature to bring all active widgets to the foreground by double-clicking the system tray icon.
- **Improved Window Management**: Widgets now automatically restore from a minimized state when activated via the tray icon.
- **Topmost Priority**: Improved focus handling to ensure widgets appear on top of other applications when summoned.

### 🛠️ Improvements
- **Version Bump**: Incremented version to 0.7.0 in preparation for the automated release.
- **Workflow Reliability**: Validated the new automated release pipeline with the updated versioning.
