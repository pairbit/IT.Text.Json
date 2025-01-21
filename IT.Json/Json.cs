using IT.Json.Internal;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IT.Json;

public static class Json
{
    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        TValue? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(utf8Json, options);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(
            Stream utf8Json,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

        TValue? value;

        var rentedList = new RentedList();

        var jso = options != null
            ? new JsonSerializerOptions(options)
            : new JsonSerializerOptions();
        jso.Converters.Add(rentedList);

        try
        {
            value = await JsonSerializer.DeserializeAsync<TValue>(utf8Json, jso, cancellationToken);
        }
        catch
        {
            rentedList.ReturnAndClear();
            throw;
        }

        rentedList.Clear();

        return value;
    }
}