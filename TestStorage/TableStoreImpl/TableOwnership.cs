using System;
using System.Threading;

namespace TestStorage.TableStoreImpl
{
    internal class TableOwnership : IDisposable
    {
        private readonly Table _table;

        /// <summary>
        /// Allow our lock to outlive us. This indicates the beginning of a long running process that will carry out several operations
        /// that will all take locks. The final operation will release our lock at the end of the process in addition to the lock it takes
        /// for itself, so if this is set to true, we will not release the lock on the table.
        /// </summary>
        private bool _holdLockOnDispose;

        /// <summary>
        /// Release a long lasting lock when this ownership is disposed. Previously, an instance of this class will have been used to initiate
        /// a long lasting lock (see <see cref="Pin"/>). When this member is set to true, the long lasting operation is complete and the extra
        /// lock needs to be released when we are disposed.
        /// </summary>
        private bool _releaseOnDispose;

        public TableOwnership(Table table)
        {
            _table = table;
            _table.LockToThread(Thread.CurrentThread);
        }

        public void Dispose()
        {
            if (_releaseOnDispose)
                _table.ReleaseLock();

            if (!_holdLockOnDispose)
                _table.ReleaseLock();

        }

        /// <summary>
        /// Keep the lock for an extended period. Calling this function will reverse the effect of any calls to <see cref="Release"/> on this instance.
        /// </summary>
        public void Pin()
        {
            _holdLockOnDispose = true;
            _releaseOnDispose = false;
        }

        /// <summary>
        /// Release an extended lock. Calling this function will reverse the effect of any calls to <see cref="Pin"/> on this instance.
        /// </summary>
        public void Release()
        {
            _releaseOnDispose = true;
            _holdLockOnDispose = false;
        }
    }
}