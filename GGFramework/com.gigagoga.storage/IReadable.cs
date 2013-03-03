using System;
using System.Collections.Generic;
using System.Text;
using com.gigagoga.storage.meta;

namespace com.gigagoga.storage
{
    public interface IReadable
    {
        Int64 ID { get; set; }

        void InitFromObject(object obj);
        void InitFromObject(IDictionary<String, Object> keyValues);
    }
}
