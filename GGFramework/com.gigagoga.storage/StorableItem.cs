using System;
using com.gigagoga.storage.meta;

/*
namespace com.gigagoga.storage
{
    /// <summary>
    /// An abstract class containing basic props shared by
    /// many objects. Inherits StorableBase.
    /// </summary>
    public abstract class StorableItem : StorableObject
    {
        [StorableField("item_name", StorableFieldDataType.Char64)]
        protected String name;

        [StorableField("item_desc", StorableFieldDataType.Varchar255, StorableFieldInfo.Optional)]
        protected String desc;

        public String Name
        {
            get { return this.name; }
            set
            {
                if (value.Length > 64)
                {
                    value = value.Substring(0, 64);
                }
                this.name = value;
            }
        }

        public String Description
        {
            get { return this.desc; }
            set
            {
                if (value.Length > 255)
                {
                    value = value.Substring(0, 250) + "...";
                }
                this.desc = value;
            }
        }
    }
}
*/
