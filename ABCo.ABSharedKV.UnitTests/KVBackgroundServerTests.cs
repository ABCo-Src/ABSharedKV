using ABCo.ABSave.Configuration;
using ABCo.ABSave.Mapping;
using ABCo.ABSave.Serialization;
using ABCo.ABSharedKV.Background;
using ABCo.ABSharedKV.Background.Database;
using ABCo.ABSharedKV.Background.Enums;
using ABCo.ABSharedKV.Background.Interfaces;
using ABCo.ABSharedKV.Background.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABCo.ABSharedKV.UnitTests
{
    // A memory stream with an extra integer to act a seperator.
    public class SeperatedMemoryStream : Stream
    {
        public MemoryStream Response { get; } = new MemoryStream();
        public MemoryStream Request { get; } = new MemoryStream();
        public bool Flushed { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() => Flushed = true;

        public override int Read(byte[] buffer, int offset, int count) => Request.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => Response.Write(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
    }

    [TestClass]
    public class KVBackgroundServerTests
    {
        [TestMethod]
        public void Connects_DisconnectNormally()
        {
            // Arrange
            var communicatorMock = CreateMockWithGeneral(1);
            FinishSetupMessage(CreateStreamForId(0, communicatorMock));

            var server = new KVServerCommunicator(Mock.Of<KVServerSegmentDomain>(), communicatorMock.Object);

            // Act
            server.PerformCommunications();

            // Assert
            VerifyMockMethodsForId(0, communicatorMock);
        }

        [TestMethod]
        public void Connects_MultipleConnections_DisconnectsNormally()
        {
            // Arrange
            var communicatorMock = CreateMockWithGeneral(2);
            FinishSetupMessage(CreateStreamForId(0, communicatorMock));
            FinishSetupMessage(CreateStreamForId(1, communicatorMock));

            var server = new KVServerCommunicator(Mock.Of<KVServerSegmentDomain>(), communicatorMock.Object);

            // Act
            server.PerformCommunications();

            // Assert
            VerifyMockMethodsForId(0, communicatorMock);
            VerifyMockMethodsForId(1, communicatorMock);
        }

        [TestMethod]
        public void Connects_MultipleConnections_OneDisconnectsUnexpectedly()
        {
            // Arrange
            var communicatorMock = CreateMockWithGeneral(2);
            FinishSetupMessage(CreateStreamForId(0, communicatorMock));
            CreateStreamForId(1, communicatorMock);

            var server = new KVServerCommunicator(Mock.Of<KVServerSegmentDomain>(), communicatorMock.Object);

            // Act
            server.PerformCommunications();

            // Assert
            VerifyMockMethodsForId(0, communicatorMock);
            VerifyMockMethodsForId(1, communicatorMock);
        }

        [TestMethod]
        [DataRow(CommunicationCode.LoadSegment)]
        [DataRow(CommunicationCode.DeleteSegmentById)]
        [DataRow(CommunicationCode.DeleteSegmentByName)]
        [DataRow(CommunicationCode.Add)]
        //[DataRow(CommunicationCode.Edit)]
        [DataRow(unchecked((CommunicationCode)255))] // Invalid
        //[DataRow(CommunicationCode.DeleteSegment)]
        public void SingleConnection_SingleAction_Performed(CommunicationCode code)
        {
            var communicatorMock = CreateMockWithGeneral(1);
            var serverSegmentDomain = new Mock<IKVServerSegmentDomain>();

            var server = new KVServerCommunicator(serverSegmentDomain.Object, communicatorMock.Object);

            var stream = CreateStreamForId(0, communicatorMock);
            SetupMessageDetails(code, stream, communicatorMock, serverSegmentDomain);
            FinishSetupMessage(stream);

            server.PerformCommunications();

            VerifyMessage(code, stream, serverSegmentDomain);
            VerifyMockMethodsForId(0, communicatorMock);
        }

        [TestMethod]
        [DataRow(CommunicationCode.LoadSegment, CommunicationCode.DeleteSegmentById)]
        public void SingleConnection_MultipleActions_AllPerformed(CommunicationCode first, CommunicationCode second)
        {
            var communicatorMock = CreateMockWithGeneral(1);
            var serverSegmentDomain = new Mock<IKVServerSegmentDomain>();

            var server = new KVServerCommunicator(serverSegmentDomain.Object, communicatorMock.Object);
            var stream = CreateStreamForId(0, communicatorMock);

            SetupMessageDetails(first, stream, communicatorMock, serverSegmentDomain);
            SetupMessageDetails(second, stream, communicatorMock, serverSegmentDomain);
            FinishSetupMessage(stream);

            server.PerformCommunications();

            VerifyMessage(first, stream, serverSegmentDomain);
            VerifyMessage(second, stream, serverSegmentDomain);

            VerifyMockMethodsForId(0, communicatorMock);
        }

        [TestMethod]
        public void SingleConnection_InvalidThenValid_ValidNotPerformed()
        {
            var communicatorMock = CreateMockWithGeneral(1);
            var serverSegmentDomain = new Mock<IKVServerSegmentDomain>();

            var server = new KVServerCommunicator(serverSegmentDomain.Object, communicatorMock.Object);
            var stream = CreateStreamForId(0, communicatorMock);

            SetupMessageDetails(unchecked((CommunicationCode)255), stream, communicatorMock, serverSegmentDomain);
            SetupMessageDetails(CommunicationCode.LoadSegment, stream, communicatorMock, serverSegmentDomain);
            FinishSetupMessage(stream);

            server.PerformCommunications();

            VerifyMessage(unchecked((CommunicationCode)255), stream, serverSegmentDomain);

            // Verify the connection was definitely terminated and the "CreateSegment" didn't run.
            serverSegmentDomain.Verify(s => s.LoadSegment(It.IsAny<string>()), Times.Never);

            VerifyMockMethodsForId(0, communicatorMock);
        }

        static void FinishSetupMessage(SeperatedMemoryStream stream)
        {
            stream.Request.WriteByte((byte)CommunicationCode.Disconnect);
            stream.Request.Position = 0;
        }

        // A new ABSave feature will remove the need for this. https://github.com/ABCo-Src/ABSave/issues/66
        static ABSaveMap _dummyMap = ABSaveMap.Get<bool>(ABSaveSettings.ForSpeed);

        static void SetupMessageDetails(CommunicationCode code, SeperatedMemoryStream stream, Mock<IKVCommunicationMechanism> communicatorMock, Mock<IKVServerSegmentDomain> serverSegmentDomain)
        {
            stream.Request.WriteByte((byte)code);

            using var serializer = _dummyMap.GetSerializer(stream.Request);

            switch (code)
            {
                case CommunicationCode.LoadSegment:
                    serializer.WriteString("abc");
                    serverSegmentDomain.Setup(s => s.LoadSegment("abc")).Returns(25);

                    break;
                case CommunicationCode.DeleteSegmentById:
                    serializer.WriteInt16(5);
                    serverSegmentDomain.Setup(s => s.DeleteSegment(5));

                    break;
                case CommunicationCode.DeleteSegmentByName:
                    serializer.WriteString("def");
                    serverSegmentDomain.Setup(s => s.DeleteSegment("def"));

                    break;
                case CommunicationCode.Add:
                    serializer.WriteInt16(999);
                    serializer.WriteString("key");
                    serializer.SerializeItem(new byte[] { 3 }, serializer.GetRuntimeMapItem(typeof(byte[])));
                    serverSegmentDomain.Setup(s => s.Add(999, "key", new byte[] { 3 }));

                    break;
                case unchecked((CommunicationCode)255):
                    break;
                //case CommunicationCode.DeleteSegmentByName:
                //    serializer.WriteString(5);
                //    serverSegmentDomain.Setup(s => s.DeleteSegment(5));

                //    break;
                default:
                    throw new Exception("Not testing!");
            }
        }

        static void VerifyMessage(CommunicationCode code, SeperatedMemoryStream seperatedStream, Mock<IKVServerSegmentDomain> serverSegmentDomain)
        {
            var stream = seperatedStream.Response;
            stream.Position = 0;

            using var deserializer = _dummyMap.GetDeserializer(stream);

            switch (code)
            {
                case CommunicationCode.LoadSegment:
                    serverSegmentDomain.Verify(s => s.LoadSegment("abc"));

                    // Response should be: Segment code
                    Assert.AreEqual(25, stream.ReadByte());
                    break;
                case CommunicationCode.DeleteSegmentById:
                    serverSegmentDomain.Verify(s => s.DeleteSegment(5));
                    break;
                case CommunicationCode.DeleteSegmentByName:
                    serverSegmentDomain.Verify(s => s.DeleteSegment("def"));
                    break;
                case CommunicationCode.Add:
                    serverSegmentDomain.Verify(s => s.Add(999, "key", new byte[] { 3 }));
                    break;
                case unchecked((CommunicationCode)255):
                    Assert.AreEqual(0xFFFFFFFF, (uint)deserializer.ReadInt32());
                    break;
                default:
                    throw new Exception("Not testing!");
            }
        }

        static Mock<IKVCommunicationMechanism> CreateMockWithGeneral(int maxConnectionNo)
        {
            var res = new Mock<IKVCommunicationMechanism>();

            int currentId = 0;
            res.Setup(l => l.WaitForNewCommunication(It.IsAny<CancellationToken>())).Returns((CancellationToken t) =>
            {
                if (currentId == maxConnectionNo)
                {
                    while (true)
                    {
                        Thread.Sleep(1);
                        if (t.IsCancellationRequested) return null;
                    }
                }

                return currentId++;
            });

            return res;
        }

        static SeperatedMemoryStream CreateStreamForId(int id, Mock<IKVCommunicationMechanism> communicatorMock)
        {
            var stream = new SeperatedMemoryStream();

            communicatorMock.Setup(l => l.WaitForMessage(id)).Returns(async () =>
            {
                // Wait some time before we send the message, to allow other connections to be made.
                await Task.Delay(100);

                int msg = stream.ReadByte();

                if (msg == -1)
                    return new MessageResponse(null, 0);
                else
                    return new MessageResponse(stream, (byte)msg);
            });

            communicatorMock.Setup(l => l.CloseConnection(id));

            return stream;
        }

        static void VerifyMockMethodsForId(int id, Mock<IKVCommunicationMechanism> communicatorMock)
        {
            communicatorMock.Verify(l => l.WaitForMessage(id));
            communicatorMock.Verify(l => l.CloseConnection(id));
        }
    }
}
