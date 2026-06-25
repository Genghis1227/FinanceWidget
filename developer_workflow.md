# Developer Workflow Guide

To ensure consistency, safety, and clear alignment, all contributors must strictly adhere to the following workflow sequence for all code changes. Failure to follow this sequence is a breach of project protocol.

## 1. Branching
* No Direct Commits: Never commit directly to the main or production branch.
* Feature/Fix Isolation: Always create a new feature or fix branch for any task.
  Command: git checkout -b <prefix>-<issue-name>
  (e.g., feature-login-ui or fix-auth-token)

---

## 2. Implementation & Pushing
* Verify Changes: Complete code changes and ensure all local unit tests and linting suites pass successfully.
* Documentation Review: Before finalizing, verify if documentation (e.g., README.md, FEATURES.md, or API specs) needs an update to reflect your changes.
* Push Remote: Push the local branch to the remote repository.
* Hold: STOP and inform the project owner once the branch is pushed. Do not proceed to the next step automatically.

---

## 3. Pull Requests (PR)
* Owner Authorization: Do NOT create a Pull Request until the project owner explicitly requests or approves it.
* Version Check: When a PR is requested, first ask the project owner if the project version number should be incremented.
* PR Creation: Once the version status is confirmed, create the PR via the GitHub CLI or Web UI. 

---

## 4. Merging
* Explicit Permission: Do NOT merge the Pull Request until the project owner explicitly provides permission to merge.
* Strategy Alignment: Use the specific merge strategy preferred by the owner for this repository (e.g., Standard Merge, Squash and Merge, or Rebase).

---

## 5. Cleanup & Release
* Sync Local: Only after the PR is successfully merged, switch back to your local main branch and pull the latest changes.
* Repository Hygiene: Delete the feature/fix branch from both the local and remote repositories to keep the branch history clean.
  Commands: 
  git branch -d <branch-name>
  git push origin --delete <branch-name>
* Release Notes: Provide a concise summary of the changes formatted for the target platform. If deploying an application (e.g., Google Play Store), the notes must strictly adhere to the following safety and character constraints:
  * Character Limit: Strictly capped at a maximum of 512 characters per language.
  * No Emojis: Do not include emojis or special decorative icons.
  * No Excessive Capitalization: Avoid using ALL CAPS for emphasis.
  * Compliance: Do not include promotional language, marketing copy, or text that incentivizes app installs. Focus entirely on user-facing functional changes or fixes.
