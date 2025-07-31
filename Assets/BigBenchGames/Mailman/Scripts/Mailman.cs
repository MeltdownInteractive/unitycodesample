using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// The Mailman main class for sending data across the project
    /// </summary>
    public static class Mailman
    {
        /// <summary>
        /// Internal class for storing the callback data
        /// </summary>
        private class CallbackData
        {
            /// <summary>
            /// A hash of the original callback used for deletion lookup
            /// </summary>
            public int Hash;

            /// <summary>
            /// The Callback function stored as a action for faster invoking
            /// </summary>
            public Action<object> Callback;

            /// <summary>
            /// The priority of the callback in the list
            /// </summary>
            public int Priority;
        }

        /// <summary>
        /// An internal struct that stores data surrounding a mail dispatch type
        /// </summary>
        private class MailDispatchData
        {
            /// <summary>
            /// A pool of mail objects for lowered GC generation
            /// </summary>
            public Stack<Mail> pool;
            /// <summary>
            /// A list of subscribers to the mail dispatch
            /// </summary>
            public List<CallbackData> subscribers;
            /// <summary>
            /// Is the list of callbacks sorted
            /// </summary>
            public bool Sorted;
        }

        /// <summary>
        /// The default initial pool size for mail types
        /// </summary>
        public const int INITIAL_POOL_SIZE = 5;

        /// <summary>
        /// The callback delegate for Mailman callbacks
        /// </summary>
        /// <typeparam name="T">The mail class type <see cref="Mail"/></typeparam>
        /// <param name="mail">The mail required in the callback function signature</param>
        public delegate void CallbackHandler<T>(T mail) where T : Mail;

        private static Dictionary<int, MailDispatchData> mailDispatchDict;

        private static bool dispatching = false;
        private static Type dispatchingType;
        //private static List<CallbackData> enqueuedDispatch = new List<CallbackData>();
        private static Dictionary<int, List<CallbackData>> enqueuedDispatch = new Dictionary<int, List<CallbackData>>();
        //private static List<CallbackData> removalEnqueueDispatch = new List<CallbackData>();
        private static Dictionary<int, List<CallbackData>> removalEnqueueDispatch = new Dictionary<int, List<CallbackData>>();

        /// <summary>
        /// Register a listener with a parametre of type T
        /// </summary>
        /// <typeparam name="T">The mail type <see cref="Mail"/></typeparam>
        /// <param name="l">The listener to register</param>
        /// <param name="priority">The priority of this listener, defaults to 0, anything larger than 0 will dispatch first</param>
        /// <param name="defaultPoolSize">The default pool size for the mail objects, default: <see cref="INITIAL_POOL_SIZE"/></param>
        /// <param name="createPool">If the pool does not exist should the function make one, default: True</param>
        public static void AddListener<T>(CallbackHandler<T> l, int priority = 0, int defaultPoolSize = INITIAL_POOL_SIZE, bool createPool = true) where T : Mail, new()
        {
            Type mailType = typeof(T);

            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            int cachedHash = FastReflections.GetCachedHash<T>();
            bool inPool = mailDispatchDict.TryGetValue(cachedHash, out var m);
            if (!inPool)
            {
                m = new MailDispatchData();
                m.pool = new Stack<Mail>();
                m.subscribers = new List<CallbackData>();
                //Pool is unsorted on additional of listener only
                m.Sorted = false;
                CallbackData callbackData = new CallbackData() { Callback = (item) => l((T)item), Hash = l.GetHashCode(), Priority = priority};
                m.subscribers.Add(callbackData);
                mailDispatchDict.Add(cachedHash, m);

                if (createPool)
                {
                    //Construct object pool
                    CreatePoolForMail<T>(defaultPoolSize, m);
                }
            }
            else
            {
                CallbackData callbackData = new CallbackData() { Callback = (item) => l((T)item), Hash = l.GetHashCode(), Priority = priority};
                //if (!dispatching)
                //    m.subscribers.Add(callbackData);
                //else if (dispatchingType != null && dispatchingType == mailType)
                //    enqueuedDispatch.Add(callbackData);
                //else
                //    m.subscribers.Add(callbackData);

                if (!dispatching)
                    m.subscribers.Add(callbackData);
                else
                {
                    if (enqueuedDispatch.ContainsKey(cachedHash))
                        enqueuedDispatch[cachedHash].Add(callbackData);
                    else
                        enqueuedDispatch.Add(cachedHash, new List<CallbackData>() { callbackData });
                }
                m.Sorted = false;
            }
        }

        /// <summary>
        /// Removes a listener from the subscribers list
        /// </summary>
        /// <typeparam name="T">The type to look for <see cref="Mail"/></typeparam>
        /// <param name="l">The delegate to remove</param>
        public static void RemoveListener<T>(CallbackHandler<T> l) where T : Mail, new()
        {
            MailDispatchData m;

            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            int cachedHash = FastReflections.GetCachedHash<T>();
            bool inPool = mailDispatchDict.TryGetValue(cachedHash, out m);
            Action<object> removal = (item) => l((T)item);

            if (inPool)
            {
                //if (dispatching && dispatchingType == typeof(T))
                //{
                //    CallbackData callbackData = new CallbackData() { Callback = (item) => l((T)item), Hash = l.GetHashCode()};
                //    removalEnqueueDispatch.Add(callbackData);
                //}
                //else
                //{
                //    m.subscribers.RemoveAll((a1) => a1.Hash == l.GetHashCode());
                //}
                if(dispatching)
                {
                    CallbackData callbackData = new CallbackData() { Callback = (item) => l((T)item), Hash = l.GetHashCode() };
                    if (removalEnqueueDispatch.ContainsKey(cachedHash))
                        removalEnqueueDispatch[cachedHash].Add(callbackData);
                    else
                        removalEnqueueDispatch.Add(cachedHash, new List<CallbackData>() { callbackData });
                }
                else
                    m.subscribers.RemoveAll((a1) => a1.Hash == l.GetHashCode());
            }
        }

        /// <summary>
        /// Sends mail to all listerers, clears the mail and returns it to its pool if it exists.
        /// </summary>
        /// <typeparam name="T">The type of mail to send <see cref="Mail"/></typeparam>
        /// <param name="letter">The actual mail instance to send <see cref="Mail"/></param>
        /// <param name="createPoolIfMissing">Should the function create a pool for the mail class if missing, Default: True</param>
        /// <param name="defaultPoolSize">The default pool size for the mail objects, default: <see cref="INITIAL_POOL_SIZE"/></param>
        public static void SendMail<T>(T letter, bool createPoolIfMissing = true, int defaultPoolSize = INITIAL_POOL_SIZE) where T : Mail, new()
        {
            Type mailType = typeof(T);
            MailDispatchData m;

            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            bool inPool = mailDispatchDict.TryGetValue(letter.CachedHash, out m);

            if (inPool)
            {
                if (!m.Sorted)
                {
                    m.subscribers.Sort((a, b) => a.Priority < b.Priority ? 1 : a.Priority > b.Priority ? -1 : 0);
                    m.Sorted = true;
                }

                dispatching = true;
                dispatchingType = mailType;
                for (int i = 0; i < m.subscribers.Count; i++)
                {
                    m.subscribers[i].Callback.Invoke(letter);
                }

                dispatching = false;
                if (enqueuedDispatch.Count > 0)
                {
                    //for (int i = 0; i < enqueuedDispatch.Count; i++)
                    //{
                    //    m.subscribers.Add(enqueuedDispatch[i]);
                    //}
                    //enqueuedDispatch.Clear();

                    foreach(var entry in enqueuedDispatch)
                    {
                        MailDispatchData tempEnqueueData;
                        mailDispatchDict.TryGetValue(entry.Key, out tempEnqueueData);
                        for(int i = 0; i < entry.Value.Count; i++)
                        {
                            tempEnqueueData.subscribers.Add(entry.Value[i]);
                        }
                    }
                    enqueuedDispatch.Clear();
                }

                if (removalEnqueueDispatch.Count > 0)
                {
                    //for (int i = 0; i < removalEnqueueDispatch.Count; i++)
                    //{
                    //    for (int j = 0; j < m.subscribers.Count; j++)
                    //    {
                    //        if (removalEnqueueDispatch[i].Hash == m.subscribers[j].Hash)
                    //        {
                    //            m.subscribers.RemoveAt(j);
                    //            break;
                    //        }
                    //    }
                    //}
                    foreach(var entry in removalEnqueueDispatch)
                    {
                        MailDispatchData tempEnqueueData;
                        mailDispatchDict.TryGetValue(entry.Key, out tempEnqueueData);
                        for(int i = 0; i < entry.Value.Count; i++)
                        {
                            tempEnqueueData.subscribers.RemoveAll((x) => x.Hash == entry.Value[i].Hash);
                        }
                    }

                    removalEnqueueDispatch.Clear();
                }

                dispatchingType = null;
            }

            //Clear and return mail to pool
            letter.Clear();
            if (inPool)
            {
                m.pool.Push(letter);
            }
            else if (createPoolIfMissing)
            {
                m = new MailDispatchData();
                m.pool = new Stack<Mail>();
                m.subscribers = new List<CallbackData>();
                mailDispatchDict.Add(letter.CachedHash, m);

                var stack = CreatePoolForMail<T>(defaultPoolSize, m);
                stack.Push(letter);
            }
        }

        /// <summary>
        /// Returns a specified mail object from a pool, if empty or does not exist, creates new pool and mail.
        /// </summary>
        /// <typeparam name="T">The type of mail to fetch <see cref="Mail"/></typeparam>
        /// <param name="defaultPoolSize">The default pool size for the mail objects, default: <see cref="INITIAL_POOL_SIZE"/></param>
        /// <param name="createPoolIfMissing">Should the function create a pool for the mail class if missing, Default: True</param>
        /// <returns>The pooled mail instance</returns>
        public static T FetchPooledMail<T>(int defaultPoolSize = INITIAL_POOL_SIZE, bool createPoolIfMissing = true) where T : Mail, new()
        {
            Type mailType = typeof(T);
            MailDispatchData m;

            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            int cachedHash = FastReflections.GetCachedHash<T>();
            bool inPool = mailDispatchDict.TryGetValue(cachedHash, out m);
            if (!inPool && createPoolIfMissing)
            {
                m = new MailDispatchData();
                m.pool = new Stack<Mail>();
                m.subscribers = new List<CallbackData>();
                mailDispatchDict.Add(cachedHash, m);

                //Construct object pool
                var stack = CreatePoolForMail<T>(defaultPoolSize, m);
                return (T)stack.Pop();
            }
            else if (inPool)
            {
                if (m.pool.Count == 0)
                    return (T)Activator.CreateInstance(mailType);
                else
                    return (T)m.pool.Pop();
            }

            return (T)Activator.CreateInstance(mailType);
        }

        /// <summary>
        /// Returns the size of the pool for a specific mail type
        /// </summary>
        /// <typeparam name="T">The type of mail to check <see cref="Mail"/></typeparam>
        /// <returns>Returns the size of the pool and -1 if it does not exist</returns>
        public static int GetPoolSizeForPooledType<T>() where T : Mail, new()
        {
            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            int cachedHash = FastReflections.GetCachedHash<T>();
            int size = -1;
            if (mailDispatchDict.ContainsKey(cachedHash))
                size = mailDispatchDict[cachedHash].pool.Count;

            return size;
        }

        /// <summary>
        /// Returns the number of subscribers of a specific mail type
        /// </summary>
        /// <typeparam name="T">The mail type to check <see cref="Mail"/></typeparam>
        /// <returns>Returns the number of subscirbers or -1 if it does not exist</returns>
        public static int GetSubscriberCountForPooledType<T>() where T : Mail, new()
        {
            if (mailDispatchDict == null)
                mailDispatchDict = new Dictionary<int, MailDispatchData>();

            int cachedHash = FastReflections.GetCachedHash<T>();
            int size = -1;
            if (mailDispatchDict.ContainsKey(cachedHash))
                size = mailDispatchDict[cachedHash].subscribers.Count;
            return size;
        }

        private static Stack<Mail> CreatePoolForMail<T>(int defaultPoolSize, MailDispatchData m) where T : Mail, new()
        {
            Type mailType = typeof(T);
            for (int i = 0; i < defaultPoolSize; i++)
                m.pool.Push((T)Activator.CreateInstance(mailType));

            return m.pool;
        }

        /// <summary>
        /// Creates a new instance of a type (faster than Activator but more GC)
        /// </summary>
        /// <typeparam name="T">The type to create an instance from</typeparam>
        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (
                                                       Expression.New(typeof(T))
                                                      ).Compile();
        }

        /// <summary>
        /// A static helper class for generating fast reflection
        /// </summary>
        public static class FastReflections
        {
            /// <summary>
            /// Returns the reflected cached hash value for a type
            /// </summary>
            /// <typeparam name="T">The type to get the cached hash from</typeparam>
            /// <returns>The cached hash</returns>
            public static int GetCachedHash<T>() => CachedHash<T>.Hash;

            private static class CachedHash<T>
            {
                public static readonly int Hash = (typeof(T).GetCustomAttributes(typeof(CachedHashAttribute), true)[0] as CachedHashAttribute).CachedHash;
            }
        }
    }
}