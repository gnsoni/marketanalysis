// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using System.Text.Json;

namespace MarketAnalysis
{
    public static class Utils
    {
        public static T DeepClone<T>(this T obj)
        {
            var serialized = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(serialized)!;
        }
    }
}
