# Finance Widget v0.9.1 - Release Notes

## What's New
This minor update addresses a usability issue with the widget's context menu on the title bar.

### 🐛 Bug Fixes
- **Context Menu Fix**: Right-clicking the top gray drag handle now correctly displays the custom Finance Widget menu (Settings, Add Widget, etc.) instead of the standard Windows system menu.
- **Improved Hit Testing**: Refined how mouse clicks are handled in the window chrome to ensure application events are prioritized over system events in the title bar area.

---

# Finance Widget v0.9.0 - Release Notes

## What's New
This release introduces the highly requested **Resizable Widgets** feature, allowing you to tailor each widget's size to your desktop layout.

### ✨ Features
- **Dynamic Resizing**: You can now freely resize any widget by dragging its edges or the bottom-right corner.
- **Responsive Layout**: The internal Google Finance charts and data will automatically scale and reposition themselves to fit the new widget dimensions.
- **Size Persistence**: Your preferred widget dimensions are now saved and will be restored automatically when you restart the application.

### 🛠️ Improvements
- **Airspace Fix**: Implemented a subtle transparent margin around the browser container to ensure the embedded web view doesn't block window resizing events.
- **Updated State Model**: Enhanced the app's internal state management to handle window width and height.

### 🐛 Bug Fixes
- Resolved an issue where the embedded web browser could intercept and block mouse interactions with the window's resize handles.
