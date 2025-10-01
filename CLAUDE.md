# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Lc_auto is a Windows WPF (.NET 8) launcher application designed for automated Lightroom Classic workflows. The app provides:
- Four main actions via buttons (a/b/c/d): video playback, two Lightroom Classic automations, and folder opening
- Always-on-top window management with smart focus handoff to Lightroom during automations
- Scheduled alert popups (HH:mm based)
- Configuration-driven behavior via `config/config.json`

**Target Platform**: Windows 10/11, x64 only
**Language**: Korean UI

## Architecture

**Pattern**: MVVM + Service Layer + Config-driven

**Key Layers**:
- **UI Layer** (WPF): MainWindow with buttons a/b/c/d, alert modals, toast notifications
- **ViewModels**: `MainViewModel`, `AlertsViewModel`
- **Services**:
  - `TopMostManager`: Window z-order control and focus handoff to/from Lightroom
  - `WindowFocusService`: Foreground window control via Win32 interop
  - `AutomationService`: FlaUI (UIA3) executor for Lightroom Classic UI automation
  - `FeatureSelectorRegistry`: Maps `featureId` to UIA selectors/automation scripts
  - `MediaService`: Video playback with single-instance enforcement
  - `FolderService`: Windows Explorer integration
  - `AlertsScheduler`: Time-based modal popup scheduler with duplicate-update logic
  - `ConfigService`: JSON config loading, validation, hot-reload via FileSystemWatcher
  - `LoggingService`: Serilog configuration and helpers

**External Dependencies**:
- **FlaUI** (v4.x): UI Automation for Lightroom Classic
- **ModernWpf** (v0.10.x): UI theming
- **Serilog** (v3.x) + `Serilog.Sinks.File`: Logging with rolling file policy
- **Win32 Interop**: P/Invoke for `SetForegroundWindow`, `ShowWindow`, etc.

## User Flow

The complete user workflow is defined in `docs/PRD.md` §1.2:
- Button **a (촬영 시작)**: Opens input form for "예약자 성함" + "휴대폰 뒤4자리", then runs automation with combined input
- Button **b (내보내기)**: Opens input form for "예약자 성함" + "휴대폰 뒤4자리", then runs export automation with combined input
- Button **c (배경지 설치 동영상 보기)**: Plays instructional video
- Button **d (사진 저장 폴더 열기)**: Opens target folder in Windows Explorer

**Input Validation & Format**:
- **Validation Rules**:
  - "예약자 성함": Non-empty string (Korean, English, spaces allowed)
  - "휴대폰 뒤4자리": Exactly 4 digits (regex: `1234`)
  - Validation failure → show error dialog, do NOT proceed
- **Combined Format**: `{성함}{뒤4자리}` (no separators between fields)
  - Example: "홍길동" + "1234" → `홍길동1234`
  - Example: "Kim Jane" + "5678" → `Kim Jane5678`
  - **Final format**: String + 4-digit number

**Important**: Input values are collected via UI forms before automation runs, validated, combined as specified above, and used once during the automation sequence. No persistent storage beyond the current run.

## Critical Behaviors

### Always-On-Top Management (FR-01)
- App starts with `TopMost=true`
- During automations (buttons a/b):
  1. `TopMostManager` hands off focus to Lightroom Classic (removes app z-order competition)
  2. Automation runs via FlaUI
  3. On completion or Lightroom minimize/exit, app regains `TopMost` within 500ms
- Retry policy: 1s interval, max 3 attempts if TopMost restore fails

### FlaUI Automation (FR-02/FR-03)
- **Selector Registry**: `FeatureSelectorRegistry` maps `featureId` to UIA selectors. Lightroom UI updates may break selectors; versioned in code.
- **Execution**:
  - Timeout per attempt: 30 seconds
  - Retry policy: Initial attempt (1) + retry up to 2 times = **total 3 attempts** (max ~90s)
  - Completion detection: UI state/condition checks (e.g., dialog closed), no artificial delays
- **Edge Cases**:
  - If Lightroom minimizes/exits during automation: restore app focus immediately, show warning toast, mark automation as failed
  - Selector miss/interaction failure: show toast, log context, respect retry policy
- **Localization**: Consider localized titles/control IDs in selectors

### Configuration Management (FR-06)
- **Location**: `config/config.json` next to executable
- **Schema**: `config/config.schema.json` (JSON Schema standard) for validation
- **Hot Reload**: `FileSystemWatcher` monitors changes. On change:
  - Validate against schema → if valid, apply live (e.g., reschedule alerts)
  - If invalid, keep prior config and show toast
- **Validation**: Required fields: `videoPath`, `targetFolder`, `automation.p1/p2`, `alerts`
- **Invalid Config**: Start in limited mode, show toast per missing area, log

### Scheduled Alerts (FR-06)
- **Scheduling**: Local time (HH:mm), tolerance ±1s
- **Modal**: Single dialog with message + OK button
- **Duplicate Rule**: If alert triggers while existing dialog is visible, update dialog content to latest message (no queue)
- **Sleep/Wake**: Skip missed alerts after resume, continue from next schedule

### Video Playback (FR-04)
- If `playerPath` is non-empty but invalid/missing: show warning dialog, do NOT fallback to OS default
- Single-instance policy: track active playback via process handle, disallow overlapping launches

### Folder Open (FR-05)
- Launch `explorer.exe` on `paths.targetFolder`
- Nonexistent path: show warning, do NOT create folder

## Concurrency and Performance

- UI work on `Dispatcher` thread; long-running tasks on background threads
- Use async/await with `CancellationToken` support for automations and timers
- **Idle Targets**: CPU < 3%, RSS < 120 MB (no busy loops, minimal timers, dispose FlaUI resources promptly)

## Logging (NFR-04)

- **Directory**: `log/` (next to exe, created if missing)
- **Policy**: 50 MB per file, rolling, 7-day retention
- **Levels**: INFO (normal), WARN (recoverable), ERROR (failures)
- **Context**: Timestamps, thread, event, `featureId`, elapsed time

## Permissions and Security (NFR-05)

- Base app runs without admin
- If UIA access requires elevation, prompt UAC and log reason
- If denied, show warning and skip that automation

## Packaging and Deployment (NFR-03)

- **Publish**: Single-file, self-contained, `win-x64`
- **Trimming**: Disabled (WPF incompatibility)
- **Signing**: None (ship unsigned)
- **Distribution**: Zip with exe, `config/` seed file, `log/` folder (created at runtime)
- **Versioning**: SemVer with optional pre-release tags (`-alpha.N`, `-beta.N`, `-rc.N`)

## CI/CD

- **Platform**: GitHub Actions
- **Build**: Restore, build, publish single-file. Upload artifacts.
- **Release**: Draft GitHub Release from tags; attach artifacts; generate notes from commits.

## Testing Strategy

- **Primary**: Manual testing against real Lightroom Classic on test workstation
- **Scenarios** (from Acceptance Criteria):
  - AC-01: TopMost behavior and restoration after automation
  - AC-02: Video playback (default/custom player), invalid `playerPath` warning
  - AC-03: Automations with selector success/failure, retries, UI-complete detection
  - AC-04: Folder open and nonexistent path warning
  - AC-05: Alerts at HH:mm, modal OK, duplicate update, sleep-resume
  - AC-06: No admin required; elevation request only when needed
- **Optional**: Unit tests for config validation, scheduler calculations

## Key Risks

- **Selector Fragility**: Lightroom Classic UI updates may invalidate `featureId` selectors. Maintain registry in version control and coordinate with user on updates.
- **Foreground Control Limits**: Windows may deny foreground changes; implement retries and user guidance.
- **Player Edge Cases**: Some players spawn helper processes; use in-progress flag for single-instance enforcement.

## Important Files

- `docs/PRD.md`: Product requirements including user workflow (Korean) — **primary reference for button semantics and flows**
- `docs/Architecture.md`: Module structure and interaction sequences (Korean)
- `docs/Specifications.md`: Technical specs, APIs, NFRs (English)
- `docs/ErrorHandling.md`: Error handling policy and guidelines (Korean)
- `docs/TestStrategy.md`: Testing approach and scenarios (Korean/English)
- `config/config.json`: Runtime configuration (hot-reloadable)
- `config/config.schema.json`: JSON Schema for configuration validation
- `resources/errors.json`: Error messages catalog (single source of truth for all error messages)
- `resources/errors.schema.json`: JSON Schema for error definitions

## Notes for Future Development

- P1 (Next Release): Enhanced logging beyond base rolling policy, tray icon, i18n beyond Korean
- When implementing `FeatureSelectorRegistry`, coordinate with user to finalize `featureId` → selector mappings
- Avoid artificial delays in automation; rely on UI condition checks and FlaUI timeouts
- For any Win32 interop (`SetForegroundWindow`, etc.), consider Windows security policies (may require `AttachThreadInput` workarounds)