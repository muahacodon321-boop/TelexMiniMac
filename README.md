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

Build output: `windows/TelexMini.exe` (ignored by Git). The build **overwrites this one canonical executable** instead of creating version-suffixed filenames. Close any running `TelexMini.exe` before rebuilding. Only run one TelexMini instance. The program uses native Windows APIs, so it must be compiled and tested on a Windows machine. A verified previous build exists on the development PC; the bundled copy of source was made from that project.

## macOS (Swift 5 mode, Xcode)

From this repository's parent directory (or adjust the script path):

```bash
bash TelexMiniMac/tools/build.sh
```

Build outputs: `build/TelexMini.app` and `build/TelexMini Settings.app` (ignored by Git). The system input method uses InputMethodKit and the preferences app uses SwiftUI. Both build on Apple Silicon. See [MACOS-README.md](MACOS-README.md) for manual install, setup, tested scope and release blockers.

**macOS is a technical preview, not a commercial release.** Unit tests and builds pass, but first-time input-source activation and typing across apps still need end-to-end verification after logging out/in. The default ad-hoc signature is suitable only for local testing.

## Windows settings and dictionary UI

The WinForms dashboard uses an explicit two-column layout so the sidebar cannot overlap the content. The light/dark palette has readable button labels, an accent-mode switch, and an in-app typing preview. Both **English dictionary** and **Vietnamese Telex preference** menus open native searchable add/edit/remove dialogs directly in TelexMini (no Notepad). Changes are applied when saved, and user files are kept in `%LOCALAPPDATA%\\TelexMini`; older portable TXT files beside the EXE remain readable.

## English typing reliability

The offline lexicon includes common development terms such as `parameter`, `parameters`, `parameterized`, `parameterization`, `parallel` and `parser`. With Smart English enabled, it protects **each prefix as you type**, not only the completed word. Regression tests assert that `p`, `pa`, `par`, `para`, ... remain unchanged through `parameter` and verify Vietnamese Telex examples still work.

### Immediate Telex shaping (smart bilingual)

When Vietnamese mode is on, a second vowel key now performs its Telex
transformation **immediately**, even when the raw letters look like an English
prefix. For example: `bo` → `boo` produces **bô** at the second `o`; `boon`
becomes **bôn** and `boons` becomes **bốn**. The same applies to `aa → â`
and `ee → ê`. English words such as `book`, `books`, `good` and `boost`
are recovered once recognized. Intermediate rendering of ambiguous English
words may temporarily show a Telex accent: this is unavoidable while also
requiring immediate, speculative Vietnamese accents.

### Vietnamese-first when an explicit Telex shape is valid

In Vietnamese mode, the user explicitly typing a vowel-shaping sequence takes
priority over an English dictionary match **when the resulting syllable is valid**.
For example, `been → bên`, `bee → bê`, `teen → tên`, and `seen → sên`
appear immediately and stay Vietnamese at word boundaries. English terms that
cannot form valid Vietnamese syllables, such as `book` and `good`, are
recovered as English. Fully ambiguous words (for example `boost → bốt`)
will now favor Vietnamese; select English mode to type those literally.

### Smart Bilingual v2: algorithmic detection

The typing path now uses three **offline** signals, not just a list of known English words:

1. **Word/prefix dictionary:** known English words stay literal from the first character.
2. **Foreign onset detection:** tokens beginning with clearly non-Vietnamese clusters (e.g. `pr`, `fl`, `cr`, `wh`, `st`, or `p` followed by anything other than `h`) enter literal mode while typing, even when absent from the dictionary.
3. **Syllable impossibility check:** when a provisional Telex transformation can no longer form a Vietnamese syllable (such as a second vowel group after a consonant or an illegal coda), the engine immediately restores the original key sequence and keeps the rest of that token literal.

`VietnameseOverrides.txt` is checked as prefixes as well, so a custom Vietnamese sequence can override English detection **before** its final character. User-added English words still work as before. Tests cover unfamiliar English technical terms and immediate Vietnamese accent insertion with both implementations.

**Limitations:** Completely ambiguous prefixes can still temporarily acquire a Telex accent before later characters disambiguate them. Some unknown English words are also indistinguishable from valid Telex syllables; no purely local, letter-by-letter heuristic can always infer the intended language. For those, add a word to `EnglishWords.txt`, toggle Smart English or use per-word Vietnamese overrides. No typing data is uploaded or recorded.

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

Source is committed; generated EXEs/DMGs/apps, machine-specific settings and signing credentials are ignored. Git remotes are managed by the repository owner; no push is performed automatically.
