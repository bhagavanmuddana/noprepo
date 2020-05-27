using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.Stripe.Models;
using Nop.Plugin.Payments.Stripe.Services;
using Nop.Web.Framework.Components;
using Stripe;

namespace Nop.Plugin.Payments.Stripe.Components
{
    [ViewComponent(Name = "PaymentStripe")]
    public class PaymentStripeViewComponent : NopViewComponent
    {
        private readonly ServiceManager _serviceManager;

        #region Ctor

        public PaymentStripeViewComponent(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        #endregion

        public IViewComponentResult Invoke()
        {
            var (clientSecret, pKey) = _serviceManager.CreatePaymentIntent();
            if (!string.IsNullOrEmpty(clientSecret))
            {
                return View("~/Plugins/Payments.Stripe/Views/PaymentInfo.cshtml", new PaymentInfoModel() { PublishableKey = pKey, ClientSecret = clientSecret });
            }

            return View("~/Plugins/Payments.Stripe/Views/PaymentInfo.cshtml", new PaymentInfoModel());
        }
    }
}
