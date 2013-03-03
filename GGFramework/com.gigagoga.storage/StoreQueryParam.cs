using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.gigagoga.storage
{
    public class StoreQueryParam
    {
        private String customQuery;
        private String propertyFormat;
        private String propertyName;
        private Object propertyValue;

        public StoreQueryParam(string propertyName)
        {
            this.propertyName = propertyName;
            this.propertyValue = null;
        }

        public StoreQueryParam Equals(Int32 value)
        {
            propertyFormat = " {0} = {1}";
            this.propertyValue = value;
            return this;
        }

        public StoreQueryParam Equals(Int64 value)
        {
            propertyFormat = " {0} = {1}";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam LessThan(Int64 value)
        {
            propertyFormat = " {0} < {1}";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam GreaterThan(Int64 value)
        {
            propertyFormat = " {0} > {1}";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam Not(Int64 value)
        {
            propertyFormat = " {0} <> {1}";
            this.propertyValue = value;
            return this;
        }

        public StoreQueryParam Equals(String value)
        {
            propertyFormat = " {0} = '{1}'";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam Not(String value)
        {
            propertyFormat = " {0} <> '{1}'";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam StartsWith(String value)
        {
            propertyFormat = " {0} LIKE '{1}%'";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam EndsWith(String value)
        {
            propertyFormat = " {0} LIKE '%{1}'";
            this.propertyValue = value;
            return this;
        }
        public StoreQueryParam Contains(String value)
        {
            propertyFormat = " {0} LIKE '%{1}%'";
            this.propertyValue = value;
            return this;
        }

        public StoreQueryParam ContainsAny(String[] values)
        {
            propertyFormat = "";

            foreach (String value in values)
            {
                propertyFormat += String.Format(" {{0}} LIKE '%{1}%' OR", "0", value);
            }
            // Remove last OR
            propertyFormat = propertyFormat.TrimEnd('O', 'R');
            // Set the value to nothing, just to keep the String.Format ok.
            this.propertyValue = String.Empty;
            return this;
        }

        public StoreQueryParam ContainsAll(String[] values)
        {
            propertyFormat = "";

            foreach (String value in values)
            {
                propertyFormat += String.Format(" {{0}} LIKE '%{1}%' AND", "0", value);
            }
            // Remove last OR
            propertyFormat = propertyFormat.TrimEnd('A', 'N', 'D');
            // Set the value to nothing, just to keep the String.Format ok.
            this.propertyValue = String.Empty;
            return this;
        }


        public StoreQueryParam Between(DateTime start, DateTime end)
        {
            this.customQuery = String.Format(" {0} BETWEEN '{1}' AND '{2}'", this.propertyName, start.ToString(), end.ToString());
            return this;
        }


        public StoreQueryParam Contains(Enum enumValue)
        {
            propertyFormat = " {0} & {1} = {1}";
            this.propertyValue = Convert.ToInt64(enumValue);
            return this;
        }


        public StoreQueryParam ContainsAny(params Enum[] enumValues)
        {
            propertyFormat = "";// = "{0} & {1} = {1} AND";

            foreach (Enum enumValue in enumValues)
            {
                propertyFormat += String.Format(" {{0}} & {1} = {1} OR", "0", Convert.ToInt64(enumValue));
            }
            // Remove last OR
            propertyFormat = propertyFormat.TrimEnd('O', 'R');
            // Set the value to nothing, just to keep the String.Format ok.
            this.propertyValue = String.Empty;
            return this;
        }

        public StoreQueryParam ContainsAll(params Enum[] enumValues)
        {
            propertyFormat = "";

            foreach (Enum enumValue in enumValues)
            {
                propertyFormat += String.Format(" {{0}} & {1} = {1} AND", "0", Convert.ToInt64(enumValue));
            }
            // Remove last OR
            propertyFormat = propertyFormat.TrimEnd('A', 'N', 'D');
            // Set the value to nothing, just to keep the String.Format ok.
            this.propertyValue = String.Empty;
            return this;
        }

        public StoreQueryParam ContainsNone(params Enum[] enumValues)
        {
            propertyFormat = "";

            foreach (Enum enumValue in enumValues)
            {
                propertyFormat += String.Format(" {{0}} & {1} = 0 AND", "0", Convert.ToInt64(enumValue));
            }
            // Remove last OR
            propertyFormat = propertyFormat.TrimEnd('A', 'N', 'D');
            // Set the value to nothing, just to keep the String.Format ok.
            this.propertyValue = String.Empty;
            return this;
        }




        public StoreQueryParam In(List<Int64> values)
        {
            List<object> l = new List<object>();
            foreach (Int64 value in values)
            {
                l.Add(String.Format("{0}", value));
            }
            return _in(l);
        }
        public StoreQueryParam In(List<String> values)
        {
            List<object> l = new List<object>();
            foreach (String value in values)
            {
                l.Add(String.Format("'{0}'", value));
            }
            return _in(l);
        }

        public StoreQueryParam In(object subQuery)
        {
            propertyFormat = " {0} IN({1})";
            this.propertyValue = subQuery.ToString();

            return this;
        }

        private StoreQueryParam _in(List<Object> values)
        {
            propertyFormat = " {0} IN({1})";
            List<String> l = new List<string>(
                values.ConvertAll<String>(
                    delegate(Object o)
                    { return o.ToString(); }));
            this.propertyValue = String.Join(",", l.ToArray());

            return this;
        }

        public override string ToString()
        {
            if (customQuery != null)
            {
                return customQuery;
            }

            if (propertyValue == null)
            {
                propertyValue = "NULL";
                propertyFormat = " {0} IS {1}";
            }
            else if (propertyFormat == null)
            {
                propertyFormat = " {0} = '{1}'";
            }

            return String.Format(propertyFormat, propertyName, propertyValue);
        }
    }
}
