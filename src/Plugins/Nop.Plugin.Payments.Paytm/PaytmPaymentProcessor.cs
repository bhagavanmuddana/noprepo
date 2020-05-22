using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Payment.Paytm;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.Paytm
{
    /// <summary>
    /// PayPalStandard payment processor
    /// </summary>
    public class PaytmPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly PaytmPaymentSettings _paytmPaymentSettings;

        #endregion

        #region Ctor

        public PaytmPaymentProcessor(
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            PaytmPaymentSettings paytmPaymentSettings)
        {
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _paytmPaymentSettings = paytmPaymentSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new PaytmPaymentSettings
            {
                UseSandbox = true
            });

            //locales
                _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
                {
                    ["Plugins.Payments.Paytm.Instructions"] = @"
                        <p>
	                        <b>If you're using this gateway ensure that your primary store currency is supported by Paytm.</b>
	                        <br />
	                        <br />To use paytm payments, you must generate a production api key from paytm marchent account profile. Follow these steps to configure your account for PDT:<br />
	                        <br />1. Log in to your Paytm marchent account (click <a href=""https://developer.paytm.com/docs/"" target=""_blank"">here</a> to create your account).
                            <br />2. Go to Activate Account from left nav and activate your account.
	                        <br />2. After successfull activation, Go to API Key -> Production API Details.
	                        <br />3. Generate a new production API.
                            <br />8. Under Payment Data Transfer, click the On radio button and get your PDT identity token.
	                        <br />9. Click Save.
	                        <br />
                        </p>",

                    ["Plugins.Payments.Paytm.Fields.UseSandbox"] = "Use Sandbox",
                    ["Plugins.Payments.Paytm.Fields.UseSandbox.Hint"] = "Check to enable Sandbox (testing environment).",
                    ["Plugins.Payments.Paytm.Fields.MId"] = "MID",
                    ["Plugins.Payments.Paytm.Fields.MId.Hint"] = "Enter marchent id genrated in paytm",
                    ["Plugins.Payments.Paytm.Fields.MKey"] = "MKey",
                    ["Plugins.Payments.Paytm.Fields.MKey.Hint"] = "Enter Marchent secret key generated in paytm",
                    ["Plugins.Payments.Paytm.Fields.WebSite"] = "Website",
                    ["Plugins.Payments.Paytm.Fields.WebSite.Hint"] = "Enter webSite specified in paytm",
                    ["Plugins.Payments.Paytm.Fields.IndustryTypeId"] = "IndustryTypeId",
                    ["Plugins.Payments.Paytm.Fields.IndustryTypeId.Hint"] = "Enter industryTypeId generated in paytm",
                    ["Plugins.Payments.Paytm.Fields.ChannelId"] = "ChannelId",
                    ["Plugins.Payments.Paytm.Fields.ChannelId.Hint"] = "Enter channelId generated in paytm",
                    ["Plugins.Payments.Paytm.Fields.AdditionalFee"] = "AdditionalFee",
                    ["Plugins.Payments.Paytm.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                    ["Plugins.Payments.Paytm.Fields.AdditionalFeePercentage"] = "Additional Fee Percentage",
                    ["Plugins.Payments.Paytm.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                    ["Plugins.Payments.Paytm.PaymentMethodDescription"] = "You will be redirected to Paytm site to complete the payment",
                    ["Plugins.Payments.Paytm.Fields.RedirectionTip"] = "You will be redirected to PayPal site to complete the order.",
                });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PaytmPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.Paytm");

            base.Uninstall();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPaytm/Configure";
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                 _paytmPaymentSettings.AdditionalFee, _paytmPaymentSettings.AdditionalFeePercentage);
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentPaytm";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.Paytm.PaymentMethodDescription");

        #endregion
    }
}