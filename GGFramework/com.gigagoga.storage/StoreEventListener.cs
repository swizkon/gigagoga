using System;
using System.Collections.Generic;
using System.Text;

namespace com.gigagoga.storage
{
    /// <summary>
    /// A static class that enables customized actions on common events.
    /// 
    /// </summary>
    public static class StoreEventListener
    {
        private static object storeEventListenerLock = new object();
        private static EventHandler<StoreEventArgs> onChanged = null;

        /// <summary>
        /// Event fired when an item has been created,u pdated or deleted.
        /// The object parameter to the Event handler is the entry that was removed.
        /// Store.EnableStoreEventListener must be set to true to add delegate.
        /// </summary>
        public static event EventHandler<StoreEventArgs> Changed
        {
            add
            {
                if (Store.EnableStoreEventListener)
                {
                    lock (storeEventListenerLock)
                    {
                        onChanged += value;
                    }
                }
            }
            remove
            {
                lock (storeEventListenerLock)
                {
                    onChanged -= value;
                }
            }
        }

        internal static void OnChanged(object sender, StoreEventArgs e)
        {
            if (Store.EnableStoreEventListener)
            {
                if (onChanged != null)
                {
                    onChanged(sender, e);
                }
            }
        }
    }
}
