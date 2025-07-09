namespace Marketplace.Core.Abstractions.Services;

public interface IStringParser
{
    string? ExtractParameter(string? input, int parameterIndex, char delimiter = '/');
}
