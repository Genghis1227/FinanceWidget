# Release Notes - v0.9.4

## What's New in v0.9.4

### ✨ Improved Beta Site Startup
Resolved a recurring UI issue where a dark "Search or ask a question" panel (Google Finance Research bar) would appear at the bottom of the widget upon initial launch.

### 🛠️ Key Fixes & Enhancements
*   **Auto-Interaction Simulation**: The widget now simulates a one-time background click shortly after loading the Beta site. This automatically dismisses "first-load" overlays, tooltips, and floating banners that Google sometimes displays to new or non-logged-in users.
*   **Enhanced UI Cleanup Script**: Added more aggressive and precise CSS selectors to the cleanup logic. This ensures that floating search bars and AI research panels are immediately hidden and neutralized, preventing them from blocking the chart or X-axis labels.
*   **Improved Layout Stability**: By removing these intrusive elements programmatically, the widget maintains its clean, borderless appearance even when Google introduces new promotional UI components.

---
*For the full history of changes, see the [Archive](Archive/) folder.*
