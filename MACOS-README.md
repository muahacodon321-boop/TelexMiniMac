# TelexMini — macOS Preview

## Build status
- Native ARM64 input method built with Swift / InputMethodKit.
- Native SwiftUI settings app built and launched.
- Telex composer: 26/26 automated tests passed, including English protection and Telex tone examples.
- Bundle metadata passed `plutil`; both local bundles passed ad-hoc codesign verification.
- Input method locally installed under `~/Library/Input Methods/TelexMini.app`.
- `TISRegisterInputSource` returned 0 (success), and the input server process launched.
- **Not yet integration-verified inside TextEdit, Chrome or password fields**. Newly registered bundle ID did not appear in `TISCreateInputSourceList` during this login session; log out and log back in before UI testing. No automatic logout/reboot performed.

## Paths

```
TelexMiniMac/build/TelexMini.app
TelexMiniMac/build/TelexMini Settings.app
TelexMiniMac/Sources/TelexComposer.swift
TelexMiniMac/Sources/InputMain.swift
TelexMiniMac/Sources/SettingsMain.swift
```

## Build

From `/Volumes/Kodak/projects/ExampleDesign`:

```sh
bash TelexMiniMac/tools/build.sh
```

Requires Xcode + Command Line Tools.

## Install & enable (user steps)

1. Quit any running TelexMini input method before replacing the app (do not overwrite a running binary during an update).
2. Copy `TelexMini.app` to `~/Library/Input Methods/`. This has already been done for the current macOS account.
3. Log out of macOS and log in again (or restart) to refresh text-input source registration for this new bundle ID.
4. Open **System Settings → Keyboard → Text Input → Edit → +**; locate **TelexMini** under Vietnamese or Other and add it if listed.
5. Select TelexMini from the macOS input menu. Keep **ABC/English** installed as a fallback.
6. Open `TelexMini Settings.app` for user settings. Ensure Smart English enabled.
7. Test: `tieengs vieetj` → **tiếng việt**; `office testing password` should remain literal; try mixed phrases.
8. Check secure password entry and both rich-text and web fields; confirm the system selects an appropriate Roman source or passes literal keystrokes. The IMK app does not read password contents.
9. If TelexMini is not listed after re-login, treat registration as a known blocker and do not publish commercially yet.

## Important implementation differences from Windows

Windows uses keyboard hooks and Unicode replacement; macOS uses native IMK marked composition. Their behavior is not identical. The macOS version has not yet been certified across apps.

No globally installed keyboard hook or Accessibility permission is required by the macOS input source. The app does not collect user typing or send it over a network. Only a single in-progress composition remains in memory.

Settings use `UserDefaults(suiteName: "com.telexmini.mac")`. User dictionaries are local at `~/Library/Application Support/TelexMini/EnglishWords.txt` and `VietnameseOverrides.txt`. Activate the input source again to refresh the lexicon.

## Shipping requirements (NOT done)

- **Developer ID Application** code signing, hardened runtime, Apple notarization and stapling. Ad-hoc signing is for this local preview only.
- Integration tests after logging back in (TextEdit, Chrome/Safari, Slack, VS Code, Office) and secure fields.
- Apple Intel (x86_64) build, test and Universal 2 packaging if distributing to older Intel Macs.
- Version migration, installer/uninstaller, update mechanism, localization, documented privacy policy and user consent for any analytics.
- Accessibility and keyboard-only testing, dark/light modes, screen readers, visual UI review.
