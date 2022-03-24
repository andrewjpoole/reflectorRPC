namespace reflectorRPC.Caching;

public interface IDictionaryCache<TKey, T> where TKey : notnull
{
    T Fetch(TKey key, Func<T> insertNew);
}