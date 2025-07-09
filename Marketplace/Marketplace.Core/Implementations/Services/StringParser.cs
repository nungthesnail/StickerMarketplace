using Marketplace.Core.Abstractions.Services;

namespace Marketplace.Core.Implementations.Services;

public class StringParser : IStringParser
{
    public string? ExtractParameter(string? input, int parameterIndex, char delimiter = '/')
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex, nameof(parameterIndex));
        var parameters = input?.Split(delimiter);
        return parameters is null || parameters.Length >= parameterIndex ? null : parameters[parameterIndex];
    }
}
