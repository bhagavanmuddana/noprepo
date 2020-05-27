using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Paytm.Controllers
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //Return
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Stripe.Return", "Plugins/PaymentStripe/Return", new { controller = "PaymentStripe", action = "Return" });
        }

        public int Priority
        {
            get { return -1; }
        }
    }
}