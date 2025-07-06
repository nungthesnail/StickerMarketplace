namespace Marketplace.Core.Abstractions;

public interface IEntity<TKey>
{
    TKey Id { get; init; }
}
