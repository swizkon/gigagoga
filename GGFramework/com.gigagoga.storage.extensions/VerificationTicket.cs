using System;
using System.Collections.Generic;
using System.Web;
using com.gigagoga.storage;
using com.gigagoga.storage.meta;
using System.Web.Security;
using System.Web.Configuration;

namespace com.gigagoga.storage.extensions
{
    [StorableObject(Name = "com_gigagoga_extensions_verification_ticket", Capabilities = StorableObjectCapability.Default)]
    public class VerificationTicket : StorableObject
    {
        [Storable(StorableDataType.Int64)]
        protected long owner = 0;
        public Int64 Owner
        {
            get { return this.owner; }
            set { this.owner = value; }
        }



        [Storable(StorableDataType.Varchar32)]
        protected string action = "None";
        public String Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        [Storable(StorableDataType.Char16)]
        protected string ticket;
        public String Ticket
        {
            get { return this.ticket; }
            set { this.ticket = value; }
        }

        [Storable(StorableDataType.Varchar32)]
        protected string token;
        public String Token
        {
            get { return this.token; }
            set { this.token = value; }
        }

        [Storable(StorableDataType.Varchar64)]
        protected string host = "http://www.example.com/";
        public String Host
        {
            get { return this.host; }
            set { this.host = value; }
        }

        [Storable(StorableDataType.Varchar64)]
        protected string handler = "/verify";
        public String Handler
        {
            get { return this.handler; }
            set { this.handler = value; }
        }

        [Storable(StorableDataType.Varchar64)]
        protected string reciever = "";
        public String Reciever
        {
            get { return reciever; }
            set { reciever = value; }
        }

        public VerificationTicket()
        {
        }

        public VerificationTicket(bool initialize)
        {
            if (initialize)
            {
                this.ticket = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                this.token = Guid.NewGuid().ToString().Replace("-", "");
            }
        }

        /// <summary>
        /// Returns the 
        /// </summary>
        /// <returns></returns>
        public String GetLocation()
        {
            return this.host.TrimEnd('/')
                + '/' + this.handler.TrimStart('/')
                + '?'
                + "ticket=" + this.ticket
                + "&token=" + this.token;
        }
    }
}
