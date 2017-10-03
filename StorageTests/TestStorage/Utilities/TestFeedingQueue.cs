using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Utilities;
using NUnit.Framework;

namespace StorageTests.TestStorage.Utilities
{
    [TestFixture]
    public class TestFeedingQueue
    {
        private FeedingQueue _queue;
        private Random _random;

        private byte[] Buffer(int size)
        {
            var buffer = new byte[size];
            _random.NextBytes(buffer);
            return buffer;
        }

        [SetUp]
        public void SetUp()
        {
            _random = new Random();
            _queue = new FeedingQueue(1000, 5000);
        }

        [Test]
        public void QueueIgnoresBlocksOutsideRange()
        {
            //Arrange
            var data = Buffer(1000);

            //Act
            _queue.Add(data);

            //Assert
            Assert.That(_queue.BufferedBytes, Is.EqualTo(0));
        }

        [Test]
        public void QueueIncludesBlocksWithStartOfRange()
        {
            //Arrange
            var data = Buffer(999);
            var data2 = Buffer(1999);

            //Act
            _queue.Add(data);
            _queue.Add(data2);

            //Assert
            Assert.That(_queue.BufferedBytes, Is.EqualTo(1999));
        }

        [Test]
        public void QueueExcludesBlocksBeyondEndOfRequiredData()
        {
            //Arrange
            var data = Buffer(1000);
            var data2 = Buffer(4999);
            var data3 = Buffer(100);
            var data4 = Buffer(400);

            //Act
            _queue.Add(data);
            _queue.Add(data2);
            _queue.Add(data3);
            _queue.Add(data4);

            //Assert
            Assert.That(_queue.BufferedBytes, Is.EqualTo(5099));
        }

        [Test]
        public void QueueFillsRequestedPartOfBuffer()
        {
            //Arrange
            var data = Buffer(7000);
            _queue.Add(data);
            var buffer = new byte[5000];

            //Act
            _queue.Fill(buffer, 1000, 2000);

            //Assert
            var expected = new byte[1000]
                .Concat(data.Skip(1000).Take(2000))
                .Concat(new byte[2000]);
            Assert.That(buffer.SequenceEqual(expected), Is.True);
        }

        [Test]
        public void NumBytesReturnedReflectsCopyLengthNotBufferLength()
        {
            //Arrange
            var data = Buffer(1500);
            _queue.Add(data);
            _queue.CompleteAdding();
            var buffer = new byte[5000];

            //Act
            var bytesReturned = _queue.Fill(buffer, 1000, 2000);

            //Assert
            Assert.That(bytesReturned, Is.EqualTo(500));
        }

        [Test]
        public void QueueFillsBufferOnRequest()
        {
            //Arrange
            var data = Buffer(7000);
            _queue.Add(data);
            var buffer = new byte[5000];

            //Act
            _queue.Fill(buffer);

            //Assert
            Assert.That(buffer.SequenceEqual(data.Skip(1000).Take(buffer.Length)), Is.True);
        }

        [Test]
        public void UsedBlocksAreRemovedFromByteCount()
        {
            //Arrange
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            _queue.Add(Buffer(1000));
            var buffer = new byte[2000];

            //Act
            _queue.Fill(buffer);

            //Assert
            Assert.That(_queue.BufferedBytes, Is.EqualTo(3000));
        }

        [Test]
        public void QueueFillsBufferFromInputBlockSpan()
        {
            //Arrange
            var data = Buffer(2000);
            var data2 = Buffer(5000);
            _queue.Add(data);
            _queue.Add(data2);
            var buffer = new byte[5000];

            //Act
            _queue.Fill(buffer);

            //Assert
            var expected = data.Concat(data2)
                .Skip(1000)
                .Take(buffer.Length);
            Assert.That(buffer.SequenceEqual(expected), Is.True);
        }

        [Test]
        public void ExcessInputDataIsNotReturned()
        {
            //Arrange
            var data = Buffer(3000);
            var data2 = Buffer(5000);
            _queue.Add(data);
            _queue.Add(data2);
            var buffer = new byte[10000];

            //Act
            var bytesReturned = _queue.Fill(buffer);

            //Assert
            Assert.That(bytesReturned, Is.EqualTo(5000));
        }

        [Test]
        public void ShortBufferContainsExpectedData()
        {
            //Arrange
            var data = Buffer(3000);
            var data2 = Buffer(5000);
            _queue.Add(data);
            _queue.Add(data2);
            var buffer = new byte[10000];

            //Act
            var bytesReturned = _queue.Fill(buffer);

            //Assert
            var expected = data.Concat(data2)
                .Skip(1000)
                .Take(bytesReturned);
            Assert.That(buffer.Take(bytesReturned).SequenceEqual(expected), Is.True);
        }

        [Test]
        public void AllDataSuppliedInOneBufferShouldBeAllowed()
        {
            //Arrange
            var data = Buffer(6000);
            _queue.Add(data);
            var buffer = new byte[10000];

            //Act
            var bytesReturned = _queue.Fill(buffer);

            //Assert
            var expected = data.Skip(1000);
            Assert.That(buffer.Take(bytesReturned).SequenceEqual(expected), Is.True);
        }

        [Test]
        public void MiddleBufferShouldBeTotallyConsumed()
        {
            //Arrange
            var data = Buffer(2000);
            var data2 = Buffer(2000);
            var data3 = Buffer(2000);
            _queue.Add(data);
            _queue.Add(data2);
            _queue.Add(data3);

            var buffer = new byte[10000];

            //Act
            var bytesReturned = _queue.Fill(buffer);

            //Assert
            var expected = data.Concat(data2).Concat(data3)
                .Skip(1000)
                .Take(5000);
            Assert.That(buffer.Take(bytesReturned).SequenceEqual(expected), Is.True);
        }

        [Test]
        public void DataCanBeReadInChunks()
        {
            //Arrange
            var data = Buffer(2000);
            var data2 = Buffer(2000);
            var data3 = Buffer(2000);
            _queue.Add(data);
            _queue.Add(data2);
            _queue.Add(data3);

            var buffer = new byte[1000];
            var buffer2 = new byte[10000];

            //Act
            var bytesReturned = _queue.Fill(buffer);
            var bytesReturned2 = _queue.Fill(buffer2);

            //Assert
            var expected = data.Concat(data2).Concat(data3)
                .Skip(1000)
                .Take(5000);
            var actual = buffer.Concat(buffer2);
            Assert.That(actual.Take(bytesReturned + bytesReturned2).SequenceEqual(expected), Is.True);
        }

        [Test]
        public void DataFlowsThroughQueue()
        {
            //Arrange
            var queue = new FeedingQueue(0);
            var output = new byte[65536];
            var task = Task.Factory.StartNew(() => { queue.Fill(output, 0, 32768); queue.Fill(output, 32768, output.Length - 32768); });
            var bigData = new byte[100000];

            //Act
            var buffer = new byte[5000]; 
            for (int i = 0; i < 20; i++)
            {
                var data = Buffer(buffer.Length);
                Array.Copy(data, buffer, data.Length);
                queue.Add(buffer);
                Array.Copy(data, 0, bigData, data.Length * i, data.Length);
            }

            task.Wait();

            //Assert
            var expected = bigData.Take(output.Length);
            Assert.That(output.SequenceEqual(expected), Is.True);
        }
    }
}