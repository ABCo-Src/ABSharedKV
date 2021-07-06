using ABCo.ABSharedKV;
using ABCo.ABSharedKV.Background;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABCo.ABSharedKV.UnitTests
{
    [TestClass]
    public class KVDatabaseBackTests
    {
        [TestMethod]
        public void Get_NonExistant()
        {
            var back = new KVDatabaseBack();
            var result = back.Get("thing");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SaveAndGet()
        {
            // Arrange
            var back = new KVDatabaseBack();
            var newData = new byte[3] { 5, 10, 15 };
            back.Set("First", newData);

            // Act
            var result = back.Get("First");

            // Assert
            Assert.AreEqual(newData, result);
        }

        [TestMethod]
        public void SaveAndGet_Multiple()
        {
            // Arrange
            var back = new KVDatabaseBack();

            var newData = new byte[3] { 5, 10, 15 };
            back.Set("First", newData);

            var fakeData = new byte[3] { 15, 20, 25 };
            back.Set("Second", fakeData);

            // Act
            var result = back.Get("First");

            // Assert
            Assert.AreEqual(newData, result);
        }

        [TestMethod]
        public void Save_Existing()
        {
            // Arrange
            var back = new KVDatabaseBack();

            var fakeData = new byte[3] { 15, 20, 25 };
            back.Set("First", fakeData);

            var newData = new byte[3] { 5, 10, 15 };
            back.Set("First", newData);

            // Act
            var result = back.Get("First");

            // Assert
            Assert.AreEqual(newData, result);
        }
    }
}
