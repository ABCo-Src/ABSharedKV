using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background.Enums
{
    public enum CommunicationCode : byte
    {
        Disconnect = 0,

        // Segment operations:
        LoadSegment = 1,
        CreateSegment = 2,
        DeleteSegmentById = 3,
        DeleteSegmentByName = 4,

        // Key-value operations:
        Add = 16,
        Load = 17,
        Edit = 18,
        Rename = 19,
        Remove = 20
    }
}
