using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sproutopia.Models;

namespace Sproutopia.Utilities
{
    public class CellCoordinateConverter : JsonConverter<CellCoordinate>
    {
        public override void WriteJson(JsonWriter writer, CellCoordinate value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }

        public override CellCoordinate ReadJson(JsonReader reader, Type objectType, CellCoordinate existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            int x = obj["X"].Value<int>();
            int y = obj["Y"].Value<int>();
            var cc = new CellCoordinate(x, y);
            return cc;
        }
    }
}