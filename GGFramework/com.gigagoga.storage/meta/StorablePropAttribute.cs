using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

/*
namespace com.gigagoga.storage.meta
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StorablePropAttribute : Attribute
    {
        private String name = null;
        private int maxLength = -1;

        private StorableFieldDataType dataType = StorableFieldDataType.AutoDetect;

        private StorableFieldInfo info = StorableFieldInfo.Default;

        public String Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public StorableFieldDataType DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

        public StorableFieldInfo Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        public StorablePropAttribute(StorableFieldDataType dataType)
            : this(null, dataType, StorableFieldInfo.Default)
        {
        }

        public StorablePropAttribute(String name, StorableFieldDataType dataType)
            : this(name, dataType, StorableFieldInfo.Default)
        {
        }

        public StorablePropAttribute(String name, StorableFieldDataType dataType, StorableFieldInfo info)
        {
            this.name = name;
            this.dataType = dataType;
            this.info = info;
        }


        internal void ensureName(string fallbackName)
        {
            if (String.IsNullOrEmpty(this.name))
            {
                this.name = fallbackName;
            }
        }
    }
}
*/