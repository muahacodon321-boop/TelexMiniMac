#!/bin/bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD="$ROOT/build"
mkdir -p "$BUILD"
rm -rf "$BUILD/TelexMini.app" "$BUILD/TelexMini Settings.app" "$BUILD/TelexMini.iconset"
INPUT="$BUILD/TelexMini.app"
SETTINGS="$BUILD/TelexMini Settings.app"
mkdir -p "$INPUT/Contents/MacOS" "$INPUT/Contents/Resources"
mkdir -p "$SETTINGS/Contents/MacOS" "$SETTINGS/Contents/Resources"
cp "$ROOT/Resources/Input-Info.plist" "$INPUT/Contents/Info.plist"
cp "$ROOT/Resources/Settings-Info.plist" "$SETTINGS/Contents/Info.plist"
cp "$ROOT/Resources/english-core.txt" "$INPUT/Contents/Resources/english-core.txt"
swiftc -O -swift-version 5 -parse-as-library -framework AppKit -framework InputMethodKit \
    "$ROOT/Sources/TelexComposer.swift" "$ROOT/Sources/InputMain.swift" \
    -o "$INPUT/Contents/MacOS/TelexMiniInput"
swiftc -O -swift-version 5 -parse-as-library -framework SwiftUI -framework AppKit \
    "$ROOT/Sources/SettingsMain.swift" -o "$SETTINGS/Contents/MacOS/TelexMiniSettings"
swiftc -O -swift-version 5 -parse-as-library -framework AppKit \
    "$ROOT/tools/MakeIcon.swift" -o "$BUILD/telexmini-makeicon"
"$BUILD/telexmini-makeicon" "$BUILD/icon-1024.png"
mkdir -p "$BUILD/TelexMini.iconset"
for size in 16 32 128 256 512; do
  sips -s format png -z "$size" "$size" "$BUILD/icon-1024.png" --out "$BUILD/TelexMini.iconset/icon_${size}x${size}.png" >/dev/null
  twice=$((size*2))
  sips -s format png -z "$twice" "$twice" "$BUILD/icon-1024.png" --out "$BUILD/TelexMini.iconset/icon_${size}x${size}@2x.png" >/dev/null
done
iconutil -c icns "$BUILD/TelexMini.iconset" -o "$BUILD/TelexMini.icns"
cp "$BUILD/TelexMini.icns" "$INPUT/Contents/Resources/TelexMini.icns"
cp "$BUILD/TelexMini.icns" "$SETTINGS/Contents/Resources/TelexMini.icns"
# Local-only ad-hoc signature. Distribution must use Developer ID + notarization.
codesign --force --sign - --timestamp=none "$INPUT"
codesign --force --sign - --timestamp=none "$SETTINGS"
plutil -lint "$INPUT/Contents/Info.plist" "$SETTINGS/Contents/Info.plist"
codesign --verify --deep --strict "$INPUT"
codesign --verify --deep --strict "$SETTINGS"
swiftc -swift-version 5 "$ROOT/Sources/TelexComposer.swift" "$ROOT/Tests/EngineTests.swift" -o "$BUILD/telexmini-engine-tests"
(cd "$(dirname "$ROOT")" && "$BUILD/telexmini-engine-tests")
echo "BUILT INPUT: $INPUT"
echo "BUILT SETTINGS: $SETTINGS"
echo "NOTE: For manual test install input .app under ~/Library/Input Methods then enable via System Settings."
