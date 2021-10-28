using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace MTF.Utilities
{
    public class DelayedList<T>
    {
        protected class DLElem
        {
            public T Cell;
            public int Cntr;

            public DLElem(T val, int cnt)
            {
                Cell = val;
                Cntr = cnt;
            }
        }

        private object _locker = new object();
        private int CurrentIdentity = 1;
        protected Dictionary<int,DLElem> Lst = new Dictionary<int, DLElem>();

        public async Task DLPut(T v, int cnt)
        {
            lock (_locker)
            {
                cnt = cnt < 0 ? 0 : cnt;
                Lst.Add(CurrentIdentity++, new DLElem(v, cnt));
            }

            await Task.CompletedTask;
        }

        public async Task<List<T>> DLGet()
        {
            List<T> res = new List<T>();
            List<int> toRem = new List<int>();

            lock (_locker)
            {
                foreach (var elem in Lst)
                {
                    if (elem.Value.Cntr-- == 0)
                    {
                        res.Add(elem.Value.Cell);
                        toRem.Add(elem.Key);
                    }
                }
                foreach (int k in toRem)
                {
                    _ = Lst.Remove(k);
                }
            }

            return await Task.FromResult(res);
        }
    }
}
