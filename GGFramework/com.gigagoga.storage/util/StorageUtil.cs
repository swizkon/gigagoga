using System;
using System.Collections.Generic;
using System.Text;
using com.gigagoga.storage.meta;
using System.Reflection;
using System.Data;

namespace com.gigagoga.storage.util
{
    /// <summary>
    /// Contains static methods for 
    /// </summary>
    public static class StorageUtil
    {

        /// <summary>
        /// Method that uses reflection to get the
        /// StorableAttribute for a Type.
        /// </summary>
        /// <returns>Returns the StorableObjectAttribute or StorableObjectAttribute.Empty if not found</returns>
        public static StorableObjectAttribute GetStorableMeta(Type type)
        {
            // Check for a StoreableAttribute, return the first found:
            object[] attrs = type.GetCustomAttributes(typeof(StorableObjectAttribute), false);
            if (attrs.Length > 0)
            {
                return attrs[0] as StorableObjectAttribute;
            }
            return StorableObjectAttribute.Empty;
        }


        public static List<StorableAttribute> GetStorables(Type type)
        {
            StorableObjectAttribute objMeta = GetStorableMeta(type);
            List<StorableAttribute> storables = new List<StorableAttribute>();
            if (objMeta == StorableObjectAttribute.Empty)
            {
                return storables;
            }

            // Read all the fields
            if (objMeta.StorableFields)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo field in fields)
                {
                    if (field.IsDefined(typeof(StorableAttribute), true))
                    {
                        StorableAttribute attribute = field.GetCustomAttributes(typeof(StorableAttribute), true)[0] as StorableAttribute;
                        attribute.ensureName(field.Name);
                        storables.Add(attribute);
                    }
                }
            }

            // Read all the fields
            if (objMeta.StorableProps)
            {
                PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (PropertyInfo prop in props)
                {
                    if (prop.IsDefined(typeof(StorableAttribute), true))
                    {
                        StorableAttribute attribute = prop.GetCustomAttributes(typeof(StorableAttribute), true)[0] as StorableAttribute;
                        attribute.ensureName(prop.Name);
                        storables.Add(attribute);
                    }
                }
            }

            return storables;
        }

        /*
        [Obsolete("Use GetStorables instead.", true)]
        public static List<StorableFieldAttribute> GetStorableFields(Type type)
        {
            List<StorableFieldAttribute> storableFields = new List<StorableFieldAttribute>();
            {
                // Read all the custom attributes:
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo field in fields)
                {
                    if (field.IsDefined(typeof(StorableFieldAttribute), true))
                    {
                        StorableFieldAttribute attribute = field.GetCustomAttributes(typeof(StorableFieldAttribute), true)[0] as StorableFieldAttribute;
                        attribute.ensureName(field.Name);
                        storableFields.Add(attribute);
                    }
                }
            }
            return storableFields;
        }
        */


        /// <summary>
        /// Tries to make the IStorable type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static bool Register<TStorable>(IDbConnection connection, bool unregister, out String debug) where TStorable : IStorable, new()
        {
            if (unregister)
            {
                Unregister<TStorable>(connection, out debug);
            }

            return Register<TStorable>(connection, out debug);
        }
        public static bool Register<TStorable>(IDbConnection connection, out String debug) where TStorable : IStorable, new()
        {
            StoreType typeOfStore = (connection is System.Data.SqlClient.SqlConnection) ? StoreType.SQLServer : StoreType.MySQL;

            bool openClose = (connection.State == ConnectionState.Closed);

            TStorable t = new TStorable();

            List<String> columnStatements = new List<String>();

            List<meta.StorableAttribute> storableCollection = util.StorageUtil.GetStorables(t.GetType());

            // Get all defenitions:
            foreach (meta.StorableAttribute storableAttr in storableCollection)
            {
                columnStatements.Add(getColumnDef(storableAttr, typeOfStore));
            }

            // Get all indexes:
            foreach (meta.StorableAttribute storableAttr in storableCollection)
            {
                if (typeOfStore == StoreType.MySQL && (storableAttr.Info & StorableInfo.Index) == StorableInfo.Index)
                {
                    // Add index column for
                    columnStatements.Add(String.Format(" INDEX(`{0}`)", storableAttr.Name));
                }
            }


            if (openClose && connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            String createStatement = String.Format(@"CREATE TABLE IF NOT EXISTS `{0}`.`{1}` (
	{2}
)
ENGINE=MyISAM DEFAULT CHARSET=latin1 AUTO_INCREMENT=101;"
                , connection.Database
                , util.StorageUtil.GetStorableMeta(t.GetType()).Name
                , String.Join("\n\t, ", columnStatements.ToArray()));
            if (typeOfStore == StoreType.SQLServer)
            {
                createStatement = String.Format(@"
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

CREATE TABLE [{0}].[dbo].[{1}](
    {2}
) ON [PRIMARY];

SET ANSI_PADDING OFF;"
               , connection.Database
               , util.StorageUtil.GetStorableMeta(t.GetType()).Name
               , String.Join("\n\t, ", columnStatements.ToArray()));

            }

            debug = createStatement;

            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = createStatement;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    debug += "\n\n\n" + ex.Message;
                }
            }
            if (openClose && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            return false;
        }


        /// <summary>
        /// Tries to remove the schema for the IStorable type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static bool Unregister<TStorable>(IDbConnection connection, out String errorMessage) where TStorable : IStorable, new()
        {
            errorMessage = null;


            StoreType typeOfStore = (connection is System.Data.SqlClient.SqlConnection) ? StoreType.SQLServer : StoreType.MySQL;

            bool openClose = (connection.State == ConnectionState.Closed);

            TStorable t = new TStorable();

            if (openClose && connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            String createStatement = (typeOfStore == StoreType.SQLServer) ?
                String.Format(@"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[dbo].[{1}]') AND type in (N'U'))
DROP TABLE [{0}].[dbo].[{1}]", connection.Database, util.StorageUtil.GetStorableMeta(t.GetType()).Name)
                    :
                String.Format(@"DROP TABLE IF EXISTS `{0}`.`{1}`;", connection.Database, util.StorageUtil.GetStorableMeta(t.GetType()).Name);


            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = createStatement;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
            if (openClose && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            return errorMessage != null;
        }



        // internal static String getColumnDef(StorableFieldAttribute storableFieldAttribute, StoreType typeOfStore)
        internal static String getColumnDef(StorableAttribute storableAttribute, StoreType typeOfStore)
        {
            String columnDef = String.Format((typeOfStore == StoreType.SQLServer ? "[{0}]" : "`{0}`"), storableAttribute.Name);

            // Special:
            if (storableAttribute.DataType == StorableDataType.AutoIdentity)
            {
                columnDef += (typeOfStore == StoreType.SQLServer) ? " [bigint] IDENTITY(101,1) PRIMARY KEY" : " BIGINT auto_increment PRIMARY KEY";
            }
            else
            {
                // Add type pending on the StorablePropertyType
                // The order should be re-organized to begin with the most used types.
                switch (storableAttribute.DataType)
                {
                    case StorableDataType.Byte:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [tinyint]" : " TINYINT unsigned";
                        break;

                    case StorableDataType.Int32:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [int]" : " INT";
                        break;

                    case StorableDataType.Int64:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [bigint]" : " BIGINT";
                        break;


                    case StorableDataType.Single:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [float]" : " FLOAT";
                        break;

                    case StorableDataType.Double:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [float]" : " DOUBLE";
                        break;

                    case StorableDataType.DateTime:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [datetime]" : " datetime";
                        break;

                    // Variable or fixed length text field using the MaxLength property
                    case StorableDataType.Varchar:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [varchar]" : " varchar";
                        columnDef += (storableAttribute.MaxLength > 0) ? "(" + storableAttribute.MaxLength + ")" : "(50)";
                        break;

                    case StorableDataType.Char:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [char]" : " char";
                        columnDef += (storableAttribute.MaxLength > 0) ? "(" + storableAttribute.MaxLength + ")" : "(50)";
                        break;


                    // Handle char and varchar in batch, since the only diff is varchar, char and length?
                    case StorableDataType.BitField16:
                    case StorableDataType.BitField32:
                    case StorableDataType.Char16:
                    case StorableDataType.Char32:
                    case StorableDataType.Char64:
                    case StorableDataType.SHA1:
                    case StorableDataType.Varchar32:
                    case StorableDataType.Varchar64:
                    case StorableDataType.Varchar255:
                        string columnType = "[" + ((
                                storableAttribute.DataType == StorableDataType.Varchar32
                                || storableAttribute.DataType == StorableDataType.Varchar64
                                || storableAttribute.DataType == StorableDataType.Varchar255
                                ) ? "varchar" : "char") + "]";
                        if (typeOfStore == StoreType.MySQL)
                        {
                            columnType = columnType.Trim('[', ']');
                        }
                        int length = (
                                        storableAttribute.DataType == StorableDataType.Varchar32
                                        || storableAttribute.DataType == StorableDataType.Char32
                                        || storableAttribute.DataType == StorableDataType.BitField32
                                        ) ? 32 :
                                            (
                                                storableAttribute.DataType == StorableDataType.Varchar64
                                                || storableAttribute.DataType == StorableDataType.Char64
                                                ) ? 64 :
                                                    (
                                                        storableAttribute.DataType == StorableDataType.Varchar255
                                                        ) ? 255 :
                                                    (
                                                        storableAttribute.DataType == StorableDataType.SHA1
                                                        ) ? 40 : 16;

                        columnDef += String.Format(" {0}({1})", columnType, length);
                        break;

                    case StorableDataType.String:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [text]" : " TEXT";
                        break;

                    case StorableDataType.Text:
                    case StorableDataType.XML:
                        columnDef += (typeOfStore == StoreType.SQLServer) ? " [text]" : " MEDIUMTEXT";
                        break;

                    default:
                        columnDef += " UNDEFINED " + storableAttribute.DataType.ToString();
                        break;
                }

            }

            if ((storableAttribute.Info & StorableInfo.Optional) == StorableInfo.Optional || (storableAttribute.Info & StorableInfo.Nullable) == StorableInfo.Nullable)
            {
                columnDef += " NULL";
            }
            else
            {
                columnDef += " NOT NULL";
            }

            return columnDef;
        }


    }
}
