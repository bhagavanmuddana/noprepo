using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Paytm.Models
{
    public class CheckOutResponse
    {
        public string MID { get; set; }

        public string TXNID { get; set; }

        public Guid ORDERID { get; set; }

        public string BANKTXNID { get; set; }

        public string TXNAMOUNT { get; set; }

        public string CURRENCY { get; set; }

        public string STATUS { get; set; }

        public string RESPCODE { get; set; }

        public string RESPMSG { get; set; }

        public DateTime TXNDATE { get; set; }

        public string GATEWAYNAME { get; set; }

        public string BANKNAME { get; set; }

        public string PAYMENTMODE { get; set; }

        public string CHECKSUMHASH { get; set; }
    }
}