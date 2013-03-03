using System;
using com.gigagoga.storage.meta;
using System.Collections.Generic;

namespace com.gigagoga.storage
{
    public interface IStorable : IReadable
    {
        IDictionary<String, Object> GetValues();

        /*
        bool HandlePut(Store store, out bool wasHandled);
        bool HandleDelete(Store store, out bool wasHandled);
        */
        /*
        StorableObjectState State
        {
            get;set;
        }
        */
    }
}
