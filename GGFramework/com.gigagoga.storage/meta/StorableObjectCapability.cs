using System;

namespace com.gigagoga.storage.meta
{
    [Flags]
    public enum StorableObjectCapability : int
    {
        None = 0,
        Readable = 1,
        Writable = 2,
        Default = 3,
        Composite = 4,
        Linkable = 8
    }
}
