using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payment.Paytm;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using paytm;

namespace Nop.Plugin.Payments.Paytm.Services
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
        private readonly PaytmPaymentSettings _paytmPaymentSettings;
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
            _paytmPaymentSettings = _settingService.LoadSetting<PaytmPaymentSettings>(storeScope);
        }

        #endregion

        #region Methods

        private bool IsConfigured(PaytmPaymentSettings _paytmPaymentSettings)
        {
            return !string.IsNullOrEmpty(_paytmPaymentSettings?.MId)
                && !string.IsNullOrEmpty(_paytmPaymentSettings?.MKey)
                && !string.IsNullOrEmpty(_paytmPaymentSettings?.WebSite)
                && !string.IsNullOrEmpty(_paytmPaymentSettings?.IndustryTypeId)
                && !string.IsNullOrEmpty(_paytmPaymentSettings?.ChannelId);
        }

        public (Dictionary<string, string>, string) CreateOrder(Guid orderGuid)
        {
            try
            {
                if (!IsConfigured(_paytmPaymentSettings))
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

                //creating checksum
                var paytmParams = new Dictionary<string, string>();
                paytmParams.Add("MID", _paytmPaymentSettings.MId);
                paytmParams.Add("WEBSITE", _paytmPaymentSettings.WebSite);
                paytmParams.Add("INDUSTRY_TYPE_ID", _paytmPaymentSettings.IndustryTypeId);
                paytmParams.Add("CHANNEL_ID", _paytmPaymentSettings.ChannelId);
                paytmParams.Add("ORDER_ID", orderGuid.ToString());
                paytmParams.Add("CUST_ID", _workContext.CurrentCustomer.CustomerGuid.ToString());
                paytmParams.Add("EMAIL", _workContext.CurrentCustomer.Email);
                paytmParams.Add("TXN_AMOUNT", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
                paytmParams.Add("CALLBACK_URL", $"{_webHelper.GetStoreLocation(false)}Plugins/PaymentPaytm/Return");
                var checksum = CheckSum.generateCheckSum(_paytmPaymentSettings.MKey, paytmParams);

                return (paytmParams, checksum);
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

                return (null, null);
            }
        }

        #endregion
    }
}