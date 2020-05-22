using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Paytm.Models
{
    public class CheckOut
    {
        public string Errors { get; set; }

        public string PaytmURL { get; set; }

        public Dictionary<string, string> PaytmParams { get; set; }

        public string CheckSum { get; set; }

        public string CustomerId { get; set; }
    }
}