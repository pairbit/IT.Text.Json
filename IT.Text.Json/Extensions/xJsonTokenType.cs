using System.Text.Json;

namespace IT.Text.Json.Extensions;

public static class xJsonTokenType
{
    public static int GetOffset(this JsonTokenType tokenType)
    {
        if (tokenType == JsonTokenType.String || tokenType == JsonTokenType.PropertyName)
            return 1;

        if (tokenType == JsonTokenType.Comment)
            return 2;

        return 0;
    }
}