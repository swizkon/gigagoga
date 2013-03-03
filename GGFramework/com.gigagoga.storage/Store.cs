using System;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using com.gigagoga.storage.meta;
using System.Configuration;

namespace com.gigagoga.storage
{
    public class Store : IDisposable
    {
        private static bool enableEvents = false;

        public static bool EnableStoreEventListener
        {
            get { return enableEvents; }
            set { enableEvents = value; }
        }

        /*
        private static bool searchForStorableFields = true;
        /// <summary>
        /// Option to reflect fields in order to find StorableField attributes.
        /// Default is false
        /// </summary>
        public static bool SearchForStorableFields
        {
            get { return searchForStorableFields; }
            set { searchForStorableFields = value; }
        }
        */

        /*
        private static bool searchForStorableProps = false;
        /// <summary>
        /// Option to reflect props in order to find StorableProp attributes.
        /// Default is false
        /// </summary>
        public static bool SearchForStorableProps
        {
            get { return searchForStorableProps; }
            set { searchForStorableProps = value; }
        }
        */

        private StoreType typeOfStore = StoreType.None;
        private object store = null;
        private bool isInternallyInitiated = false;

        public Store(XmlNode storeNode)
        {
            this.typeOfStore = StoreType.XmlDocument;
            this.store = storeNode;
        }

        public Store(ConnectionStringSettings settings)
        {
            if (String.IsNullOrEmpty(settings.ProviderName))
            {
                throw new ArgumentException("The ProviderName property has not been set.", "settings");
            }

            if (settings.ProviderName == "System.Data.SqlClient")
            {
                this.typeOfStore = StoreType.SQLServer;
                store = new SqlConnection(settings.ConnectionString);
            }
            else if (settings.ProviderName == "System.Data.Odbc")
            {
                this.typeOfStore = StoreType.MySQL;
                store = new OdbcConnection(settings.ConnectionString);
            }
            isInternallyInitiated = true;
            IDbConnection istore = store as IDbConnection;
            if (istore != null)
            {
                istore.Open();
            }
        }

        public Store(IDbConnection connection)
        {
            this.store = connection;
            if (connection is SqlConnection)
            {
                this.typeOfStore = StoreType.SQLServer;
            }
            else if (connection is OdbcConnection && connection.ConnectionString != null && connection.ConnectionString.ToLower().Contains("mysql"))
            {
                this.typeOfStore = StoreType.MySQL;
            }
        }


        // public bool Create<TStorable>(TStorable storable, out long newid, out String errorMessage) where TStorable : IStorable, new()
        public bool Create<TStorable>(TStorable storableObj, out long newid) where TStorable : IStorable, new()
        {
            // errorMessage = null;
            bool ok = false;

            newid = 0;


            IDictionary<String, Object> storable_Values = storableObj.GetValues();

            // Check the type for values that should be auto generated:
            foreach (StorableAttribute storable in util.StorageUtil.GetStorables(storableObj.GetType()))
            {
                if (storable.DataType == StorableDataType.AutoIdentity)
                {
                    storable_Values.Remove(storable.Name);
                }
            }

            if (store is IDbConnection)
            {
                string statement = "INSERT INTO {0}({1}) VALUES({2})";

                string fields = "";
                string values = "";


                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    using (IEnumerator<KeyValuePair<String, Object>> iterator = storable_Values.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (iterator.Current.Value != null)
                            {
                                fields += ',' + iterator.Current.Key;
                                values += ',' + ((typeOfStore == StoreType.SQLServer) ? "@" + iterator.Current.Key : "?");

                                IDataParameter parameter = command.CreateParameter();
                                parameter.ParameterName = "@" + iterator.Current.Key;
                                parameter.Value = iterator.Current.Value;
                                command.Parameters.Add(parameter);
                            }
                        }
                    }
                    command.CommandText = String.Format(statement, util.StorageUtil.GetStorableMeta(storableObj.GetType()).Name, fields.Trim(','), values.Trim(','));

                    try
                    {
                        command.ExecuteNonQuery();
                        /*
                        storable.State = StorableObjectState.Created;
                        */
                        ok = true;
                        object scalar = new object();
                        try
                        {
                            command.CommandText = (this.typeOfStore == StoreType.SQLServer) ? "SELECT @@IDENTITY" : "SELECT LAST_INSERT_ID()";
                            scalar = command.ExecuteScalar();
                            if (typeOfStore == StoreType.MySQL)
                            {
                                newid = Convert.ToInt64(scalar);
                            }
                            else
                            {
                                newid = (long)(Decimal)scalar;
                            }
                            storableObj.ID = newid;
                            if (Store.EnableStoreEventListener)
                            {
                                StoreEventListener.OnChanged(storableObj, new StoreEventArgs(StorableObjectState.Created));
                            }
                        }
                        catch (Exception scalarEx)
                        {
                            newid = 0;
                            throw scalarEx;
                        }
                    }
                    catch (Exception ex)
                    {
                        newid = 0;
                        ok = false;
                        throw ex;
                    }
                }
            }
            else if (store is XmlNode)
            {
                String metaDataName = util.StorageUtil.GetStorableMeta(storableObj.GetType()).Name;

                XmlElement element = (XmlElement)(store as XmlNode).AppendChild((store as XmlNode).OwnerDocument.CreateElement(metaDataName));

                using (IEnumerator<KeyValuePair<String, Object>> iter = storable_Values.GetEnumerator())
                {
                    while (iter.MoveNext())
                    {
                        if (iter.Current.Value == null)
                        {
                            element.RemoveAttribute(iter.Current.Key);
                        }
                        else
                        {
                            element.SetAttribute(iter.Current.Key, iter.Current.Value.ToString());
                        }
                    }
                }
                ok = true;

            }

            return ok;
        }

        public bool TryCreate<TStorable>(TStorable storable, out long newid, out Exception exception) where TStorable : IStorable, new()
        {
            bool createOK = false;
            try
            {
                createOK = Create<TStorable>(storable, out newid);
                exception = null;
            }
            catch (Exception ex)
            {
                newid = 0;
                exception = ex;
                createOK = false;
            }
            return createOK;
        }

        public bool TryCreate<TStorable>(TStorable storable, out long newid, out String errorMessage) where TStorable : IStorable, new()
        {
            errorMessage = null;
            Exception ex = null;
            bool createOK = TryCreate<TStorable>(storable, out newid, out ex);
            if (ex != null && ex.Message != null)
            {
                errorMessage = ex.Message;
            }
            return createOK;
        }

        public bool Update<TStorable>(TStorable storable) where TStorable : IStorable, new()
        {
            bool ok = false;

            IDictionary<String, Object> storable_Values = storable.GetValues();

            // Check the type for values that should not be updated:
            foreach (StorableAttribute storableAttr in util.StorageUtil.GetStorables(storable.GetType()))
            {
                if (storableAttr.DataType == StorableDataType.AutoIdentity || storableAttr.Info == StorableInfo.ReadOnly)
                {
                    storable_Values.Remove(storableAttr.Name);
                }
            }

            if (store is IDbConnection)
            {
                String stmt = "UPDATE {0} SET {2} WHERE id = {1}";
                string values = "";

                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    using (IEnumerator<KeyValuePair<String, Object>> iterator = storable_Values.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (iterator.Current.Value != null)
                            {
                                values += iterator.Current.Key + '='
                                    + ((typeOfStore == StoreType.SQLServer) ? "@" + iterator.Current.Key : "?")
                                    + ',';

                                IDataParameter parameter = command.CreateParameter();
                                parameter.ParameterName = "@" + iterator.Current.Key;
                                parameter.Value = iterator.Current.Value;
                                command.Parameters.Add(parameter);
                            }
                        }
                    }
                    command.CommandText = String.Format(stmt
                                            , util.StorageUtil.GetStorableMeta(storable.GetType()).Name
                                            , storable.ID
                                            , values.Trim(','));
                    try
                    {
                        command.ExecuteNonQuery();
                        /*
                        storable.State = StorableObjectState.Updated;
                        */
                        if (Store.EnableStoreEventListener)
                        {
                            StoreEventListener.OnChanged(storable, new StoreEventArgs(StorableObjectState.Updated));
                        }
                        ok = true;
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        throw ex;
                    }
                }
            }
            else if (store is XmlNode)
            {
                String metaDataName = util.StorageUtil.GetStorableMeta(storable.GetType()).Name;
                String xpath = "//" + metaDataName + "[@id='" + storable.ID.ToString() + "']";
                XmlElement element = (XmlElement) (store as XmlNode).SelectSingleNode(xpath);
                //.AppendChild((store as XmlNode).OwnerDocument.CreateElement(metaDataName));

                if (element != null)
                {
                    using (IEnumerator<KeyValuePair<String, Object>> iter = storable_Values.GetEnumerator())
                    {
                        while (iter.MoveNext())
                        {
                            if (iter.Current.Value == null)
                            {
                                element.RemoveAttribute(iter.Current.Key);
                            }
                            else
                            {
                                element.SetAttribute(iter.Current.Key, iter.Current.Value.ToString());
                            }
                        }
                    }
                    ok = true;
                }
                else
                {
                    ok = false;
                }
            }

            return ok;
        }


        internal StoreType getTypeOfStore()
        {
            return this.typeOfStore;
        }

        public TReadable ReadFirstOrDefault<TReadable>(StoreQueryParam param1) where TReadable : IReadable, new()
        {
            return ReadFirstOrDefault<TReadable>(param1, null, null);
        }
        public TReadable ReadFirstOrDefault<TReadable>(StoreQueryParam param1, StoreQueryParam param2) where TReadable : IReadable, new()
        {
            return ReadFirstOrDefault<TReadable>(param1, param2, null);
        }

        public TReadable ReadFirstOrDefault<TReadable>(StoreQueryParam param1, StoreQueryParam param2, StoreQueryParam param3) where TReadable : IReadable, new()
        {
            return ReadFirstOrDefault<TReadable>(
                StoreQuery.QueryFor<TReadable>(this, 
                new StoreQueryParam[] { param1, param2, param3 }));
            /*
            return ReadFirstOrDefault<TReadable>(new StoreQueryParam[] { param1, param2, param3 });
            */
        }

        /*
        public TReadable ReadFirstOrDefault<TReadable>(params StoreQueryParam[] matchAll) where TReadable : IReadable, new()
        {
            StoreQuery storeQuery = StoreQuery.Create<TReadable>(this, matchAll).Range(0, 1);
            return ReadFirstOrDefault<TReadable>(storeQuery);
        }
        */

        public TReadable ReadFirstOrDefault<TReadable>(StoreQuery storeQuery) where TReadable : IReadable, new()
        {
            storeQuery.Range(0, 1);
            return ReadFirstOrDefault<TReadable>(storeQuery.ToString());
        }

        public TReadable ReadFirstOrDefault<TReadable>(string query) where TReadable : IReadable, new()
        {
            TReadable t = new TReadable();
            List<TReadable> ts = Read<TReadable>(query);
            if (ts.Count > 0)
            {
                t = ts[0];
            }
            return t;
        }


        public TReadable Read<TReadable>(long id, out String errorMessage) where TReadable : IReadable, new()
        {
            errorMessage = null;

            TReadable t = new TReadable();
            List<TReadable> ts = Read<TReadable>(new StoreQueryParam("id").Equals(id), out errorMessage);
            if (ts.Count > 0)
            {
                t = ts[0];
            }
            else
            {
                errorMessage = "NotFound";
            }
            return t;
        }

        public List<TReadable> Read<TReadable>(StoreQuery storeQuery, out String errorMessage) where TReadable : IReadable, new()
        {
            errorMessage = null;
            List<TReadable> ts = new List<TReadable>();
            // Select every row that matches...
            if (store is IDbConnection)
            {

                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    command.CommandText = storeQuery.ToString();
                    try
                    {
                        using (IDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                TReadable current = new TReadable();
                                current.InitFromObject(fromDataReader(dr));
                                ts.Add(current);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message + ",  " + storeQuery.ToString();
                    }
                }
            }

            return ts;
        }


        public List<TReadable> Read<TReadable>(StoreQuery storeQuery) where TReadable : IReadable, new()
        {
            return Read<TReadable>(storeQuery.ToString());
        }

        public List<TReadable> Read<TReadable>(String query) where TReadable : IReadable, new()
        {
            List<TReadable> ts = new List<TReadable>();
            // Select every row that matches...
            if (store is IDbConnection)
            {
                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    command.CommandText = query;
                    using (IDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            TReadable current = new TReadable();
                            current.InitFromObject(fromDataReader(dr));
                            ts.Add(current);
                        }
                    }
                }
            }
            return ts;
        }

        /*
        public List<TReadable> TryRead<TReadable>(StoreQuery storeQuery, out String errorMessage) where TReadable : IReadable, new()
        {
            errorMessage = null;
            List<TReadable> ts = new List<TReadable>();
            try
            {
                ts = Read<TReadable>(storeQuery);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + ",  " + storeQuery.ToString();
            }
            return ts;
        }
        */


        public List<TReadable> Read<TReadable>(String customSelectQuery, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this).CustomQuery(customSelectQuery);
            return Read<TReadable>(q, out errorMessage);
        }
        public List<TReadable> Read<TReadable>(StoreQueryParam param1, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1);
            return Read<TReadable>(q, out errorMessage);
        }
        public List<TReadable> Read<TReadable>(StoreQueryParam param1, StoreQueryParam param2, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1, param2);
            return Read<TReadable>(q, out errorMessage);
        }
        public List<TReadable> Read<TReadable>(StoreQueryParam param1, StoreQueryParam param2, StoreQueryParam param3, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1, param2, param3);
            return Read<TReadable>(q, out errorMessage);
        }


        public List<TReadable> Read<TReadable>(params StoreQueryParam[] matchAll) where TReadable : IReadable, new()
        {
            StoreQuery storeQuery = StoreQuery.QueryFor<TReadable>(this, matchAll);
            return Read<TReadable>(storeQuery);
        }

        public long Count<TReadable>(String customCountQuery, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this).CustomQuery(customCountQuery);
            return Count<TReadable>(q, out errorMessage);
        }
        public long Count<TReadable>(StoreQueryParam param1, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1);
            return Count<TReadable>(q, out errorMessage);
        }
        public long Count<TReadable>(StoreQueryParam param1, StoreQueryParam param2, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1, param2);
            return Count<TReadable>(q, out errorMessage);
        }
        public long Count<TReadable>(StoreQueryParam param1, StoreQueryParam param2, StoreQueryParam param3, out String errorMessage) where TReadable : IReadable, new()
        {
            StoreQuery q = StoreQuery.QueryFor<TReadable>(this, param1, param2, param3);
            return Count<TReadable>(q, out errorMessage);
        }
        public long Count<TReadable>(StoreQuery storeQuery, out String errorMessage) where TReadable : IReadable, new()
        {
            long count = 0;

            errorMessage = null;
            if (store is IDbConnection)
            {
                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    command.CommandText = storeQuery.ToCountString();
                    try
                    {
                        object c = command.ExecuteScalar();
                        count = Convert.ToInt64(c);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                    }
                }
            }

            return count;
        }

        public bool TryUpdate<T>(T storable, out Exception exeption) where T : IStorable, new()
        {
            bool updateOK = false;
            try
            {
                updateOK = Update<T>(storable);
                exeption = null;
            }
            catch (Exception ex)
            {
                exeption = ex;
                updateOK = false;
            }

            return updateOK;
        }

        /// <summary>
        /// Deletes the objects that matches the StoreQuery.
        /// The result array contains the storables that was deleted OK.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deleteQuery"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public TStorable[] BatchDelete<TStorable>(StoreQuery deleteQuery, out String errorMessage) where TStorable : IStorable, new()
        {
            List<TStorable> deleted = new List<TStorable>();
            List<TStorable> queryResult = Read<TStorable>(deleteQuery, out errorMessage);
            for (int i = 0; i < queryResult.Count; i++)
            {
                if (Delete<TStorable>(queryResult[i], out errorMessage))
                {
                    deleted.Add(queryResult[i]);
                }
            }
            return deleted.ToArray();
        }


        public bool Delete<TStorable>(TStorable storable, out String errorMessage) where TStorable : IStorable, new()
        {
            errorMessage = null;

            if (store is IDbConnection)
            {
                using (IDbCommand command = (store as IDbConnection).CreateCommand())
                {
                    command.CommandText = String.Format("DELETE FROM {0} WHERE id = {1}", util.StorageUtil.GetStorableMeta(storable.GetType()).Name, storable.ID);
                    try
                    {
                        command.ExecuteNonQuery();
                        if (Store.EnableStoreEventListener)
                        {
                            StoreEventListener.OnChanged(storable, new StoreEventArgs(StorableObjectState.Deleted));
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                    }
                }
            }
            return (errorMessage == null);
        }

        private IDictionary<String, Object> fromDataReader(IDataReader dr)
        {
            IDictionary<String, Object> dictionary = new Dictionary<String, Object>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
                dictionary.Add(dr.GetName(i), dr[i]);
            }
            return dictionary;
        }

        public void Dispose()
        {
            if (isInternallyInitiated && store is IDisposable)
            {
                //  Open DB aswell? Close before dispose...
                if (store is IDbConnection)
                {
                    IDbConnection istore = store as IDbConnection;
                    if (istore != null && istore.State == ConnectionState.Open)
                    {
                        istore.Close();
                    }
                }

                (store as IDisposable).Dispose();
                store = null;
            }
        }
    }
}
