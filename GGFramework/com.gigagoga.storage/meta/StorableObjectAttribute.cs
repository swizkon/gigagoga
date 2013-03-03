using System;

namespace com.gigagoga.storage.meta
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StorableObjectAttribute : Attribute
    {
        private bool storableFields = true;
        private bool storableProps = false;

        private String name = null;
        private StorableObjectCapability capabilities = StorableObjectCapability.Default;

        public static readonly StorableObjectAttribute Empty = new StorableObjectAttribute(null, StorableObjectCapability.None); 

        public String Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public StorableObjectCapability Capabilities
        {
            get { return this.capabilities; }
            set { this.capabilities = value; }
        }

        public Boolean StorableProps
        {
            get { return this.storableFields; }
            set { this.storableFields = value; }
        }

        public Boolean StorableFields
        {
            get { return this.storableFields; }
            set { this.storableFields = value; }
        }

        public StorableObjectAttribute()
        {
        }

        public StorableObjectAttribute(String entityName)
        {
            this.name = entityName;
            this.storableFields = true;
            this.storableProps = true;
        }

        public StorableObjectAttribute(String entityName, StorableObjectCapability capabilities)
        {
            this.name = entityName;
            this.capabilities = capabilities;
        }

        public StorableObjectAttribute(String entityName, bool storableFields, bool storableProps)
        {
            this.name = entityName;
            this.storableFields = storableFields;
            this.storableProps = storableProps;
        }
    }
}
