# Finance Widget v0.6.0 - Release Notes

## What's New
This release features a premium icon refresh and significant enhancements to the Beta site cleanup logic.

### ✨ Features
- **Premium Icon Refresh**: Replaced the original icon with a sleek, modern 3D neon trend design featuring dark-mode optimized aesthetics and glassmorphism effects.

### 🛠️ Improvements
- **Enhanced Beta UI Cleanup**: 
    - Implemented robust logic to hide intrusive "Overview", "Financials", and "About" sections.
    - Added specialized code to detect and remove AI Insights that previously cluttered the Beta chart view.
    - Improved title visibility by adding top padding and refining scroll-offset logic.
- **Layout Stabilization**: Improved the Beta cleanup script to be more reliable and less aggressive, preventing "blank screen" issues.
- **Project Organization**: Created a dedicated `ReleaseNotes` folder to house version documentation.
- **Icon Tooling**: Updated `convert_icon.ps1` with advanced auto-cropping logic to ensure icons are always centered.
- **Version Alignment**: Synced versioning across the project file and public documentation.
