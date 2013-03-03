using System;
using System.Collections.Generic;
using System.Text;

namespace com.gigagoga.storage
{
    public class StoreQuery
    {
        private StoreType typeOfStore = StoreType.None;

        private Int32 offset = -1;
        private Int32 size = -1;
        private Type type = null;

        private String customQuery = null;

        private String selector = "*";
        private String query = null;
        private String sortBy = "id ASC";


        private StoreQuery(StoreType typeOfStore, Type typeOfObject)
        {
            this.typeOfStore = typeOfStore;
            this.type = typeOfObject;
        }

        public static StoreQuery QueryFor<TReadable>(Store store) where TReadable : IReadable, new()
        {
            StoreQuery storeQ = new StoreQuery(store.getTypeOfStore(), typeof(TReadable));
            // storeQ.type = typeof(T);
            return storeQ;
        }

        public static StoreQuery QueryFor<TReadable>(Store store, params StoreQueryParam[] matchAll) where TReadable : IReadable, new()
        {
            StoreQuery storeQ = new StoreQuery(store.getTypeOfStore(), typeof(TReadable));
            foreach (StoreQueryParam p in matchAll)
            {
                if (p != null)
                {
                    storeQ.And(p);
                }
            }
            return storeQ;
        }

        /// <summary>
        /// Append an AND select group where every match is added as an OR statement
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public StoreQuery And(params StoreQueryParam[] match)
        {
            if (query != null)
            {
                query += " AND (";
            }
            else
            {
                query = " (";
            }

            List<String> ors = new List<string>();
            foreach (StoreQueryParam p in match)
            {
                ors.Add(p.ToString());
            }

            query += String.Join(" OR ", ors.ToArray());

            query += ") ";

            return this;
        }

        /// <summary>
        /// Append an OR select group where every match is added as an AND statement
        /// </summary>
        public StoreQuery Or(params StoreQueryParam[] match)
        {
            if (query != null)
            {
                query += " OR (";
            }
            else
            {
                query = " (";
            }

            List<String> ors = new List<string>();
            foreach (StoreQueryParam p in match)
            {
                ors.Add(p.ToString());
            }

            query += String.Join(" AND ", ors.ToArray());

            query += ") ";

            return this;
        }



        public StoreQuery CustomQuery(String sqlOrXpathExpr)
        {
            this.customQuery = sqlOrXpathExpr;
            return this;
        }

        public StoreQuery SortBy(String sorting)
        {
            this.sortBy = sorting;
            return this;
        }

        public StoreQuery Range(int offset, int size)
        {
            this.offset = offset;
            this.size = size;
            return this;
        }

        /// <summary>
        /// Returns the SELECT statement.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.customQuery != null)
            {
                return this.customQuery;
            }

            if (this.type == null)
            {
                return null;
            }

            String tableName = util.StorageUtil.GetStorableMeta(this.type).Name ?? this.type.FullName.Replace('.', '_');

            // Make specials for MySQL and SQL Server...

            // Sele

            StringBuilder statementBuilder = new StringBuilder();
            statementBuilder.Append("SELECT ").Append(this.selector)
                            .Append(" FROM ").Append(tableName);

            if (this.query != null)
            {
                statementBuilder.Append(" WHERE ").Append(this.query);
            }

            if (this.sortBy != null)
            {
                statementBuilder.Append(" ORDER BY ").Append(this.sortBy);
            }

            // LIMIT
            if(this.offset >= 0 && this.size >= 0)
            {
                if(this.typeOfStore == StoreType.MySQL)
                {
                    statementBuilder.Append(" LIMIT ").Append(this.size)
                                    .Append(" OFFSET ").Append(this.offset); 
                }
                else if (this.typeOfStore == StoreType.SQLServer)
                {
                    statementBuilder.Replace("SELECT ", "SELECT TOP " + (this.offset + this.size) + " ROW_NUMBER() over (ORDER BY " + this.sortBy + ") as rowNumber, ");
                    // Wrap with ROW USE ROW_OVER
                    statementBuilder.Insert(0, "SELECT * FROM (");

                    // statementBuilder.AppendFormat(") resultSet WHERE rowNumber BETWEEN {0} AND {1} AND {2}", this.offset + 1, this.offset + this.size, this.query);
                    statementBuilder.AppendFormat(") resultSet WHERE rowNumber BETWEEN {0} AND {1}", this.offset + 1, this.offset + this.size);
                }
            }


            return statementBuilder.ToString();
        }

        public string ToCountString()
        {
            if (this.customQuery != null)
            {
                return this.customQuery;
            }
            String tableName = util.StorageUtil.GetStorableMeta(this.type).Name ?? this.type.FullName.Replace('.', '_');

            String stmt = "SELECT COUNT(*) as numberOfRows"; // FROM {0}  {1}";
            if (tableName != null)
            {
                stmt += " FROM " + tableName;
            }
            if (this.query != null)
            {
                stmt += " WHERE " + this.query;
            }
            return stmt;
            /*
            return String.Format(stmt, tableName, this.query);
            */
        }
    }
}