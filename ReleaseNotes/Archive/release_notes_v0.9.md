# Finance Widget v0.9.5 - Release Notes

## What's New
This release improves the internal version tracking system and standardizes the application's documentation history.

### 🛠️ Improvements
- **Dynamic Version Detection**: The "Check for Updates" feature now dynamically retrieves the application's version from the assembly. This ensures update notifications are always accurate and eliminates the need for manual version strings in the source code.
- **Documentation Standardization**: Consolidated all patch release notes into a single cumulative file and standardized titles and headers across the entire project history (v0.5.0 through v0.9.5) for a more professional and consistent experience.
- **Tray Icon Enhancement**: The system tray icon now displays the current version number in its tooltip (e.g., `Finance Widget Manager (v0.9.5)`), providing a quick way to verify which version is running.

---

# Finance Widget v0.9.4 - Release Notes

## What's New
This update resolves a persistent UI issue with the Google Finance Beta site and improves the widget's startup stability.

### 🛠️ Improvements
- **Improved Beta Site Startup**: Resolved a recurring UI issue where a dark "Search or ask a question" panel (Google Finance Research bar) would appear at the bottom of the widget upon initial launch.
- **Auto-Interaction Simulation**: The widget now simulates a one-time background click shortly after loading the Beta site. This automatically dismisses "first-load" overlays, tooltips, and floating banners.
- **Enhanced UI Cleanup Script**: Added more aggressive and precise CSS selectors to the cleanup logic to ensure promotional UI components are immediately hidden.
- **Improved Layout Stability**: Maintained a clean, borderless appearance by programmatically neutralizing intrusive elements.

---

# Finance Widget v0.9.3 - Release Notes

## What's New
This release focuses on improving the Google Login experience and ensuring the application state always matches the site version being displayed.

### 🛠️ Improvements
- **Reliable Login Redirect**: Fixed an issue where logging into Google would always redirect you to the Classic site. The application now correctly remembers your preference.
- **Automatic UI Synchronization**: The "Use Google Beta Site" menu checkbox and the widget's background color now automatically update to match the actual version of the site being displayed.
- **Enhanced Visual Consistency**: Consolidated styling logic to ensure that widget backgrounds and UI elements perfectly match the signature themes of both the Classic and Beta interfaces.

---

# Finance Widget v0.9.2 - Release Notes

## What's New
This release simplifies the widget experience by streamlining settings and making the modern Google Beta interface the default view.

### 🛠️ Improvements
- **Simplified Settings**: Removed the separate "Settings" window. You can now toggle the **"Use Google Beta Site"** option directly from the widget's context menu.
- **Beta by Default**: All new widgets now default to the modern Google Finance Beta interface for a cleaner, more feature-rich experience out of the box.
- **Easy Ticker Updates**: To change a widget's ticker, simply spawn a new widget with the desired symbol. This reduces UI clutter and keeps the workflow focused.

---

# Finance Widget v0.9.1 - Release Notes

## What's New
This minor update addresses a usability issue with the widget's context menu on the title bar.

### 🐛 Bug Fixes
- **Context Menu Fix**: Right-clicking the top gray drag handle now correctly displays the custom Finance Widget menu instead of the standard Windows system menu.
- **Improved Hit Testing**: Refined how mouse clicks are handled in the window chrome to ensure application events are prioritized over system events.

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
- **Browser Interaction Fix**: Resolved an issue where the embedded web browser could intercept and block mouse interactions with the window's resize handles.
