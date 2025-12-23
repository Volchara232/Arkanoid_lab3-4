using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arkanoid.State
{
    public static class GameStateSerializer
    {
        public static void Save(GameState state, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
            var json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(filePath, json);
        }

        public static GameState Load(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<GameState>(json, options)
                   ?? throw new InvalidDataException();
        }
    }
}