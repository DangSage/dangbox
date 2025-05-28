extends Control

@onready var playHost: Button = $VerticalContainer/play_host
@onready var settings: Button = $VerticalContainer/settings
@onready var quit: Button = $VerticalContainer/quit
@onready var playConnect: Button = $VerticalContainer/play_connect


func _onPlayHostPressed() -> void:
	get_tree().change_scene_to_file("res://scenes/test.tscn")

func _onPlayConnectPressed() -> void:
	pass # Replace with function body.

func _onSettingsPressed() -> void:
	pass

func _onQuitPressed() -> void:
	get_tree().quit()
