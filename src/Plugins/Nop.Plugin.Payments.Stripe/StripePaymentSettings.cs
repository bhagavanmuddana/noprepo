
using Nop.Core.Configuration;

namespace Nop.Plugin.Payment.Stripe
{
    public class StripePaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PKey { get; set; }

        public string SKey { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}