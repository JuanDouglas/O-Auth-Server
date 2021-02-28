using OAuth.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuth.Server.Models
{
    public class IPAdressResult 
    {
        /// <summary>
        /// IP Adress.
        /// </summary>
        public string Adress { get; set; }
        /// <summary>
        /// Confiance in this IP.
        /// </summary>
        public IPConfiance Confiance { get; set; }
        /// <summary>
        /// This IP already been banned.
        /// </summary>
        public bool AlreadyBeenBanned { get; set; }
        public IPAdressResult(IP iP)
        {
           
        }
    }
}