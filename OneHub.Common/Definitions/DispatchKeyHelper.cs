using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public static class DispatchKeyHelper
    {
        public static bool TryGetNestedProperty(JsonDocument document, string path, out string result)
        {
            if (document is null)
            {
                result = null;
                return false;
            }
            return TryGetNestedPropertyInternal(document.RootElement, path, out result);
        }

        private static bool TryGetNestedPropertyInternal(in this JsonElement json, string path, out string result)
        {
            return TryGetNestedPropertyInternal(in json, path.Split('.'), 0, out result);
        }

        private static bool TryGetNestedPropertyInternal(in this JsonElement json, string[] path, int index,
            out string result)
        {
            if (index == path.Length)
            {
                result = json.GetString();
                return true;
            }
            if (json.TryGetProperty(path[index], out var propertyValue))
            {
                return TryGetNestedPropertyInternal(in propertyValue, path, index + 1, out result);
            }
            result = default;
            return false;
        }
    }
}
