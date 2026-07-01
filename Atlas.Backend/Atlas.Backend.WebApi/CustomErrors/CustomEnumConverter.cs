using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atlas.Backend.WebApi.Converters;

public class CustomEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();

        // Tenta di fare il parsing dell'enum ignorando il case
        if (Enum.TryParse<T>(stringValue, true, out var result))
        {
            return result;
        }

        // Se fallisce, crea un messaggio custom leggibile.
        // Prende i valori possibili per mostrarli all'utente
        var validValues = string.Join(", ", Enum.GetNames<T>());

        throw new JsonException($"Valore non valido per {typeToConvert.Name}. Valori ammessi: {validValues}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class CustomEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(CustomEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
