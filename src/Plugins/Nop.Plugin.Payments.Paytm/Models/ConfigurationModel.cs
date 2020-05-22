using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Paytm.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.MId")]
        public string MId { get; set; }

        public bool MId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.MKey")]
        public string MKey { get; set; }

        public bool MKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.WebSite")]
        public string WebSite { get; set; }

        public bool WebSite_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.IndustryTypeId")]
        public string IndustryTypeId { get; set; }

        public bool IndustryTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.ChannelId")]
        public string ChannelId { get; set; }

        public bool ChannelId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paytm.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}