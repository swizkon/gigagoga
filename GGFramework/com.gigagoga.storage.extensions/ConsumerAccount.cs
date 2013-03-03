using System;
using System.Collections.Generic;
using System.Web;
using com.gigagoga.storage;
using com.gigagoga.storage.meta;
using System.Web.Security;
using System.Web.Configuration;

namespace com.gigagoga.storage.extensions
{
    public abstract class ConsumerAccount : StorableObject
    {
        [Storable(StorableDataType.Int64)]
        protected long owner = 0;

        [Storable(StorableDataType.Varchar64)]
        protected string mail;

        [Storable(StorableDataType.Varchar32)]
        protected string apiKey;

        [Storable(StorableDataType.Varchar64)]
        protected string secretKey;

        public Int64 Owner
        {
            get { return this.owner; }
            set { this.owner = value; }
        }

        public String ApiKey
        {
            get { return this.apiKey; }
            set { this.apiKey = value; }
        }

        public String SecretKey
        {
            get { return secretKey; }
            set { secretKey = value; }
        }

        public String Mail
        {
            get { return mail; }
            set { mail = value; }
        }
    }
}
