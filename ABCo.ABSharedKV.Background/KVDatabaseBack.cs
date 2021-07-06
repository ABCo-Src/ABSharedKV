using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background
{
    /// <summary>
    /// Represents the background process part of a database.
    /// </summary>
    public class KVDatabaseBack
    {
        Dictionary<string, byte[]> _keys = new();

        public byte[] Get(string key)
        {
            if (_keys.TryGetValue(key, out var val))
                return val;

            return null;
        }

        public void Set(string key, byte[] arr)
        {
            _keys[key] = arr;
        }
    }
}
