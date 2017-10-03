using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AzureStorage.Utilities
{
    public class FeedingStream : Stream
    {
        private readonly int _maxBufferedBytes;
        private FeedingQueue _queue;
        private long _readPosition;

        public FeedingStream(int startOffset, int lengthToStream, int maxBufferedBytes = 40000)
        {
            _maxBufferedBytes = maxBufferedBytes;
            _queue = new FeedingQueue(startOffset, lengthToStream);
        }

        /// <summary>
        /// Add data to be returned by the feed.
        /// </summary>
        /// <param name="buffer"></param>
        public void Feed(byte[] buffer)
        {
            while (_queue.BufferedBytes > _maxBufferedBytes) Thread.Sleep(50);
            _queue.Add(buffer);
        }

        /// <summary>
        /// Indicate that all of the data has been added.
        /// </summary>
        public void CompleteAdding()
        {
            _queue.CompleteAdding();
        }

        public override void Flush()
        {
            //this is a no-op for FeedingStream
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            //not valid for FeedingStream
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var readLength = _queue.Fill(buffer, offset, count);
                Interlocked.Add(ref _readPosition, readLength);
                return readLength;
            }
            catch
            {
                return 0;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return _readPosition; }
            set { throw new NotSupportedException(); }
        }
    }
}
