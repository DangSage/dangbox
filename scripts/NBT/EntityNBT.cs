using Godot;
using SharpNBT;

namespace DangboxGame.Scripts.NBT {
	public partial class EntityNBT : NBT {
		public Node Owner { get; set; }

		public EntityNBT(string name = "root") : base(new CompoundTag(name) {
			new FloatTag("base_speed", 3.0f),
			new FloatTag("jump_velocity", 4.0f),
			new FloatTag("sensitivity", 0.005f),
			new FloatTag("acceleration", 6.0f),
			new FloatTag("gravity", (float)ProjectSettings.GetSetting("physics/3d/default_gravity")),
			new CompoundTag("speed_modifiers") {
				new FloatTag("crouch", 0.5f),
				new FloatTag("sprint", 1.4f),
				new FloatTag("default", 1.0f)
			},
			new CompoundTag("inventory") {
				new ListTag("items", TagType.String),
				new IntTag("capacity", 10)
			},
			new CompoundTag("health") {
				new IntTag("current", 1),
				new IntTag("max", 1)
			},
			new ListTag("transform", TagType.Float) {
				// Position (x, y, z)
				new FloatTag("position_x", 0.0f), new FloatTag("position_y", 0.0f), new FloatTag("position_z", 0.0f),
				// Velocity (x, y, z)
				new FloatTag("velocity_x", 0.0f), new FloatTag("velocity_y", 0.0f), new FloatTag("velocity_z", 0.0f),
				// Rotation (x, y, z)
				new FloatTag("rotation_x", 0.0f), new FloatTag("rotation_y", 0.0f), new FloatTag("rotation_z", 0.0f),
				// Scale (x, y, z)
				new FloatTag("scale_x", 1.0f), new FloatTag("scale_y", 1.0f), new FloatTag("scale_z", 1.0f)
			}
		}) { }

		public override byte[] Serialize(string filename = null) {
			if (Owner is Node3D node) {
				var transformTag = GetProperty<ListTag>("transform");
				if (transformTag != null) {
					// Update position
					transformTag[0] = new FloatTag("position_x", node.Position.X);
					transformTag[1] = new FloatTag("position_y", node.Position.Y);
					transformTag[2] = new FloatTag("position_z", node.Position.Z);

					// Update velocity (if applicable, otherwise leave as is)
					// Velocity is not directly available in Node3D, so it may need to be updated elsewhere.

					// Update rotation
					transformTag[6] = new FloatTag("rotation_x", node.Rotation.X);
					transformTag[7] = new FloatTag("rotation_y", node.Rotation.Y);
					transformTag[8] = new FloatTag("rotation_z", node.Rotation.Z);

					// Update scale
					transformTag[9] = new FloatTag("scale_x", node.Scale.X);
					transformTag[10] = new FloatTag("scale_y", node.Scale.Y);
					transformTag[11] = new FloatTag("scale_z", node.Scale.Z);
				}
			}

			return base.Serialize(filename);
		}

		public void ApplyTransform(Node3D node) {
			var transformTag = GetProperty<ListTag>("transform");
			if (transformTag != null && transformTag.Count >= 12) {
				// Apply position
				node.Position = new Vector3(
					((FloatTag)transformTag[0]).Value,
					((FloatTag)transformTag[1]).Value,
					((FloatTag)transformTag[2]).Value
				);

				// Apply velocity (if applicable, otherwise skip)
				// Velocity is not directly applicable to Node3D.

				// Apply rotation
				node.Rotation = new Vector3(
					((FloatTag)transformTag[6]).Value,
					((FloatTag)transformTag[7]).Value,
					((FloatTag)transformTag[8]).Value
				);

				// Apply scale
				node.Scale = new Vector3(
					((FloatTag)transformTag[9]).Value,
					((FloatTag)transformTag[10]).Value,
					((FloatTag)transformTag[11]).Value
				);
			}
		}

		public float GetCurrentSpeed(string activeModifier) {
			float baseSpeed = GetProperty<float>("base_speed");
			if (GetProperty<CompoundTag>("speed_modifiers")?.TryGetValue<FloatTag>(activeModifier, out var tag) == true) {
				return baseSpeed * tag.Value;
			}
			GD.PrintErr($"Invalid or missing speed modifier: '{activeModifier}'");
			return baseSpeed;
		}
	}
}
