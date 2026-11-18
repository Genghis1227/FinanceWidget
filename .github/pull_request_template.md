## Summary
<!-- Briefly describe what this PR changes and why. -->

## Linked Issues
<!-- Reference any related GitHub issues (e.g., Closes #123 or Refs #123). -->

## Contributor Pre-PR Checklist
- [ ] Local guardrail suite (`node tests/validate-guardrails.js --self-test` and `node tests/validate-guardrails.js`) and strict Release build (`dotnet build FinanceWidget.csproj -c Release -warnaserror`) pass with 0 errors and 0 warnings.
- [ ] Zero secrets, `.env` files, private keys, data dumps, or hardcoded local machine paths introduced.
- [ ] Zero paid/metered APIs, unapproved NuGet packages or feeds, or third-party analytics/telemetry trackers added.
- [ ] Preserved `Trinhism` attribution (`https://trinhism.com/app/`), MIT `LICENSE`, and canonical `Genghis1227/FinanceWidget` repository and updater URLs.
- [ ] Updated `README.md` / documentation to match any user-facing changes.
- [ ] Left `<Version>` in `FinanceWidget.csproj`, `README.md`, and `ReleaseNotes/latest_release_notes.md` untouched unless explicitly approved by `@Genghis1227`.
- [ ] Ready for review and merge by `@Genghis1227` (contributors do not self-merge).
