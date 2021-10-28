using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace MTF.Utilities
{
    public interface AESelem : IDisposable
    {
        // init can be call many times but do work only first time
        // the idea is call async part of initialization
        // outside of constructors and lock brackets
        public Task init(object prms);
        // just a way to obtain desered object
        public Task<object> get();
        // just a way to release desered object
        public void release(object dbc);
    }
    public class ActualEntriesStore<T> where T : AESelem, new()
    {
        public delegate Task OnActualize(string uId);

        protected class AERecord
        {
            public int numActualizations = 1;
            public bool markToDelete = false;
            public DateTime markedToDelete = new DateTime();
            public T dscr;

            public AERecord(string uId)
            {
                // a way to call constructor with parameters
                // for generic type
                dscr = (T)Activator.CreateInstance(typeof(T), uId);
            }
        }

        private object _locker = new object();
        // Store for user events
        private Dictionary<string, AERecord> ActualEntries 
            = new Dictionary<string, AERecord>();
        private OnActualize _onActCallback;

        // add compromised cache here as dictionary
        private Dictionary<string, DateTime> RecordsToDelete
            = new Dictionary<string, DateTime>();

        public ActualEntriesStore()
        {
            _onActCallback = null;
        }
        public void RegisterActualizationCallback(OnActualize callback)
        {
            _onActCallback = callback;
        }

        public async Task EntryActualized(string eId)
        {
            bool useCallback = false;
            lock (_locker)
            {
                if (ActualEntries.TryGetValue(eId, out AERecord crn))
                {
                    crn.markToDelete = false;
                    if (crn.numActualizations <= 0)
                    {
                        crn.numActualizations = 1;
                    }
                    else
                    {
                        crn.numActualizations += 1;
                    }
                    RecordsToDelete.Remove(eId);
                }
                else
                {
                    ActualEntries.Add(eId, new AERecord(eId));
                    useCallback = true;
                }
            }

            if (useCallback)
            {
                await (_onActCallback == null ? Task.CompletedTask : _onActCallback(eId));
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task EntryComprimized(string eId)
        {
            lock (_locker)
            {
                if (ActualEntries.TryGetValue(eId, out AERecord crn))
                {
                    if (--crn.numActualizations <= 0)
                    {
                        if (!crn.markToDelete)
                        {
                            crn.markToDelete = true;
                            crn.markedToDelete = DateTime.Now;
                            // add functionality for todelete cache here
                            RecordsToDelete.Add(eId, crn.markedToDelete);
                        }
                    }
                }
                else 
                {
                    throw new Exception("EntryComprimized Unknown eId.");
                }
            }

            await Task.CompletedTask;
        }

        public async Task EntryComprimizedHard(string eId)
        {
            lock (_locker)
            {
                if (ActualEntries.TryGetValue(eId, out AERecord crn))
                {
                    crn.numActualizations = 0;
                    crn.markToDelete = true;
                    crn.markedToDelete = DateTime.Now.AddYears(-1);
                    // add functionality for todelete cache here
                    RecordsToDelete[eId] = crn.markedToDelete;
                }
                else
                {
                    throw new Exception("EntryComprimized Unknown eId.");
                }
            }

            await Task.CompletedTask;
        }

        public async Task<List<string>> DeleteCompromizedEntries(TimeSpan tOut)
        {
            List<T> toRemoveObjs = new List<T>();
            List<string> toRemove = new List<string>();

            lock (_locker)
            {
                foreach (var el in RecordsToDelete)
                {
                    if (DateTime.Now - el.Value > tOut)
                    {
                        toRemove.Add(el.Key);
                        toRemoveObjs.Add(ActualEntries.GetValueOrDefault(el.Key).dscr);
                    }
                }
                foreach (string k in toRemove)
                {
                    _ = ActualEntries.Remove(k);
                    _ = RecordsToDelete.Remove(k);
                }
            }

            foreach (T v in toRemoveObjs)
            {
                v.Dispose();
            }

            return await Task.FromResult(toRemove);
        }

        protected List<string> DeleteAllEntries()
        {
            List<T> toRemoveObjs = new List<T>();
            List<string> toRemove = new List<string>();

            lock (_locker)
            {
                RecordsToDelete.Clear();

                foreach (var el in ActualEntries)
                {
                    toRemove.Add(el.Key);
                    toRemoveObjs.Add(ActualEntries.GetValueOrDefault(el.Key).dscr);
                }
                ActualEntries.Clear();
            }

            foreach (T v in toRemoveObjs)
            {
                v.Dispose();
            }

            return toRemove;
        }

        public async Task<List<string>> GetActualEntries()
        {
            List<string> rtn;
            lock (_locker)
            {
                rtn = new List<string>(ActualEntries.Keys);
            }

            return await Task.FromResult(rtn);
        }

        // How to obtain secret object, ha-ha
        public async Task<object> getAESElemValue(string eId, object prms)
        {
            T res = default(T);
            lock (_locker)
            {
                if (ActualEntries.TryGetValue(eId, out AERecord crn))
                {
                    res = crn.dscr;
                }
                else
                {
                    throw new Exception("getAESElemValue Unknown eId. Please goto application's home page in your browser by hands.");
                }
            }
            // the idea is call async part of initialization
            // outside of constructors and lock brackets
            await res.init(prms);

            return await res.get();
        }

        // How to release secret object, ha-ha
        public async Task<bool> ReleaseAESElemValue(string eId, object dbc)
        {
            bool rc = false;
            lock (_locker)
            {
                if (ActualEntries.TryGetValue(eId, out AERecord crn))
                {
                    crn.dscr.release(dbc);
                    rc = true;
                }
                else
                {
                    rc = false;
                    //throw new Exception("ReleaseAESElemValue Unknown eId. Please goto application's home page in your browser by hands.");
                }
            }

            return await Task.FromResult(rc);
        }

    }
}
