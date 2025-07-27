using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Pages
{
    public class OrderViewModel : PageModel
    {
        private readonly IOrderService _ordsvc;

        public OrderViewModel(IOrderService ordsvc)
            => _ordsvc = ordsvc;

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = default!;

        [BindProperty]
        public Order Order { get; set; } = default!;

        public IEnumerable<OrderItem> LineItems => Order.Items;
        public decimal ItemsTotal   => Order.SubTotal    ?? 0m;
        public decimal CarrierCost  => Order.ShippingFee ?? 0m;
        public decimal Tax          => Order.Tax         ?? 0m;
        public decimal Total        => Order.Total       ?? (ItemsTotal + CarrierCost + Tax);

        public async Task<IActionResult> OnGetAsync(string id)
        {
            Id = id;

            var order = await _ordsvc.GetWebAppOrderAsync(Id);
            if (order == null)
                return Redirect($"{WebAppBase.Url}/order-is-null.html");

            Order = order;
            return Page();
        }
    }
}
//https://secure-endlessly-puma.ngrok-free.app/OrderView/gecG2Bbnuql8Ew