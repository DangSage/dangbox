using Godot;
using System;
using MessagePack;

[Tool]
public partial class NBT : Resource {
	private Godot.Collections.Dictionary data { get; set; } = new Godot.Collections.Dictionary();

	public NBT() {
		data["uuid"] = GenerateUuid();
	}

	public NBT(Godot.Collections.Dictionary defaultData) {
		defaultData ??= [];
		if (!defaultData.ContainsKey("uuid")) {
			defaultData["uuid"] = GenerateUuid();
		}

		foreach (Variant key in defaultData.Keys) {
			SetProperty(key.ToString(), defaultData[key]);
		}
		data = defaultData;
	}

	public T GetProperty<[MustBeVariant] T>(string key) {
		if (data.ContainsKey(key)) {
			if (data[key] is Godot.Variant variant) {
				return variant.As<T>();
			}
			throw new InvalidCastException($"Property '{key}' is not of type {typeof(T)}.");
		} else {
			GD.PrintErr($"Key '{key}' not found in NBT data.");
			return default;
		}
	}

	public void SetProperty<T>(string key, T value) {
		try {
			// Ensure the value is of a supported type
			data[key] = Variant.From(value);
		} catch (Exception ex) {
			GD.PrintErr($"Failed to set property '{key}' with value '{value}': {ex.Message}");
			throw;
		}
	}

	public void ModifyProperty(string key, object delta) {
		if (data.ContainsKey(key)) {
			Variant value = data[key];
			switch (value.VariantType) {
				case Variant.Type.Int:
					if (delta is int intDelta)
						data[key] = value.As<int>() + intDelta;
					else
						GD.PrintErr($"Delta type mismatch. Expected 'int' for key '{key}'.");
					break;
				case Variant.Type.Float:
					if (delta is float floatDelta)
						data[key] = value.As<float>() + floatDelta;
					else
						GD.PrintErr($"Delta type mismatch. Expected 'float' for key '{key}'.");
					break;
				case Variant.Type.String:
					if (delta is string stringDelta)
						data[key] = value.As<string>() + stringDelta;
					else
						GD.PrintErr($"Delta type mismatch. Expected 'string' for key '{key}'.");
					break;
				default:
					GD.PrintErr($"Modification not supported for type '{value.VariantType}'.");
					break;
			}
		} else {
			GD.PrintErr($"Key '{key}' not found in NBT data.");
		}
	}

	public Godot.Collections.Dictionary GetAllProperties() {
		return data;
	}

	public void SetAllProperties(Godot.Collections.Dictionary newData) {
		data = newData;
	}

	public string GenerateUuid() {
		return Guid.NewGuid().ToString();
	}

	public byte[] Serialize() {
		return MessagePackSerializer.Serialize(data);
	}

	public void Deserialize(byte[] bytes) {
		data = MessagePackSerializer.Deserialize<Godot.Collections.Dictionary>(bytes);
	}
}
