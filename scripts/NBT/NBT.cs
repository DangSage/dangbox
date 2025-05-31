using Godot;
using System;
using SharpNBT;

namespace DangboxGame.Scripts.NBT {
	[Tool]
	public partial class NBT : Resource {
		private CompoundTag _data;
		protected string _filePath = null;

		public NBT() : this(new CompoundTag("root")) { }

		public NBT(CompoundTag defaultData) {
			_data = defaultData ?? new CompoundTag("root");
			if (!_data.ContainsKey("uuid")) {
				_data.Add(new StringTag("uuid", Guid.NewGuid().ToString()));
			}
		}

		public T GetProperty<T>(string key) {
			if (_data.ContainsKey(key)) {
				var tag = _data[key];
				return tag switch {
					IntTag intTag => (T)Convert.ChangeType(intTag.Value, typeof(T)),
					FloatTag floatTag => (T)Convert.ChangeType(floatTag.Value, typeof(T)),
					StringTag stringTag => (T)Convert.ChangeType(stringTag.Value, typeof(T)),
					DoubleTag doubleTag => (T)Convert.ChangeType(doubleTag.Value, typeof(T)),
					LongTag longTag => (T)Convert.ChangeType(longTag.Value, typeof(T)),
					CompoundTag compoundTag => (T)(object)compoundTag, // Cast to object to avoid type mismatch
					ListTag listTag => (T)(object)listTag, // Cast to object to avoid type mismatch
					_ => throw new InvalidCastException($"Unsupported tag type for key '{key}'.")
				};
			}
			GD.PrintErr($"Key '{key}' not found in NBT data.");
			return default;
		}

		public void SetProperty<T>(string key, T value) {
			Tag tag = value switch {
				int intValue => new IntTag(key, intValue),
				float floatValue => new FloatTag(key, floatValue),
				double doubleValue => new DoubleTag(key, doubleValue),
				long longValue => new LongTag(key, longValue),
				string stringValue => new StringTag(key, stringValue),
				Variant variant when variant.VariantType == Variant.Type.Int => new IntTag(key, (int)variant),
				Variant variant when variant.VariantType == Variant.Type.Float => new FloatTag(key, (float)variant),
				Variant variant when variant.VariantType == Variant.Type.String => new StringTag(key, (string)variant),
				_ => throw new NotSupportedException($"Type {typeof(T)} or Variant type {value?.GetType()} is not supported.")
			};

			if (_data.ContainsKey(key)) {
				_data[key] = tag;
			} else {
				_data.Add(tag);
			}
		}

		public CompoundTag GetAllProperties() => _data;

		public void SetAllProperties(CompoundTag newData) {
			_data = newData;
		}

		virtual public byte[] Serialize(string path = null) {
			using var memoryStream = new System.IO.MemoryStream();
			using (var writer = new TagWriter(memoryStream, FormatOptions.BigEndian)) {
				writer.WriteTag(_data);
			}
			path ??= _filePath;

			if (!string.IsNullOrEmpty(path)) {
				if (path.StartsWith("res://")) {
					GD.PrintErr($"Cannot write to res:// in exported builds. Redirecting to user://.");
					path = path.Replace("res://", "user://");
				}

				System.IO.File.WriteAllBytes(path, memoryStream.ToArray());
				GD.Print($"NBT data serialized to {path}");
			} else {
				GD.Print("NBT data serialized to memory stream.");
			}

			return memoryStream.ToArray();
		}


		public void Deserialize(byte[] bytes) {
				using var stream = new System.IO.MemoryStream(bytes);
				using TagReader reader = new(stream, FormatOptions.BigEndian);
				var tag = reader.ReadTag();

				if (tag == null) {
					GD.PrintErr("Warning: No valid tag found in the provided byte array. Initializing with a new root tag.");
					_data = new CompoundTag("root");
					return;
				}

				if (tag is not CompoundTag compoundTag) {
					GD.PrintErr("Warning: Root tag is not a CompoundTag. Initializing with a new root tag.");
					_data = new CompoundTag("root");
					return;
				}

				_data = compoundTag;
			}

		public override string ToString() => _data.ToString();
	}
}
