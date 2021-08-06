using ABCo.ABSharedKV.Background.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background.Interfaces
{
    public interface IKVCommunicationMechanism
    {
        object WaitForNewCommunication(CancellationToken src);
        Task<MessageResponse> WaitForMessage(object obj);
        void CloseConnection(object obj);
    }
}
