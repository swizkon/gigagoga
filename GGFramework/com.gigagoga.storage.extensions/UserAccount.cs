using System;
using System.Collections.Generic;
using System.Web;
using com.gigagoga.storage;
using com.gigagoga.storage.meta;
using System.Web.Security;
using System.Web.Configuration;

namespace com.gigagoga.storage.extensions
{
    /// <summary>
    /// Summary description for Account
    /// </summary>
    // [StorableObject(Name = "com_gigagoga_extensions_user_account", Capabilities = StorableObjectCapability.Default)]
    public abstract class UserAccount : StorableObject
    {
        [Storable(StorableDataType.Byte)]
        protected byte verified;

        [Storable("mail", StorableDataType.Varchar64, StorableInfo.Index)]
        protected string mail;

        [Storable("accountName", StorableDataType.Varchar32, StorableInfo.Optional)]
        protected string accountName;

        [Storable("nickName", StorableDataType.Varchar32, StorableInfo.Optional)]
        protected string nickName;

        [Storable("password", StorableDataType.SHA1, StorableInfo.Optional)]
        protected string password;

        protected string sessionKey;

        public String SessionKey
        {
            get { return sessionKey; }
            set { sessionKey = value; }
        }

        public String AccountName
        {
            get { return this.accountName; }
            set { this.accountName = value; }
        }

        public String NickName
        {
            get { return nickName; }
            set { nickName = value; }
        }

        public String Mail
        {
            get { return mail; }
            set { mail = value; }
        }

        public String Password
        {
            get { return password; }
            set
            {
                password = FormsAuthentication.HashPasswordForStoringInConfigFile(value, FormsAuthPasswordFormat.SHA1.ToString());
            }
        }

        public bool Verified
        {
            get { return this.verified == 1; }
            set { this.verified = (byte)( (value) ? 1 : 0); }
        }

        public UserAccount()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static T Login<T>(String accountName, String password, Store store, out String statusMessage)
            where T: IReadable ,new()
        {
            T loginAccount = new T();
            List<T> matches = store.Read<T>(new StoreQueryParam("accountName").Equals(accountName)
                , new StoreQueryParam("password").Equals(password), out statusMessage);
            if (matches.Count > 0)
            {
                loginAccount = matches[0];
            }
            return loginAccount;
        }

        public String GetCanonicalName()
        {
            return nickName ?? accountName ?? mail ?? "Anonymous";
        }

    }
}