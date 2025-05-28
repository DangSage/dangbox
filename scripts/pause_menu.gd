extends Control

func _ready():
	hide()

func resume():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	get_tree().paused = false
	hide()

func pause():
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	get_tree().paused = true
	show()

func testEsc():
	if Input.is_action_just_pressed("esc"):
		if get_tree().paused:
			resume()
		else:
			pause()

func _onButtonResumePressed() -> void:
	resume()

func _onButtonSettingsPressed() -> void:
	pass # Replace with function body.

func _onButtonExitPressed() -> void:
	get_tree().paused = false
	get_tree().change_scene_to_file("res://scenes/mainscreen.tscn")

func _process(_delta):
	testEsc()
