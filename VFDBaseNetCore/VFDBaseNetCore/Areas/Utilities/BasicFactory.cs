using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTF.Utilities
{
    public interface IProduct
    {
        public string UID { get; }
        public void prehireCleaning();
    }

    public class BasicFactory<T, P> : IDisposable
        where T : IProduct, IDisposable
    {
        private bool _disposed = false;
        private P _defprm = default(P);
        private object _locker = new object();

        Dictionary<string, T> _hiredProducts = new Dictionary<string, T>();
        Queue<T> _avlProducts = new Queue<T>();

        public BasicFactory(P prm)
        {
            _defprm = prm;
        }

        public async Task<T> hire()
        {
            // Check if we have avl items
            T elemToHire = default(T);
            lock (_locker)
            {
                if (!_avlProducts.TryDequeue(out elemToHire))
                {
                    elemToHire = (T)Activator.CreateInstance(typeof(T), _defprm);
                }
                _hiredProducts.Add(elemToHire.UID, elemToHire);
            }

            elemToHire.prehireCleaning();

            return await Task.FromResult(elemToHire);
        }

        public void release(T product)
        {
            lock(_locker)
            {
                if (_hiredProducts.ContainsKey(product.UID))
                {
                    _hiredProducts.Remove(product.UID);
                    _avlProducts.Enqueue(product);
                }
                else
                {
                    throw new Exception("BasicFactory product returned is not hired.");
                }
            }
        }

        // IDisposible realization
        ~BasicFactory() => Dispose(false);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (var el in _avlProducts)
                {
                    el.Dispose();
                }
                foreach (var el in _hiredProducts)
                {
                    el.Value.Dispose();
                }
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
