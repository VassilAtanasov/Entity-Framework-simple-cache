using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace Eq.Data
{
    public static class Cache
    {
        static private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        static string[] _Tables;

        public static string[] CachedTables
        {
            get
            {
                return _Tables;
            }
            set
            {
                _Tables = value;
var i++;
                Refresh();
            }
        }


        public static Eq.Data.H2HAffilieates A
        {
            get
            {
                cacheLock.EnterReadLock();
                try
                {
                    return GetContext();
                }
                finally
                {
                    if (cacheLock.IsReadLockHeld) cacheLock.ExitReadLock();
                }
            }
        }

        public static void Refresh()
        {
            cacheLock.EnterWriteLock();
            try
            {
                H2HAffilieates db = new H2HAffilieates();

                foreach (string sTab in _Tables)
                {
                    try
                    {
                        #region Pluralize/Singularize
                        var tableName = "Eq.Data.";
                        string s = sTab.Trim();
                        if (s.GetLast(3) == "ies")
                        {
                            tableName += s.TrimEnd("ies");
                            tableName += "y";
                        }
                        else if (s.GetLast(2) == "es")
                            tableName += s.TrimEnd("es");

                        else if (s.GetLast(1) == "s" && s.GetLast(2) != "ss")
                            tableName += s.TrimEnd("s");
                        else
                            tableName += s;

                        tableName += ",Eq.Data";
                        #endregion
                        var type = Type.GetType(tableName);
                        var dbset = db.Set(type);
                        if (dbset != null)
                            dbset.Load();
                    }
                    catch
                    {
                        //table was not entity - ignore errors
                    }
                }

                MemoryCache.Default.Set("H2HAffilieates", db, new System.Runtime.Caching.CacheItemPolicy().AbsoluteExpiration = DateTimeOffset.Now.AddHours(48.0));
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        static H2HAffilieates GetContext()
        {
            //Get the default MemoryCache
            ObjectCache cache = MemoryCache.Default;

            //Get object from cache and return it, if its there
            H2HAffilieates val = (H2HAffilieates)cache.Get("H2HAffilieates");
            if (val != null)
                return val;

            if (cacheLock.IsReadLockHeld) cacheLock.ExitReadLock();
            Refresh();
            return (H2HAffilieates)cache.Get("H2HAffilieates");
        }

    }
    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
        public static string TrimEnd(this string source, string value)
        {
            if (!source.EndsWith(value))
                return source;

            return source.Remove(source.LastIndexOf(value));
        }
    }
}
