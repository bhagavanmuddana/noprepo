using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.Payments.Paytm
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class Defaults
    {
        /// <summary>
        /// Gets the session key to get process payment request
        /// </summary>
        public static string PaymentRequestSessionKey => "OrderPaymentInfo";

        /// <summary>
        /// Gets the service js script URL
        /// </summary>
        public static string ServiceRedirectUrl => "https://securegw-stage.paytm.in/order/process";


        public static string ServiceStatusUrl = "https://securegw-stage.paytm.in/order/status";

        public static string SystemName => "Payments.Paytm";
    }
}