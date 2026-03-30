using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CueTime.Classes.Utilities;

namespace CueTime.Classes.Statistics
{
    internal static class AtomicJsonFileStore
    {
        private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

        public static JsonSerializerOptions CreateDefaultOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public static bool TryLoad<T>(string filePath, out T? value, JsonSerializerOptions? options = null)
        {
            value = default;

            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return false;
                }

                string json = File.ReadAllText(filePath);
                value = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                return value != null;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere il file JSON '{filePath}'.", ex);
                value = default;
                return false;
            }
        }

        public static T? Load<T>(string filePath, JsonSerializerOptions? options = null)
        {
            return TryLoad<T>(filePath, out T? value, options) ? value : default;
        }

        public static void Save<T>(string filePath, T value, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Il percorso del file JSON non e' valido.", nameof(filePath));
            }

            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonSerializerOptions serializerOptions = options ?? DefaultOptions;
            string json = JsonSerializer.Serialize(value, serializerOptions);
            string temporaryPath = filePath + ".tmp";

            File.WriteAllText(temporaryPath, json);

            try
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Replace(temporaryPath, filePath, null);
                    }
                    catch
                    {
                        File.Move(temporaryPath, filePath, true);
                    }
                }
                else
                {
                    File.Move(temporaryPath, filePath);
                }
            }
            catch
            {
                if (File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                    }
                }

                throw;
            }
        }
    }
}

