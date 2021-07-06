using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV
{
    public class KVDatabase
    {
        public object Object;
        public object Value;

        public T? GetValue<T>(string key) where T : struct
        {
            return (T?)Object;
        }

        public T GetObject<T>(string key) where T : class
        {
            return (T)Value;
        }

        public void Save<T>(string key, T val)
        {
            if (typeof(T).IsValueType)
                Value = val;
            else
                Object = val;
        }
    }
}
