using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using com.gigagoga.storage.meta;
using System.Data;

namespace com.gigagoga.storage
{
    public abstract class StorableBase : IStorable, IReadable
    {
        public abstract Int64 ID
        {
            get;
            set;
        }

        /*
        public abstract StorableObjectState State
        {
            get;
            set;
        }
        */

        /*
        /// <summary>
        /// Virtual method that enables you to check for linked objects
        /// that are affected by this create / update command.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="wasHandled"></param>
        /// <returns></returns>
        public virtual bool HandlePut(Store store, out bool wasHandled)
        {
            wasHandled = false;
            return true;
        }
        */

        /*
        /// <summary>
        /// Virtual method that enables you to check for linked objects
        /// that are affected by this delete.
        /// </summary>
        public virtual bool HandleDelete(Store store, out bool wasHandled)
        {
            wasHandled = false;
            return true;
        }
        */

        /// <summary>
        /// Base class method that uses reflection to read all variables
        /// that are attributed with a StorableField.
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<String, Object> GetValues()
        {
            IDictionary<String, Object> fieldValues = new Dictionary<String, Object>();

            // Read all the fields with custom attribute:
            Type type = this.GetType();

            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (fieldInfo.IsDefined(typeof(StorableAttribute), true))
                {
                    StorableAttribute attribute = fieldInfo.GetCustomAttributes(typeof(StorableAttribute), true)[0] as StorableAttribute;
                    attribute.ensureName(fieldInfo.Name);
                    fieldValues.Add(attribute.Name, fieldInfo.GetValue(this));
                }
            }

            foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (propInfo.IsDefined(typeof(StorableAttribute), true))
                {
                    StorableAttribute attribute = propInfo.GetCustomAttributes(typeof(StorableAttribute), true)[0] as StorableAttribute;
                    attribute.ensureName(propInfo.Name);
                    fieldValues.Add(attribute.Name, propInfo.GetValue(this, null));
                }
            }
            return fieldValues;
        }

        /// <summary>
        /// Base class method that uses reflection to look for matching
        /// fields or props by name and type.
        /// </summary>
        /// <param name="fieldValues"></param>
        public virtual void InitFromObject(IDictionary<String, Object> fieldValues)
        {
            Type type = this.GetType();

            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                foreach (StorableAttribute attr in fieldInfo.GetCustomAttributes(typeof(StorableAttribute), true) as StorableAttribute[])
                {
                    attr.ensureName(fieldInfo.Name);
                    if (attr != null && fieldValues.ContainsKey(attr.Name))
                    {
                        object value = fieldValues[attr.Name];
                        if (! (value is System.DBNull))
                        {
                            fieldInfo.SetValue(this, value);
                        }
                    }
                }
            }

            foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (StorableAttribute attr in propInfo.GetCustomAttributes(typeof(StorableAttribute), true) as StorableAttribute[])
                {
                    attr.ensureName(propInfo.Name);
                    if (attr != null && fieldValues.ContainsKey(attr.Name))
                    {
                        object value = fieldValues[attr.Name];
                        if (!(value is System.DBNull))
                        {
                            propInfo.SetValue(this, value, null);
                        }
                    }
                }
            }
        }


        public virtual void InitFromObject(IDataReader dataReader)
        {
            IDictionary<String, Object> dictionary = new Dictionary<String, Object>();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                dictionary.Add(dataReader.GetName(i), dataReader[i]);
            }
            InitFromObject(dictionary);
        }


        public virtual void InitFromObject(object obj)
        {
            
        }
    }
}
