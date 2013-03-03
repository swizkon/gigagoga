using System;
using System.Collections.Generic;
using System.Web;
using com.gigagoga.storage;
using com.gigagoga.storage.meta;
using System.Web.Security;
using System.Web.Configuration;
/*
namespace com.gigagoga.storage.extensions
{
    /// <summary>
    /// Summary description for Account
    /// </summary>
    [Obsolete("Use UserAccount instead", true)]
    [StorableObject(Name = "com_gigagoga_extensions_account", Capabilities=StorableObjectCapability.Default)]
    public class Account : StorableObject
    {
        [StorableField("mail", StorableFieldDataType.Varchar64, StorableFieldInfo.Index)]
        protected string mail;

        [StorableField("accountName", StorableFieldDataType.Varchar32, StorableFieldInfo.Optional)]
        protected string accountName;

        [StorableField("nickName", StorableFieldDataType.Varchar32, StorableFieldInfo.Optional)]
        protected string nickName;

        [StorableField("password", StorableFieldDataType.SHA1, StorableFieldInfo.Optional)]
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

        public Account()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static Account Login(String accountName, String password, Store store, out String statusMessage)
        {
            Account loginAccount = null;
            Account[] matches = store.Read<Account>(new StoreQueryParam("accountName").Equals(accountName)
                , new StoreQueryParam("password").Equals(password), out statusMessage);
            if (matches.Length > 0)
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
*/