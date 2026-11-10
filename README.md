# TelexMini — Windows + macOS source monorepo

TelexMini is a lightweight Vietnamese Telex input utility with an offline English lexicon and local-only settings. **This repository includes the source for both Windows and macOS.** It does not include compiler outputs, credentials, local user dictionaries or binaries.

## Repository layout

```text
TelexMiniMac/
  .gitignore
  README.md                  <- this document
  MACOS-README.md            <- Mac build, install and known limitations
  Sources/
    TelexComposer.swift      <- native macOS engine
    InputMain.swift          <- InputMethodKit input source
    SettingsMain.swift       <- native SwiftUI preferences app
  Resources/
    english-core.txt         <- bundled offline public lexicon
    Input-Info.plist
    Settings-Info.plist
  Tests/
    EngineTests.swift
  tools/
    build.sh                 <- builds two native macOS apps, icon and tests
    MakeIcon.swift
  windows/
    Program.cs               <- WinForms entry + tray, options
    ModernChrome.cs          <- Windows modern UI
    Keyboard.cs              <- Windows keyboard integration
    InputContext.cs          <- password field/context detection
    TelexEngine.cs           <- Windows Telex engine
    EnglishLexicon.cs        <- offline lexicon + user overrides
    UserPreferences.cs       <- local settings
    Tests.cs
    build.ps1
    test.ps1
    HUONG-DAN.md
    COMMERCIAL-ROADMAP.md
```

## Windows (Windows 10/11, .NET Framework 4.x)

From PowerShell inside `windows/`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Build output: `windows/TelexMini.exe` (ignored by Git). Only run one TelexMini instance. The program uses native Windows APIs, so it must be compiled and tested on a Windows machine. A verified previous build exists on the development PC; the bundled copy of source was made from that project.

## macOS (Swift 5 mode, Xcode)

From this repository's parent directory (or adjust the script path):

```bash
bash TelexMiniMac/tools/build.sh
```

Build outputs: `build/TelexMini.app` and `build/TelexMini Settings.app` (ignored by Git). The system input method uses InputMethodKit and the preferences app uses SwiftUI. Both build on Apple Silicon. See [MACOS-README.md](MACOS-README.md) for manual install, setup, tested scope and release blockers.

**macOS is a technical preview, not a commercial release.** Unit tests and builds pass, but first-time input-source activation and typing across apps still need end-to-end verification after logging out/in. The default ad-hoc signature is suitable only for local testing.

## Privacy

Input processing is intended to remain fully local and offline. There is no network-based AI, keylogging archive or cloud telemetry built into the typing path. User-added words and options are local and excluded from Git. Avoid entering real passwords into any demonstration app.

## Roadmap and release

See [windows/COMMERCIAL-ROADMAP.md](windows/COMMERCIAL-ROADMAP.md) for hardening and commercial-release gates. Separate app signing, notarization, end-to-end IME tests, updates, support and licensing are **not** shipped here.

## Git

```bash
cd TelexMiniMac
git status
git log -1 --oneline
# Add your own remote, then push manually when ready:
# git remote add origin <your-repository-url>
# git push -u origin main
```

Source is committed; generated EXEs/DMGs/apps, machine-specific settings and signing credentials are ignored. No remote is configured and no push is performed automatically.
