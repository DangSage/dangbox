using Godot;
using System;

public partial class EntityNBT : NBT {
    public EntityNBT() {
        // Initialize default properties
        SetProperty("base_speed", 3.0f); // Float
        SetProperty("jump_velocity", 4.0f); // Float
        SetProperty("sensitivity", 0.001f); // Float
        SetProperty("acceleration", 5.0f); // Float
        SetProperty("gravity", ProjectSettings.GetSetting("physics/3d/default_gravity"));

        // Speed modifiers
        SetProperty("speed_modifiers", new Godot.Collections.Dictionary
        {
            { "crouch", 0.5f }, // Float
            { "sprint", 1.4f }, // Float
            { "default", 1.0f } // Float
        });

        // Inventory
        SetProperty("inventory", new Godot.Collections.Dictionary
        {
            { "items", new Godot.Collections.Array() }, // Array
            { "capacity", 10 } // Int
        });

        // Health
        SetProperty("health", new Godot.Collections.Dictionary
        {
            { "current", 1 }, // Int
            { "max", 1 } // Int
        });

        // Position
        SetProperty("position", new Godot.Collections.Dictionary
        {
            { "x", 0.0f }, // Float
            { "y", 0.0f }, // Float
            { "z", 0.0f } // Float
        });

        // Velocity
        SetProperty("velocity", new Godot.Collections.Dictionary
        {
            { "x", 0.0f }, // Float
            { "y", 0.0f }, // Float
            { "z", 0.0f } // Float
        });

        // Rotation
        SetProperty("rotation", new Godot.Collections.Dictionary
        {
            { "x", 0.0f }, // Float
            { "y", 0.0f }, // Float
            { "z", 0.0f } // Float
        });

        // Rotation in degrees
        SetProperty("rotation_degrees", new Godot.Collections.Dictionary
        {
            { "x", 0.0f }, // Float
            { "y", 0.0f }, // Float
            { "z", 0.0f } // Float
        });

        // Scale
        SetProperty("scale", new Godot.Collections.Dictionary
        {
            { "x", 1.0f }, // Float
            { "y", 1.0f }, // Float
            { "z", 1.0f } // Float
        });
    }

    public float GetCurrentSpeed(string activeModifier) {
        float baseSpeed = GetProperty<float>("base_speed");
        Godot.Collections.Dictionary modifiers = GetProperty<Godot.Collections.Dictionary>("speed_modifiers");

        if (modifiers == null) {
            GD.PrintErr("Speed modifiers dictionary is null!");
            return baseSpeed;
        }
        if (modifiers.Count == 0) {
            GD.PrintErr("Speed modifiers dictionary is empty!");
            return baseSpeed;
        }
        if (string.IsNullOrEmpty(activeModifier)) {
            GD.PrintErr("Active modifier is null or empty!");
            return baseSpeed;
        }
        if (!modifiers.ContainsKey(activeModifier)) {
            GD.PrintErr($"Active modifier '{activeModifier}' not found in speed modifiers!");
            return baseSpeed;
        }

        float modifier = (float)modifiers[activeModifier];
        return baseSpeed * modifier;
    }

    // inherits from NBT all the methods
    
}
