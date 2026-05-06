using System.IO;
using System.Text.Json;
using MuseumApp.Domain;

namespace MuseumApp.Storage
{
    public class JsonMuseumRepository : IMuseumRepository
    {
        private readonly string _filePath;

        public JsonMuseumRepository(string filePath)
        {
            _filePath = filePath;
        }

        public MuseumData LoadData()
        {
            if (!File.Exists(_filePath))
            {
                return new MuseumData();
            }

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<MuseumData>(json) ?? new MuseumData();
        }

        public void SaveData(MuseumData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_filePath, json);
        }
    }
}