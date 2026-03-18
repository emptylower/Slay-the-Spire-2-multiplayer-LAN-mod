extends SceneTree

const OUTPUT_DIR := "res://build"
const OUTPUT_FILE := "res://build/sts2_lan_connect.pck"
const ROOT_FILES := [
	"res://mod_image.png"
]
const RESOURCE_DIRS := [
	"res://assets",
	"res://localization",
	"res://resources",
	"res://scenes"
]
const SKIP_EXTENSIONS := [
	".cs",
	".csproj",
	".sln",
	".import",
	".uid"
]
const SKIP_DIR_NAMES := {
	".git": true,
	".godot": true,
	".mono": true,
	"bin": true,
	"obj": true,
	"build": true,
	"tools": true
}

func _initialize() -> void:
	DirAccess.make_dir_recursive_absolute(OUTPUT_DIR)
	var packer := PCKPacker.new()
	var ok := packer.pck_start(OUTPUT_FILE)
	if ok != OK:
		push_error("pck_start failed: %s" % ok)
		quit(1)
		return

	for file_path in ROOT_FILES:
		if FileAccess.file_exists(file_path):
			_add_file(packer, file_path)

	for dir_path in RESOURCE_DIRS:
		if DirAccess.dir_exists_absolute(dir_path):
			_add_dir_recursive(packer, dir_path)

	var flush_ok := packer.flush()
	if flush_ok != OK:
		push_error("flush failed: %s" % flush_ok)
		quit(1)
		return

	print("PCK built: %s" % OUTPUT_FILE)
	quit(0)

func _add_dir_recursive(packer: PCKPacker, dir_path: String) -> void:
	var dir := DirAccess.open(dir_path)
	if dir == null:
		push_error("Could not open dir: %s" % dir_path)
		return

	dir.list_dir_begin()
	while true:
		var entry := dir.get_next()
		if entry == "":
			break
		if entry.begins_with("."):
			continue

		var full_path := "%s/%s" % [dir_path, entry]
		if dir.current_is_dir():
			if SKIP_DIR_NAMES.has(entry):
				continue
			_add_dir_recursive(packer, full_path)
			continue

		if _should_skip_file(entry):
			continue
		_add_file(packer, full_path)

	dir.list_dir_end()

func _add_file(packer: PCKPacker, file_path: String) -> void:
	var add_ok := packer.add_file(file_path, file_path)
	if add_ok != OK:
		push_error("add_file failed: %s %s" % [file_path, add_ok])
		quit(1)

func _should_skip_file(file_name: String) -> bool:
	for extension in SKIP_EXTENSIONS:
		if file_name.ends_with(extension):
			return true
	return false
