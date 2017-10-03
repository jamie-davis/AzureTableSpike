using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Utilities
{
    public class RowBuffer<T> : IEnumerable<T> where T : class
    {
        private BlockingCollection<T> _rowsIn = new BlockingCollection<T>();
        private ThrowingEnumerator<T> _enumerator;

        class ThrowingEnumerator<TEnumeratedType> : IEnumerator<TEnumeratedType>
        {
            private readonly IEnumerator<TEnumeratedType> _enumerator;

            public ThrowingEnumerator(IEnumerator<TEnumeratedType> enumerator)
            {
                _enumerator = enumerator;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            #endregion

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                if (Exception != null)
                    throw Exception;

                var result = _enumerator.MoveNext();

                if (Exception != null)
                    throw Exception;

                return result;
            }

            public Exception Exception { get; set; }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public TEnumeratedType Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            #endregion
        }

        public RowBuffer(Func<IEnumerable<T>> fetchFunc)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var item in fetchFunc())
                    {
                        _rowsIn.Add(item);
                    }
                }
                catch (Exception e)
                {
                    _enumerator.Exception = e;
                }

                _rowsIn.CompleteAdding();
            });

            _enumerator = new ThrowingEnumerator<T>(_rowsIn.GetConsumingEnumerable().GetEnumerator());
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}
