using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.Payments.Stripe
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

        public static string SystemName => "Payments.Stripe";
    }
}