using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace AzureStorage.Utilities
{
    /// <summary>
    /// Queue buffers containing data for consumption.
    /// </summary>
    public class FeedingQueue
    {
        private class DataBlock
        {
            /// <summary>
            /// The earliest offset within the data contained within this block, from a reader's viewpoint.<para/>
            /// The data block (<see cref="Data"/>) is the original array queued by the writer, and the byte referenced
            /// by the start offset need not be the first byte in the array. The offset to the first byte is held in the 
            /// <see cref="LeadInBytes"/> property.
            /// </summary>
            public int StartOffset { get; set; }

            /// <summary>
            /// The last data byte held within the data block. This will not always be the last byte of the <see cref="Data"/>
            /// array. In the event that the data to be returned by queue as a whole ends before the actual input data, this 
            /// may indicate a byte short of the end of the data array. 
            /// </summary>
            public int EndOffset { get; set; }

            /// <summary>
            /// The number of bytes at the start of the data array that should not be released to the reader.
            /// </summary>
            public int LeadInBytes { get; set; }

            /// <summary>
            /// The data array. The usable section of this array is defined by the <see cref="LeadInBytes"/> and <see cref="EndOffset"/> less <see cref="StartOffset"/>.
            /// </summary>
            public byte[] Data { get; set; }

            public DataBlock(int startOffset, int usableCount, int leadInBytes, byte[] data)
            {
                StartOffset = startOffset;
                Data = data;
                EndOffset = StartOffset + usableCount;
                LeadInBytes = leadInBytes;
            }
        }

        /// <summary>
        /// The current set of buffered data blocks.
        /// </summary>
        private BlockingCollection<DataBlock> _dataBlocks = new BlockingCollection<DataBlock>();

        /// <summary>
        /// The current data buffer containing the next block of data to be returned. This can be null at any time if the end of the current block
        /// coincides with the end of a read operation.
        /// </summary>
        private DataBlock _current;

        /// <summary>
        /// The offset into the data that the queue should return. Data prior to this offset will be received by the queue but should be ignored.
        /// </summary>
        private readonly int _startOffset;

        /// <summary>
        /// The length of the data that should be returned by the queue. This is simply the number of bytes *AFTER* the start offset that 
        /// constitutes the data. Data recieved by the queue outside of the range defined by the start offset and the length will be ignored,
        /// and will not be apparent to the reader.
        /// </summary>
        private readonly int _length;

        /// <summary>
        /// The offset of the incoming data feed. The first data supplied to the queue is at offset zero. The queue tracks the offset as blocks are
        /// received and this member indicates the offset of the next byte received. This is only of use when data is received by the queue and should
        /// not be relevant to the process reading data out.
        /// </summary>
        private int _dataOffset;

        /// <summary>
        /// The offset of the next byte to be read from the point of view of the reader. i.e. if the queue is created with a start offset of 100, 
        /// the first byte of data returned in a fill call will be the 100th byte of data supplied to the queue, but would have a request offset of zero.
        /// </summary>
        private int _requestOffset;

        private int _bufferedBytes;

        public FeedingQueue(int startOffset, int length = 0)
        {
            _startOffset = startOffset;
            _length = length;
        }

        public int BufferedBytes
        {
            get { return _bufferedBytes; }
            set { _bufferedBytes = value; }
        }

        public void Add(byte[] data)
        {
            if (_dataBlocks.IsAddingCompleted)
                return;

            var maxOffset = _length == 0 ? int.MaxValue : _startOffset + _length;
            if (_dataOffset + data.Length > _startOffset && _dataOffset < maxOffset)
            {
                Interlocked.Add(ref _bufferedBytes, data.Length);
                var leadInBytes = _startOffset > _dataOffset ? _startOffset - _dataOffset : 0;
                var usableDataOffset = _startOffset > _dataOffset ? 0 : _dataOffset - _startOffset;
                var maxUsableCount = data.Length - leadInBytes;
                var usableCount = Math.Min(maxUsableCount, (_dataOffset + data.Length) < maxOffset ? data.Length - leadInBytes : maxOffset - _dataOffset);
                
                var dataCopy = new byte[data.Length];
                Array.Copy(data, dataCopy, dataCopy.Length);

                var block = new DataBlock(usableDataOffset, usableCount, leadInBytes, dataCopy);
                _dataBlocks.Add(block);

                if (usableDataOffset + usableCount == _length)
                    _dataBlocks.CompleteAdding();
            }
            _dataOffset += data.Length;
        }

        public void CompleteAdding()
        {
            _dataBlocks.CompleteAdding();
        }

        [DebuggerHidden]
        public int Fill(byte[] buffer, int offset = 0, int count = -1)
        {
            var copyLength = count < 0 ? buffer.Length - offset : count;
            var bufferRemaining = copyLength;
            
            while (bufferRemaining > 0)
            {
                try
                {
                    if (_current == null && !_dataBlocks.IsCompleted)
                        _current = _dataBlocks.Take();
                }
                catch (InvalidOperationException)
                {
                    //This is thrown when the enumerator is waiting for a block and the collection is completed in the feeding thread.
                }

                if (_current == null)
                    return copyLength - bufferRemaining;

                var thisBlockTakeLength = Math.Min(_current.EndOffset - _requestOffset, bufferRemaining);
                var copyStart = _current.LeadInBytes + (_requestOffset - _current.StartOffset);
                Array.Copy(_current.Data, copyStart, buffer, copyLength - bufferRemaining + offset, thisBlockTakeLength);

                if (_requestOffset + thisBlockTakeLength >= _current.EndOffset)
                {
                    Interlocked.Add(ref _bufferedBytes, 0 - _current.Data.Length);
                    _current = null;
                }

                _requestOffset += thisBlockTakeLength;
                bufferRemaining -= thisBlockTakeLength;
            }

            return copyLength;
        }
    }
}