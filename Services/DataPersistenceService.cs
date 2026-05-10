using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Dictionary.Models;

namespace Dictionary.Services
{
    public class DataPersistenceService
    {
        private readonly string _dataDirectory;
        private readonly string _dataFilePath;

        public DataPersistenceService()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dictionary");
            _dataFilePath = Path.Combine(_dataDirectory, "dictionary_data.json");

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public void SaveData(IEnumerable<DictionaryEntry> entries)
        {
            try
            {
                var entriesList = new List<DictionaryEntry>(entries);
                var options = new JsonSerializerOptions { WriteIndented = false };
                string json = JsonSerializer.Serialize(entriesList, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save data: {ex.Message}", ex);
            }
        }

        public List<DictionaryEntry> LoadData()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new List<DictionaryEntry>();
                }

                string json = File.ReadAllText(_dataFilePath);
                var options = new JsonSerializerOptions();
                var entries = JsonSerializer.Deserialize<List<DictionaryEntry>>(json, options);
                return entries ?? new List<DictionaryEntry>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load data: {ex.Message}", ex);
            }
        }

        public bool HasSavedData()
        {
            return File.Exists(_dataFilePath);
        }

        public void ClearSavedData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    File.Delete(_dataFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to clear saved data: {ex.Message}", ex);
            }
        }
    }
}
