using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Utilities;
using NUnit.Framework;

namespace StorageTests.TestStorage.Utilities
{
    [TestFixture]
    public class TestFeedingStream
    {
        #region Types for test
        class Reader
        {
            private readonly Stream _stream;
            private BlockingCollection<Tuple<byte[], int, int>> _buffers = new BlockingCollection<Tuple<byte[], int, int>>();
            private Task _readTask;

            public List<int> ReadLengths { get; set; }
            public List<long> ReadPositions { get; set; }

            public Reader(Stream stream)
            {
                _stream = stream;
                _readTask = Task.Factory.StartNew(Read);
                ReadLengths = new List<int>();
                ReadPositions = new List<long>();
            }

            private void Read()
            {
                foreach (var buffer in _buffers.GetConsumingEnumerable())
                {
                    var bytes = _stream.Read(buffer.Item1, buffer.Item2, buffer.Item3);
                    ReadLengths.Add(bytes);
                    ReadPositions.Add(_stream.Position);
                }
            }

            public void QueueRead(byte[] buffer, int offset, int count)
            {
                _buffers.Add(Tuple.Create(buffer, offset, count));
            }

            public void Done()
            {
                _buffers.CompleteAdding();
            }

            public void Wait()
            {
                _readTask.Wait();
            }
        }
        #endregion

        private Random _random;
        private FeedingStream _stream;
        private Reader _reader;

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
            _stream = new FeedingStream(1000, 0, 2000);
            _reader = new Reader(_stream);
        }

        [Test]
        public void DataIsReadFromStream()
        {
            //Arrange
            var data = Buffer(1000);
            _stream.Feed(data);
            _stream.Feed(data);
            _stream.CompleteAdding();

            //Act
            var output = new byte[1000];
            _reader.QueueRead(output, 0, output.Length);
            _reader.Done();
            _reader.Wait();

            //Assert
            Assert.That(output.SequenceEqual(data), Is.True);
        }

        [Test]
        public void LengthOfDataIsReturnedByRead()
        {
            //Arrange
            var data = Buffer(1000);
            _stream.Feed(data);
            _stream.Feed(data);
            _stream.CompleteAdding();

            //Act
            var output = new byte[1000];
            _reader.QueueRead(output, 0, output.Length);
            _reader.Done();
            _reader.Wait();

            //Assert
            Assert.That(_reader.ReadLengths, Is.EqualTo(new [] { 1000 }));
        }

        [Test]
        public void FeedBlocksWhenTooMuchDataIsQueued()
        {
            //Arrange
            var thirdFeedComplete = false;
            var atThirdFeed = false;

            Task.Factory.StartNew(() =>
            {
                var data = Buffer(1500);
                _stream.Feed(data);
                _stream.Feed(data);
                atThirdFeed = true;
                _stream.Feed(data);
                thirdFeedComplete = true;
            });

            //wait for the first two feeds
            while (!atThirdFeed) Thread.Sleep(20);
            Thread.Sleep(100);

            //Act
            var feedBlocked = !thirdFeedComplete; //third feed should be blocked, so this should evaluate to true.

            _stream.Read(new byte[1000], 0, 1000); //this should release the third feed

            //Give the third feed time to complete
            var sw = new Stopwatch();
            sw.Start();
            while (!thirdFeedComplete && sw.ElapsedMilliseconds < 500) Thread.Sleep(20);

            //Assert
            Assert.That(feedBlocked && thirdFeedComplete, Is.True); //the feed should have been blocked, and then released
        }

        [Test]
        public void FeedingStreamDoesNotSupportSeeking()
        {
            //Assert
            Assert.That(_stream.CanSeek, Is.False);
        }

        [Test]
        public void SeekThrows()
        {
            //Assert
            Assert.Throws<NotSupportedException>(() =>_stream.Seek(0, SeekOrigin.Begin));
        }

        [Test]
        public void FeedingStreamDoesNotSupportWriting()
        {
            //Assert
            Assert.That(_stream.CanWrite, Is.False);
        }

        [Test]
        public void WriteThrows()
        {
            //Assert
            Assert.Throws<NotSupportedException>(() => _stream.Write(Buffer(1000), 0, 1000));
        }

        [Test]
        public void LengthThrows()
        {
            //Assert
            Assert.Throws<NotSupportedException>(() => Console.WriteLine(_stream.Length));
        }

        [Test]
        public void FeedingStreamSupportsReading()
        {
            //Assert
            Assert.That(_stream.CanRead, Is.True);
        }

        [Test]
        public void ReadPositionIsTracked()
        {
            //Arrange
            var data = Buffer(1000);
            _stream.Feed(data);
            _stream.Feed(data);
            _stream.Feed(data);
            _stream.CompleteAdding();

            //Act
            var output = new byte[1000];
            _reader.QueueRead(output, 0, output.Length);
            _reader.QueueRead(output, 0, output.Length);
            _reader.Done();
            _reader.Wait();

            //Assert
            Assert.That(_reader.ReadPositions, Is.EqualTo(new long[] {1000, 2000}));
        }
    }
}
