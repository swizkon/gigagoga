using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.gigagoga.storage
{
    /// <summary>
    /// Represents the abstraction for a generic readable object.
    /// In most cases this will be the data reader result from a custom query or a view.
    /// </summary>
    public class TReadableObject : StorableObject, IReadable
    {
        IDictionary<String, Object> _values = null;


        public Object this[string name]
        {
            get
            {
                return _values[name];
            }
            set
            {
                /*
                State |= StorableObjectState.Modified;
                */
                _values[name] = value;
            }
        }


        public override void InitFromObject(IDictionary<string, object> fieldValues)
        {
            _values = fieldValues;
        }

        public override IDictionary<String, Object> GetValues()
        {
            return _values;
        }
    }
}
