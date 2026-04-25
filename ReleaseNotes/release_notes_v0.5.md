# Release Notes - v0.5.0

## What's New
- **Automated CI/CD**: Added GitHub Actions to automatically build and package the application on every push and pull request.
- **Tag-Based Releases**: Implemented a release system that automatically creates a GitHub Release and uploads the compiled executable when a version tag (e.g., `v0.5.0`) is pushed.
- **.NET 10 Upgrade**: Project is now configured for .NET 10.0, ensuring long-term support and performance improvements.
- **Enhanced Documentation**: 
    - Added a direct **Download** section to the README.
    - Added clear instructions for bypassing Windows SmartScreen warnings.
- **Single-File Executable**: Optimized the build process to produce a single, self-contained `FinanceWidget.exe` for easier distribution.

## Improvements
- Explicit versioning added to the project file for better assembly metadata.
- Cleaned up build workflows for faster verification on GitHub.
