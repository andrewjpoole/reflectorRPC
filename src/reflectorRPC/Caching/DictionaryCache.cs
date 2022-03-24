namespace reflectorRPC.Caching
{
    public class DictionaryCache<TKey, T> : IDictionaryCache<TKey, T>
    {
        private readonly Dictionary<TKey, T> _dictionary = new();

        public T Fetch(TKey key, Func<T> insertNew)
        {
            if (_dictionary.ContainsKey(key))
                return _dictionary[key];

            var newItem = insertNew();
            _dictionary.Add(key, newItem);

            return newItem;
        }
    }
}
