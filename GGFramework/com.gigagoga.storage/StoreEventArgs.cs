using System;
using System.Collections.Generic;
using System.Text;

namespace com.gigagoga.storage
{
    public class StoreEventArgs : EventArgs
    {
        private StorableObjectState state = StorableObjectState.None;

        // private IStorable storable = null;

        public StorableObjectState State
        {
            get { return this.state; }
        }

        public StoreEventArgs(StorableObjectState state)
        {
            this.state = state;
        }
    }
}
