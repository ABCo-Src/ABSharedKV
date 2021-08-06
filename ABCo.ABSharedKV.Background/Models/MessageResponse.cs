using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background.Models
{
    public struct MessageResponse
    {
        public Stream? StreamResponse { get; }
        public byte ResponseCode { get; }

        public MessageResponse(Stream? streamResponse, byte responseCode) =>
            (StreamResponse, ResponseCode) = (streamResponse, responseCode);
    }
}
