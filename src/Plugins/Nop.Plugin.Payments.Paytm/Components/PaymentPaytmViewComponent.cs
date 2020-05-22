using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payment.Paytm;
using Nop.Plugin.Payments.Paytm.Models;
using Nop.Plugin.Payments.Paytm.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using paytm;

namespace Nop.Plugin.Payments.Paytm.Components
{
    [ViewComponent(Name = "PaymentPaytm")]
    public class PaymentPaytmViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IPaymentService _paymentService;
        private readonly ServiceManager _serviceManager;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PaymentPaytmViewComponent(
            IPaymentService paymentService,
            ServiceManager serviceManager,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _paymentService = paymentService;
            _serviceManager = serviceManager;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        #endregion

        public IViewComponentResult Invoke()
        {
            var model = new CheckOut();
            var paymentRequest = new ProcessPaymentRequest();
            _paymentService.GenerateOrderGuid(paymentRequest);
            paymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
            var (paytmParams, checksum) = _serviceManager.CreateOrder(paymentRequest.OrderGuid);
            if (!string.IsNullOrEmpty(checksum))
            {
                paytmParams.Add("CHECKSUMHASH", checksum);

                model.PaytmURL = Defaults.ServiceRedirectUrl;
                model.CheckSum = checksum;
                model.PaytmParams = paytmParams;

                //save order details for future using
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.PayPalSmartPaymentButtons.OrderId"), paymentRequest.OrderGuid);
            }
            else
            {
                model.Errors = "Unable process the request, Please try again later";
            }

            HttpContext.Session.Set(Defaults.PaymentRequestSessionKey, paymentRequest);
            return View("~/Plugins/Payments.Paytm/Views/PaymentInfo.cshtml", model);
        }
    }
}
