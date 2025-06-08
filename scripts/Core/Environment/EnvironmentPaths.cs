using Godot;
using System;
using System.IO;

namespace DangboxGame.Scripts.Core.Environment {
	public static class EnvironmentPaths {
		// Example: Writing to system-specific temporary storage
		public static void WriteTemporaryFile(string filename, byte[] data) {
			try {
				string tempPath = EnvironmentConfig.Instance.GetValue<string>("data_temp");
				string filePath = Path.Combine(tempPath, filename);
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				
				File.WriteAllBytes(filePath, data);
				GD.Print($"Temporary file written to: {filePath}");
			} catch (Exception e) {
				GD.PrintErr($"Failed to write temporary file: {e.Message}");
			}
		}
		
		// Example: Reading from system-specific save directory
		public static byte[] ReadSaveFile(string filename) {
			try {
				string savePath = EnvironmentConfig.Instance.GetValue<string>("data_saves");
				string filePath = Path.Combine(savePath, filename);
				
				if (File.Exists(filePath)) {
					return File.ReadAllBytes(filePath);
				}
			} catch (Exception e) {
				GD.PrintErr($"Failed to read save file: {e.Message}");
			}
			return null;
		}

		// Example: Writing to system-specific save directory
		public static void WriteSaveFile(string filename, byte[] data) {
			try {
				string savePath = EnvironmentConfig.Instance.GetValue<string>("data_saves");
				string filePath = Path.Combine(savePath, filename);
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				
				File.WriteAllBytes(filePath, data);
				GD.Print($"Save file written to: {filePath}");
			} catch (Exception e) {
				GD.PrintErr($"Failed to write save file: {e.Message}");
			}
		}
		
		// Example: Logging with system-specific paths
		public static void WriteLog(string logName, string content) {
			try {
				string logPath = EnvironmentConfig.Instance.GetValue<string>("data_logs");
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				string filename = $"{logName}_{timestamp}.log";
				string filePath = Path.Combine(logPath, filename);
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				
				File.WriteAllText(filePath, content);
			} catch (Exception e) {
				GD.PrintErr($"Failed to write log: {e.Message}");
			}
		}
	}
}
