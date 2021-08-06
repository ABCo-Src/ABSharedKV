using ABCo.ABSave.Configuration;
using ABCo.ABSave.Deserialization;
using ABCo.ABSave.Mapping;
using ABCo.ABSharedKV.Background.Enums;
using ABCo.ABSharedKV.Background.Interfaces;
using ABCo.ABSharedKV.Background.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.Background
{
    /// <summary>
    /// Responsible for managing the overall background server for ABSharedKV.
    /// </summary>
    public class KVServerCommunicator
    {
        IKVCommunicationMechanism _communicator;
        IKVServerSegmentDomain _domain;
        CancellationTokenSource _serverStopper = new();
        int _currentlyActiveThreads;

        public KVServerCommunicator(IKVServerSegmentDomain domain, IKVCommunicationMechanism communicator) => 
            (_domain, _communicator) = (domain, communicator);

        public void PerformCommunications()
        {
            while (true)
            {
                object communication = _communicator.WaitForNewCommunication(_serverStopper.Token);
                if (_serverStopper.IsCancellationRequested) return;

                _currentlyActiveThreads++;
                HandleCommunicationLineAsync(communication);
            }
        }

        // A new ABSave feature will remove the need for this. https://github.com/ABCo-Src/ABSave/issues/66
        static ABSaveMap _dummyMap = ABSaveMap.Get<bool>(ABSaveSettings.ForSpeed);

        async void HandleCommunicationLineAsync(object communication)
        {
            while (true)
            {
                MessageResponse response = await _communicator.WaitForMessage(communication);

                if (response.ResponseCode == 0) goto Stop;

#pragma warning disable IDE0063 // Use simple 'using' statement - It doesn't work with "goto"
                using (var deserializer = _dummyMap.GetDeserializer(response.StreamResponse!))
                using (var serializer = _dummyMap.GetSerializer(response.StreamResponse!))
#pragma warning restore IDE0063 // Use simple 'using' statement
                    switch ((CommunicationCode)response.ResponseCode)
                    {
                        case CommunicationCode.LoadSegment:

                            ushort val = _domain.LoadSegment(deserializer.ReadString());
                            serializer.WriteInt16((short)val);
                            break;

                        case CommunicationCode.DeleteSegmentById:

                            ushort id = (ushort)deserializer.ReadInt16();
                            _domain.DeleteSegment(id);
                            break;

                        case CommunicationCode.DeleteSegmentByName:

                            string name = deserializer.ReadString();
                            _domain.DeleteSegment(name);
                            break;

                        case CommunicationCode.Add:
                            {
                                ushort segmentId = (ushort)deserializer.ReadInt16();
                                string key = deserializer.ReadString();

                                var bytes = (byte[])deserializer.DeserializeExactNonNullItem(deserializer.GetRuntimeMapItem(typeof(byte[])));
                                _domain.Add(segmentId, key, bytes);
                            }
                            break;

                        case CommunicationCode.Edit:
                            {
                                ushort segmentId = (ushort)deserializer.ReadInt16();
                                string key = deserializer.ReadString();

                                var bytes = (byte[])deserializer.DeserializeExactNonNullItem(deserializer.GetRuntimeMapItem(typeof(byte[])));
                                _domain.Edit(segmentId, key, bytes);
                            }
                            break;

                        default:

                            serializer.WriteInt32(unchecked((int)0xFFFFFFFF));
                            goto Stop;
                    }
            }

        Stop:
            _communicator.CloseConnection(communication);
            _currentlyActiveThreads--;

            if (_currentlyActiveThreads == 0)
                _serverStopper.Cancel();
        }
    }
}
