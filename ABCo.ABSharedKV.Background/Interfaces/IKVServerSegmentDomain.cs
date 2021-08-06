using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background.Interfaces
{
    public interface IKVServerSegmentDomain
    {
        ushort LoadSegment(string name);
        ushort CreateSegment(string name);
        void DeleteSegment(ushort id);
        void DeleteSegment(string name);

        void Add(ushort segmentCode, string key, byte[] value);
        void Edit(ushort segmentCode, string key, byte[] value);
        void Rename(ushort segmentCode, string oldKey, string newKey);
        void Remove(ushort segmentCode, string key);
    }
}
