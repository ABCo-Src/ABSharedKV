using ABCo.ABSharedKV.Background.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background.Database
{
    /// <summary>
    /// A group of currently loaded segments.
    /// </summary>
    public class KVServerSegmentDomain : IKVServerSegmentDomain
    {
        public void Add(ushort segmentCode, string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public ushort CreateSegment(string name)
        {
            throw new NotImplementedException();
        }

        public void Edit(ushort segmentCode, string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public ushort LoadSegment(string name)
        {
            throw new NotImplementedException();
        }

        public void Remove(ushort segmentCode, string key)
        {
            throw new NotImplementedException();
        }

        public void DeleteSegment(ushort sh)
        {
            throw new NotImplementedException();
        }

        public void Rename(ushort segmentCode, string oldKey, string newKey)
        {
            throw new NotImplementedException();
        }

        public void DeleteSegment(string name)
        {
            throw new NotImplementedException();
        }
    }
}
