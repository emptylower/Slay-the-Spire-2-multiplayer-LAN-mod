#!/usr/bin/env bash
set -euo pipefail

ASSEMBLY_NAME="sts2_lan_connect"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_DIR="${PACKAGE_DIR:-$SCRIPT_DIR}"
USERDATA_DIR="${STS2_USERDATA_DIR:-$HOME/.local/share/SlayTheSpire2}"
GAME_DIR="${STS2_ROOT:-}"
SYNC_SAVES=1

usage() {
  cat <<'EOF'
Usage:
  ./install-sts2-lan-connect-linux.sh [options]

Options:
  --game-dir <path>     Slay the Spire 2 game root directory.
  --data-dir <path>     Slay the Spire 2 user data directory.
  --package-dir <path>  Directory that contains sts2_lan_connect.dll/.pck/.json.
  --no-save-sync        Install the mod only; skip save migration/sync.
  --help                Show this help.

Behavior:
  1. Copies the mod files into the game's mods/sts2_lan_connect directory.
  2. Performs a one-way sync from non-modded saves into modded saves.
  3. Copies only missing files into modded saves and never overwrites existing modded files.

Notes:
  - Close the game before running this script.
  - Re-run the script any time you want to re-sync vanilla saves into modded saves.
EOF
}

log() {
  printf '[sts2-lan-connect] %s\n' "$*"
}

die() {
  printf '[sts2-lan-connect] ERROR: %s\n' "$*" >&2
  exit 1
}

is_valid_game_dir() {
  local candidate="$1"
  [[ -x "$candidate/SlayTheSpire2" && -d "$candidate/data_sts2_linuxbsd_x86_64" ]]
}

resolve_game_dir() {
  local candidate="$1"
  local root=""
  local library_root=""
  local -a default_roots=(
    "$HOME/.local/share/Steam/steamapps/common/Slay the Spire 2"
    "$HOME/.steam/steam/steamapps/common/Slay the Spire 2"
  )
  local -a library_files=(
    "$HOME/.local/share/Steam/steamapps/libraryfolders.vdf"
    "$HOME/.steam/steam/steamapps/libraryfolders.vdf"
  )

  if [[ -n "$candidate" ]]; then
    if is_valid_game_dir "$candidate"; then
      printf '%s\n' "$candidate"
      return 0
    fi

    if [[ -f "$candidate" && "$(basename "$candidate")" == "SlayTheSpire2" ]]; then
      printf '%s\n' "$(cd "$(dirname "$candidate")" && pwd)"
      return 0
    fi
  fi

  for root in "${default_roots[@]}"; do
    if is_valid_game_dir "$root"; then
      printf '%s\n' "$root"
      return 0
    fi
  done

  for library_file in "${library_files[@]}"; do
    [[ -f "$library_file" ]] || continue

    while IFS= read -r library_root; do
      [[ -n "$library_root" ]] || continue
      root="${library_root}/steamapps/common/Slay the Spire 2"
      if is_valid_game_dir "$root"; then
        printf '%s\n' "$root"
        return 0
      fi
    done < <(grep -E '"path"' "$library_file" | sed -E 's/.*"path"[[:space:]]+"([^"]+)".*/\1/' | sed 's/\\\\/\\/g')
  done

  die "Could not locate the Slay the Spire 2 game directory. Re-run with --game-dir <path>."
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --game-dir)
      [[ $# -ge 2 ]] || die "--game-dir requires a value"
      GAME_DIR="$2"
      shift 2
      ;;
    --data-dir)
      [[ $# -ge 2 ]] || die "--data-dir requires a value"
      USERDATA_DIR="$2"
      shift 2
      ;;
    --package-dir)
      [[ $# -ge 2 ]] || die "--package-dir requires a value"
      PACKAGE_DIR="$2"
      shift 2
      ;;
    --no-save-sync)
      SYNC_SAVES=0
      shift
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      die "Unknown option: $1"
      ;;
  esac
done

if [[ ! -f "$PACKAGE_DIR/$ASSEMBLY_NAME.dll" || ! -f "$PACKAGE_DIR/$ASSEMBLY_NAME.pck" || ! -f "$PACKAGE_DIR/$ASSEMBLY_NAME.json" ]]; then
  die "Package directory '$PACKAGE_DIR' does not contain $ASSEMBLY_NAME.dll, $ASSEMBLY_NAME.pck, and $ASSEMBLY_NAME.json"
fi

RESOLVED_GAME_DIR="$(resolve_game_dir "$GAME_DIR")"
TARGET_MOD_DIR="$RESOLVED_GAME_DIR/mods/$ASSEMBLY_NAME"
mkdir -p "$TARGET_MOD_DIR"

log "Installing mod files to: $TARGET_MOD_DIR"
rm -f "$TARGET_MOD_DIR/"*.dll "$TARGET_MOD_DIR/"*.pck "$TARGET_MOD_DIR/"*.json
cp -f "$PACKAGE_DIR/$ASSEMBLY_NAME.dll" "$TARGET_MOD_DIR/"
cp -f "$PACKAGE_DIR/$ASSEMBLY_NAME.pck" "$TARGET_MOD_DIR/"
cp -f "$PACKAGE_DIR/$ASSEMBLY_NAME.json" "$TARGET_MOD_DIR/"
if [[ -f "$PACKAGE_DIR/STS2_LAN_CONNECT_USER_GUIDE_ZH.md" ]]; then
  cp -f "$PACKAGE_DIR/STS2_LAN_CONNECT_USER_GUIDE_ZH.md" "$TARGET_MOD_DIR/"
fi

if [[ "$SYNC_SAVES" -eq 0 ]]; then
  log "Save sync skipped (--no-save-sync)."
  exit 0
fi

if [[ ! -d "$USERDATA_DIR" ]]; then
  log "User data directory '$USERDATA_DIR' does not exist yet. Installation finished without save sync."
  exit 0
fi

profiles_synced=0
files_copied=0

sync_profile_saves() {
  local platform_name="$1"
  local user_dir="$2"
  local profile_dir="$3"
  local profile_name
  local source_saves
  local dest_profile
  local dest_saves

  profile_name="$(basename "$profile_dir")"
  source_saves="$profile_dir/saves"
  [[ -d "$source_saves" ]] || return

  dest_profile="$user_dir/modded/$profile_name"
  dest_saves="$dest_profile/saves"

  mkdir -p "$dest_saves"

  while IFS= read -r -d '' source_file; do
    local relative_path
    local dest_file
    relative_path="${source_file#$source_saves/}"
    dest_file="$dest_saves/$relative_path"
    mkdir -p "$(dirname "$dest_file")"

    if [[ ! -e "$dest_file" ]]; then
      cp -f "$source_file" "$dest_file"
      files_copied=$((files_copied + 1))
    fi
  done < <(find "$source_saves" -type f -print0)

  profiles_synced=$((profiles_synced + 1))
}

for platform_name in steam default; do
  platform_dir="$USERDATA_DIR/$platform_name"
  [[ -d "$platform_dir" ]] || continue

  while IFS= read -r -d '' user_dir; do
    while IFS= read -r -d '' profile_dir; do
      sync_profile_saves "$platform_name" "$user_dir" "$profile_dir"
    done < <(find "$user_dir" -mindepth 1 -maxdepth 1 -type d -name 'profile*' -print0)
  done < <(find "$platform_dir" -mindepth 1 -maxdepth 1 -type d -print0)
done

log "Save sync finished. Profiles scanned: $profiles_synced, missing files copied: $files_copied"
log "This is a one-way sync from vanilla saves into modded saves. Existing modded files are never overwritten."
