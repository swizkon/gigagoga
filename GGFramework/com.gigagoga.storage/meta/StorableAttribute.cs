using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace com.gigagoga.storage.meta
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class StorableAttribute : Attribute
    {
        private String name = null;
        private int maxLength = -1;

        private StorableDataType dataType = StorableDataType.AutoDetect;

        private StorableInfo info = StorableInfo.Default;

        public String Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public StorableDataType DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

        public StorableInfo Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        public StorableAttribute(StorableDataType dataType)
            : this(null, dataType, StorableInfo.Default)
        {
        }

        public StorableAttribute(String name, StorableDataType dataType)
            : this(name, dataType, StorableInfo.Default)
        {
        }

        public StorableAttribute(String name, StorableDataType dataType, StorableInfo info)
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
