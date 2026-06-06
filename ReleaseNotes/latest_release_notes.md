# Finance Widget v1.0.0 - Release Notes

## What's New
Welcome to Finance Widget v1.0! This major milestone release introduces visual controls to refresh content, a global toggle to hide window title bars/borders for a clean stock-only appearance, and robust cleanup of Google's watchlist overlay buttons.

### ✨ Features
- **Widget Refresh Controls**: Added a visual refresh button (circular reload arrow) at the top-right of the drag handle on each widget. Added a hover highlight and pressed state visual feedback.
- **Tray Menu "Refresh All"**: Added a global "Refresh All" action to the system tray context menu to reload all open widgets simultaneously.
- **Borders & Drag Handle Toggling**: Added a checkable "Hide Title Bars" option in the system tray menu. Toggling this collapses the top gray drag handle on all widgets.
- **Clean Graph-Only Look**: When title bars are hidden, the widget becomes a completely clean, borderless graph. The WindowChrome's `CaptionHeight` is set to 0 dynamically so that the entire top area of the WebView chart is fully interactive.
- **State Persistence**: The "Hide Title Bars" setting is saved and restored automatically across restarts.

### 🛠️ Improvements
- **Robust Watchlist Button Hiding**: Hidden the Google Finance checkmark/plus watchlist buttons ("Add to list", "Remove from list", "Follow", "Following") completely. Using a recursive Shadow DOM traversal and a coordinate bounding box check, this works reliably across different user accounts, languages, and layout states.
- **Prevention of Accidental Dragging**: Prevents the widget from being dragged when you click on the refresh button on the drag handle.

---
*For the full history of changes, see the [Archive](Archive/) folder.*
