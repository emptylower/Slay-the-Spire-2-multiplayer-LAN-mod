#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_DIR="$ROOT_DIR/sts2-lan-connect"
ASSEMBLY_NAME="sts2_lan_connect"
PROJECT_FILE="$PROJECT_DIR/$ASSEMBLY_NAME.csproj"
MANIFEST_SOURCE="$PROJECT_DIR/$ASSEMBLY_NAME.json"
PCK_SOURCE="$PROJECT_DIR/build/$ASSEMBLY_NAME.pck"
DLL_SOURCE="$PROJECT_DIR/.godot/mono/temp/bin/Debug/$ASSEMBLY_NAME.dll"
TEMP_MODS_DIR="$ROOT_DIR/.build_output/mods/$ASSEMBLY_NAME"

STS2_ROOT="${STS2_ROOT:-}"
GODOT_BIN="${GODOT_BIN:-}"
DOTNET_BIN="${DOTNET_BIN:-}"
MODS_DIR="${STS2_MODS_DIR:-}"
SKIP_INSTALL_COPY=0

usage() {
  cat <<'EOF'
Usage:
  ./scripts/build-sts2-lan-connect.sh [options]

Options:
  --game-dir <path>       Slay the Spire 2 game root directory.
  --godot-bin <path>      Godot executable path.
  --dotnet-bin <path>     dotnet executable path.
  --mods-dir <path>       Explicit destination for the built mod files.
  --skip-install-copy     Build only; do not copy the DLL/PCK into the game mods directory.
  --help                  Show this help.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --game-dir)
      [[ $# -ge 2 ]] || { echo "--game-dir requires a value" >&2; exit 1; }
      STS2_ROOT="$2"
      shift 2
      ;;
    --godot-bin)
      [[ $# -ge 2 ]] || { echo "--godot-bin requires a value" >&2; exit 1; }
      GODOT_BIN="$2"
      shift 2
      ;;
    --dotnet-bin)
      [[ $# -ge 2 ]] || { echo "--dotnet-bin requires a value" >&2; exit 1; }
      DOTNET_BIN="$2"
      shift 2
      ;;
    --mods-dir)
      [[ $# -ge 2 ]] || { echo "--mods-dir requires a value" >&2; exit 1; }
      MODS_DIR="$2"
      shift 2
      ;;
    --skip-install-copy)
      SKIP_INSTALL_COPY=1
      shift
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

log() {
  printf '[sts2-lan-connect] %s\n' "$*"
}

die() {
  printf '[sts2-lan-connect] ERROR: %s\n' "$*" >&2
  exit 1
}

resolve_dotnet_bin() {
  local candidate

  for candidate in "$DOTNET_BIN" "$(command -v dotnet 2>/dev/null || true)" "$HOME/.dotnet/dotnet"; do
    [[ -n "$candidate" ]] || continue
    [[ -x "$candidate" ]] || continue
    printf '%s\n' "$candidate"
    return 0
  done

  die "dotnet not found. Re-run with --dotnet-bin <path>."
}

resolve_game_dir() {
  local candidate="$1"
  local default_root="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2"
  local library_file="$HOME/Library/Application Support/Steam/steamapps/libraryfolders.vdf"
  local root=""
  local library_root=""

  if [[ -n "$candidate" ]]; then
    if [[ -d "$candidate/SlayTheSpire2.app" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi

    if [[ "${candidate##*.}" == "app" && -d "$candidate" ]]; then
      printf '%s\n' "$(cd "$(dirname "$candidate")" && pwd)"
      return 0
    fi
  fi

  if [[ -d "$default_root/SlayTheSpire2.app" ]]; then
    printf '%s\n' "$default_root"
    return 0
  fi

  if [[ -f "$library_file" ]]; then
    while IFS= read -r library_root; do
      [[ -n "$library_root" ]] || continue
      root="${library_root}/steamapps/common/Slay the Spire 2"
      if [[ -d "$root/SlayTheSpire2.app" ]]; then
        printf '%s\n' "$root"
        return 0
      fi
    done < <(grep -E '"path"' "$library_file" | sed -E 's/.*"path"[[:space:]]+"([^"]+)".*/\1/' | sed 's/\\\\/\\/g')
  fi

  die "Could not locate the Slay the Spire 2 game directory. Re-run with --game-dir <path>."
}

mono_api_dir_for() {
  local executable_path="$1"
  local executable_dir
  executable_dir="$(cd "$(dirname "$executable_path")" && pwd)"

  for candidate in \
    "$executable_dir/GodotSharp/Api/Debug" \
    "$executable_dir/../Resources/GodotSharp/Api/Debug"; do
    if [[ -d "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  return 1
}

resolve_godot_bin() {
  local candidate=""
  local lowered=""
  local mono_api_dir=""

  for candidate in \
    "$GODOT_BIN" \
    "$HOME/.local/bin/godot451" \
    "$HOME/.local/bin/godot45" \
    "$HOME/.local/bin/godot4" \
    "/Applications/Godot.app/Contents/MacOS/Godot" \
    "$HOME/Applications/Godot.app/Contents/MacOS/Godot" \
    "$(command -v godot 2>/dev/null || true)" \
    "$HOME/.local/bin/godot451-mono" \
    "/Applications/Godot_mono.app/Contents/MacOS/Godot" \
    "$HOME/Applications/Godot_mono.app/Contents/MacOS/Godot"; do
    [[ -n "$candidate" ]] || continue
    [[ -x "$candidate" ]] || continue

    lowered="$(printf '%s' "$candidate" | tr '[:upper:]' '[:lower:]')"
    if [[ "$lowered" == *mono* ]]; then
      if mono_api_dir="$(mono_api_dir_for "$candidate")"; then
        :
      else
        log "Skipping incomplete mono Godot candidate '$candidate' because GodotSharp/Api/Debug is missing."
        continue
      fi
    fi

    printf '%s\n' "$candidate"
    return 0
  done

  die "Could not locate a Godot executable. Re-run with --godot-bin <path>."
}

assert_supported_godot_version() {
  local executable_path="$1"
  local version_text

  version_text="$("$executable_path" --version 2>/dev/null | head -n 1 | tr -d '\r')"
  [[ -n "$version_text" ]] || die "Could not determine Godot version from '$executable_path'."

  case "$version_text" in
    4.5.1*) ;;
    *)
      die "Unsupported Godot version '$version_text'. Use Godot 4.5.1 to build a compatible PCK for Slay the Spire 2."
      ;;
  esac
}

RESOLVED_DOTNET_BIN="$(resolve_dotnet_bin)"
RESOLVED_GAME_DIR="$(resolve_game_dir "$STS2_ROOT")"
RESOLVED_GODOT_BIN="$(resolve_godot_bin)"
RESOLVED_MODS_DIR="${MODS_DIR:-$RESOLVED_GAME_DIR/SlayTheSpire2.app/Contents/MacOS/mods/$ASSEMBLY_NAME}"

assert_supported_godot_version "$RESOLVED_GODOT_BIN"

log "Using game dir: $RESOLVED_GAME_DIR"
log "Using Godot: $RESOLVED_GODOT_BIN"
mkdir -p "$TEMP_MODS_DIR"

"$RESOLVED_DOTNET_BIN" build "$PROJECT_FILE" "/p:Sts2Root=$RESOLVED_GAME_DIR" "/p:Sts2ModsDir=$TEMP_MODS_DIR"
"$RESOLVED_GODOT_BIN" --headless --path "$PROJECT_DIR" --script "$PROJECT_DIR/tools/build_pck.gd"

[[ -f "$DLL_SOURCE" ]] || die "Expected DLL not found: $DLL_SOURCE"
[[ -f "$PCK_SOURCE" ]] || die "Expected PCK not found: $PCK_SOURCE"
[[ -f "$MANIFEST_SOURCE" ]] || die "Expected manifest not found: $MANIFEST_SOURCE"

if [[ "$SKIP_INSTALL_COPY" -eq 1 ]]; then
  log "Build complete. Install copy skipped."
  exit 0
fi

mkdir -p "$RESOLVED_MODS_DIR"
rm -f "$RESOLVED_MODS_DIR/"*.dll "$RESOLVED_MODS_DIR/"*.pck "$RESOLVED_MODS_DIR/"*.json
cp "$DLL_SOURCE" "$RESOLVED_MODS_DIR/"
cp "$PCK_SOURCE" "$RESOLVED_MODS_DIR/"
cp "$MANIFEST_SOURCE" "$RESOLVED_MODS_DIR/"

log "MOD files copied to: $RESOLVED_MODS_DIR"
