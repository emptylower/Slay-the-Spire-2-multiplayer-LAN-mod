#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ASSEMBLY_NAME="sts2_lan_connect"
BUILD_SCRIPT="$ROOT_DIR/scripts/build-sts2-lan-connect.sh"
PROJECT_DIR="$ROOT_DIR/sts2-lan-connect"
PACKAGE_ROOT="$PROJECT_DIR/release/$ASSEMBLY_NAME"
PCK_FILE="$PROJECT_DIR/build/$ASSEMBLY_NAME.pck"
DLL_FILE="$PROJECT_DIR/.godot/mono/temp/bin/Debug/$ASSEMBLY_NAME.dll"
MANIFEST_FILE="$PROJECT_DIR/$ASSEMBLY_NAME.json"
GUIDE_FILE="$ROOT_DIR/STS2_LAN_CONNECT_USER_GUIDE_ZH.md"
RELEASE_README="$ROOT_DIR/RELEASE_README.md"
MAC_INSTALLER="$ROOT_DIR/scripts/install-sts2-lan-connect-macos.sh"
WIN_INSTALLER="$ROOT_DIR/scripts/install-sts2-lan-connect-windows.ps1"
WIN_INSTALLER_BAT="$ROOT_DIR/scripts/install-sts2-lan-connect-windows.bat"
MOD_VERSION="$(grep -E '"version"' "$MANIFEST_FILE" | head -n 1 | sed -E 's/.*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')"
RELEASE_VERSION="v$MOD_VERSION"

BUILD_ARGS=()
[[ -n "${STS2_ROOT:-}" ]] && BUILD_ARGS+=(--game-dir "$STS2_ROOT")
[[ -n "${GODOT_BIN:-}" ]] && BUILD_ARGS+=(--godot-bin "$GODOT_BIN")
[[ -n "${DOTNET_BIN:-}" ]] && BUILD_ARGS+=(--dotnet-bin "$DOTNET_BIN")
"$BUILD_SCRIPT" "${BUILD_ARGS[@]}" --skip-install-copy

rm -rf "$PACKAGE_ROOT"
mkdir -p "$PACKAGE_ROOT"
cp "$DLL_FILE" "$PACKAGE_ROOT/"
cp "$PCK_FILE" "$PACKAGE_ROOT/"
cp "$MANIFEST_FILE" "$PACKAGE_ROOT/"
cp "$RELEASE_README" "$PACKAGE_ROOT/README.md"
cp "$GUIDE_FILE" "$PACKAGE_ROOT/"
cp "$MAC_INSTALLER" "$PACKAGE_ROOT/"
cp "$WIN_INSTALLER" "$PACKAGE_ROOT/"
cp "$WIN_INSTALLER_BAT" "$PACKAGE_ROOT/"

cd "$PROJECT_DIR/release"
rm -f "${ASSEMBLY_NAME}"*-macos.zip
zip -qr "${ASSEMBLY_NAME}-${RELEASE_VERSION}-macos.zip" "$ASSEMBLY_NAME"
echo "Package created at: $PROJECT_DIR/release/${ASSEMBLY_NAME}-${RELEASE_VERSION}-macos.zip"
