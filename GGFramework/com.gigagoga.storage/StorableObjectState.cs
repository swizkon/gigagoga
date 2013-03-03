using System;

namespace com.gigagoga.storage
{
    [Flags]
    public enum StorableObjectState : byte
    {
        None = 0,
        New = 1,
        Created = 2,
        Retrieved = 4,
        Updated = 8,
        Deleted = 16,
        Modified = 32
    }
}
