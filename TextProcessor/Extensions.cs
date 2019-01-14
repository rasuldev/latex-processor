using System.Collections.Generic;

namespace TextProcessor
{
    public static class Extensions
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) =>
            dict?.ContainsKey(key) == true ? dict[key] : default(TValue);
    }
}