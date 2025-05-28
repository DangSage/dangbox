@tool
extends EditorPlugin

func _enter_tree():
	# Register the NBT resource type
	add_custom_type(
		"NBT",  # Name of the custom type
		"Resource",  # Base type
		preload("res://scripts/NBT/NBT.cs"),  # Path to the C# script
		preload("res://icon.svg")  # Optional icon for the custom type
	)

func _exit_tree():
	# Unregister the NBT resource type
	remove_custom_type("NBT")
