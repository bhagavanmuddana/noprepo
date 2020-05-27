using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Stripe.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
       public string PublishableKey { get; set; }

        public string ClientSecret { get; set; }
    }
}