#!/bin/bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INPUT="$ROOT/build/TelexMini.app"
TARGET="$HOME/Library/Input Methods/TelexMini.app"
test -d "$INPUT" || { echo "Run bash tools/build.sh first"; exit 2; }
mkdir -p "$HOME/Library/Input Methods"
# Never use sudo; an input method belongs to the current user's profile.
rm -rf "$TARGET"
ditto "$INPUT" "$TARGET"
codesign --verify --deep --strict "$TARGET"
LSREGISTER="/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister"
if test -x "$LSREGISTER"; then "$LSREGISTER" -f "$TARGET" || true; fi
swiftc -framework Carbon "$ROOT/tools/RegisterInput.swift" -o "$ROOT/build/telexmini-register"
"$ROOT/build/telexmini-register" "$TARGET"
echo "Install location: $TARGET"
