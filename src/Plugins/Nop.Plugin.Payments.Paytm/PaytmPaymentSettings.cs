using Nop.Core.Configuration;

namespace Nop.Plugin.Payment.Paytm
{
    public class PaytmPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string MId { get; set; }

        public string MKey { get; set; }

        public string WebSite { get; set; }

        public string IndustryTypeId { get; set; }

        public string ChannelId { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}