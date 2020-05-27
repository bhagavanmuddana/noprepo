using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payment.Stripe;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Stripe;

namespace Nop.Plugin.Payments.Stripe.Services
{
    /// <summary>
    /// Represents the plugin service manager
    /// </summary>
    public partial class ServiceManager
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly StripePaymentSettings _stripePaymentSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly CurrencySettings _currencySettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public ServiceManager(
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            ISettingService settingService,
            IWebHelper webHelper,
            ICurrencyService currencyService,
            IShoppingCartService shoppingCartService,
            CurrencySettings currencySettings,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _currencyService = currencyService;
            _shoppingCartService = shoppingCartService;
            _currencySettings = currencySettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            _stripePaymentSettings = _settingService.LoadSetting<StripePaymentSettings>(storeScope);
        }

        #endregion

        #region Methods

        private bool IsConfigured(StripePaymentSettings _paytmPaymentSettings)
        {
            return !string.IsNullOrEmpty(_paytmPaymentSettings?.PKey)
                && !string.IsNullOrEmpty(_paytmPaymentSettings?.SKey);
        }

        public (string, string) CreatePaymentIntent()
        {
            try
            {
                if (!IsConfigured(_stripePaymentSettings))
                {
                    _logger.Error("Paytm not configured", null, _workContext.CurrentCustomer);
                }

                var currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)?.CurrencyCode;
                if (string.IsNullOrEmpty(currency))
                    throw new NopException("Primary store currency not set");

                //prepare purchase unit details
                var shoppingCart = _shoppingCartService
                    .GetShoppingCart(_workContext.CurrentCustomer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id)
                    .ToList();
                var orderTotal = Math.Round(_orderTotalCalculationService.GetShoppingCartTotal(shoppingCart, usePaymentMethodAdditionalFee: false) ?? decimal.Zero, 2);
                //var totalinINR =
                //    _currencyService.ConvertCurrency(orderTotal, "INR");

                StripeConfiguration.ApiKey = "sk_test_5HF9bM2OzA9Q26m8MYsAEUQN008rNH4EMZ";
                var options = new PaymentIntentCreateOptions
                {
                    Amount = Convert.ToInt64(orderTotal),
                    Currency = "inr",
                    Metadata = new Dictionary<string, string> { { "integration_check", "accept_a_payment" } },
                };

                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);
                if (paymentIntent != null && !string.IsNullOrEmpty(paymentIntent.ClientSecret))
                {
                    return (paymentIntent.ClientSecret, _stripePaymentSettings.PKey);
                }
            }
            catch(Exception exception)
            {
                //get a short error message
                var message = exception.Message;
                var detailedException = exception;
                do
                {
                    detailedException = detailedException.InnerException;
                } while (detailedException?.InnerException != null);

                //log errors
                var logMessage = $"{Defaults.SystemName} error: {System.Environment.NewLine}{message}";
                _logger.Error(logMessage, exception, _workContext.CurrentCustomer);
            }

            return (null, null);
        }

        #endregion
    }
}