# TelexMini hardening validation — 2026-11-11

## Verification scope

**Windows (VM)**: compiled the .NET Framework WinForms source with the installed Windows CSC compiler, ran the complete `windows/test.ps1` suite (exit 0), and ran deterministic engine stress tests (3,000 words with alternating Telex/VNI and Backspace). Both typing engines preserve the existing regression cases: `boo → bô`, `boons → bốn`, `been → bên`, `boosst → boost`, `parameter`, and repeat-key cancellation. Tests also cover the input hook's installed/removed state, password-field fail-closed policy, game/terminal classification, dictionary editing, and layout column isolation. VNI tests cover `bo6n1 → bốn`, `tie6ng1 → tiếng`, `da9ng8 → đăng`, numeric identifiers, and optional process-name exclusions.

**macOS (Apple Silicon)**: `bash tools/build.sh` passed with **3,324 engine assertions, 0 failures**. Ad-hoc signing and property-list validation passed. The SwiftUI preferences app was launched and its dashboard visually inspected. Built input method was copied to `~/Library/Input Methods/TelexMini.app`. `TISRegisterInputSource` returned `0`, but **TISCreateInputSourceList still reported no TelexMini source in this login session**.

## Integration / compatibility blockers

Windows automated SendInput-to-TextBox integration was **attempted**, but the VM repeatedly moved foreground away from the test form. The test aborted safely with `FAIL integration aborted: test window lost foreground` and **was not counted as a pass**. A second manual-focus attempt could not reliably start the test form. Do not disable the foreground safety check to force a pass. End-to-end insertion in Word, Chrome, Edge, VS Code and password fields has not been verified in this run.

macOS must have a login-session refresh followed by manual enabling of the source under **System Settings → Keyboard → Text Input → Edit**. Until then, actual typing in TextEdit/Safari/Chrome/VS Code and secure fields is **blocked**. A local ad-hoc signature is not equivalent to Developer ID signing or notarization; commercial publication is not yet ready. Registration behavior can be dependent on macOS login/session caches. See Apple InputMethodKit guidance and `tools/install-input.sh`.

Windows DPI configurations (125%, 150%, 200%), multi-monitor behavior, native Word UI, IME composition in web editors and remote desktop sessions remain targeted manual regression areas; unit checks confirm only the two-column layout constraints, not application-level compatibility.

## Safety and operating instructions

- Run at most one Windows `TelexMini.exe` process. The next build **overwrites the same file**, never a versioned EXE.
- Stop any other Vietnamese input utility during Windows hook integration tests, and use only a disposable text box, not a user's document.
- Test password handling with an empty scratch secure input field; never use real passwords in diagnostics.
- VNI support is opt-in. Telex remains the default.
- Auto-start uses the current user's Run key on Windows, ServiceManagement login item for the macOS Settings app; macOS registration may fail for local preview signatures and reports the error.
- Application exclusions match exact Windows process names or macOS bundle IDs. No keystroke history or captured passwords are persisted.
