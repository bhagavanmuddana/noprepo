using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payment.Paytm;
using Nop.Plugin.Payments.Paytm.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using paytm;

namespace Nop.Plugin.Payments.Paytm.Controllers
{
    public class PaymentPaytmController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly PaytmPaymentSettings _paytmPaymentSettings;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public PaymentPaytmController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            OrderSettings orderSettings,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            ILogger logger,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _logger = logger;
            _webHelper = webHelper;
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            _paytmPaymentSettings = _settingService.LoadSetting<PaytmPaymentSettings>(storeScope);
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var paytmPaymentSettings = _settingService.LoadSetting<PaytmPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = paytmPaymentSettings.UseSandbox,
                MId = paytmPaymentSettings.MId,
                MKey = paytmPaymentSettings.MKey,
                WebSite = paytmPaymentSettings.WebSite,
                IndustryTypeId = paytmPaymentSettings.IndustryTypeId,
                ChannelId = paytmPaymentSettings.ChannelId,
                AdditionalFee = paytmPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = paytmPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            return View("~/Plugins/Payments.Paytm/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _paytmPaymentSettings.UseSandbox = model.UseSandbox;
            _paytmPaymentSettings.MId = model.MId;
            _paytmPaymentSettings.MKey = model.MKey;
            _paytmPaymentSettings.WebSite = model.WebSite;
            _paytmPaymentSettings.IndustryTypeId = model.IndustryTypeId;
            _paytmPaymentSettings.ChannelId = model.ChannelId;
            _settingService.SaveSetting(_paytmPaymentSettings);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        protected virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        public ActionResult Return(CheckOutResponse response)
        {
            if (_orderSettings.CheckoutDisabled)
                throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

            var paymentDetails = HttpContext.Session.Get<ProcessPaymentRequest>(Defaults.PaymentRequestSessionKey);
            _workContext.CurrentCustomer.Id = paymentDetails.CustomerId;
            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            //prevent 2 orders being placed within an X seconds time frame
            if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

            //checking payment success
            if (response.RESPCODE == "01" && response.STATUS == "TXN_SUCCESS" && IsValidTransaction(response.ORDERID.ToString(), response.TXNAMOUNT))
            {
                //place order
                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                processPaymentRequest = processPaymentRequest ?? new ProcessPaymentRequest();
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }
            }

            return RedirectToRoute("OrderDetails", new { orderId = response.ORDERID });
        }

        private bool IsValidTransaction(string orderId, string amount)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("MID", _paytmPaymentSettings.MId);
            parameters.Add("ORDERID", orderId);

            //getting checksum of payment
            string checksum = CheckSum.generateCheckSum(_paytmPaymentSettings.MKey, parameters);
            try
            {
                string postData = "{\"MID\":\"" + _paytmPaymentSettings.MId + "\",\"ORDERID\":\"" + orderId + "\",\"CHECKSUMHASH\":\"" + System.Net.WebUtility.UrlEncode(checksum) + "\"}";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Defaults.ServiceStatusUrl);
                webRequest.Method = "POST";
                webRequest.Accept = "application/json";
                webRequest.ContentType = "application/json";

                using (var requestWriter2 = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter2.Write("JsonData=" + postData);
                }
                string responseData = string.Empty;
                using (var responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    responseData = responseReader.ReadToEnd();
                    return responseData.Contains("TXN_SUCCESS") && responseData.Contains(amount);
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        #endregion
    }
}