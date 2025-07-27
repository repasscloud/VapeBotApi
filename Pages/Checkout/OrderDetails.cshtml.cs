using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Pages.Checkout
{
    public class OrderDetailsModel : PageModel
    {
        private readonly IOrderService _ordsvc;

        public OrderDetailsModel(IOrderService ordsvc)
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

        public SelectList States { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch into a local variable
            var order = await _ordsvc.GetWebAppOrderAsync(Id);

            // If it's null, bail out immediately
            if (order == null)
                return Redirect($"{WebAppBase.Url}/order-is-null.html");

            // Now we know it's non-null, so it's safe to assign
            Order = order;

            PopulateStates();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // dump entire Order to JSON for debugging
            var json = JsonSerializer.Serialize(Order, new JsonSerializerOptions { WriteIndented = true });
            var debugPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "order_debug.json");
            await System.IO.File.WriteAllTextAsync(debugPath, json);

            string paymentUrl = await _ordsvc.FinalizeWebAppOrderAsync(Order);

            // always redirect back to successUrl (there is ALWAYS a success url)
            return Redirect(paymentUrl);
        }

        private void PopulateStates()
        {
            var items = Enum.GetValues<AUState>()
                            .Cast<AUState>()
                            .Select(s => new SelectListItem
                            {
                                Value = ((int)s).ToString(),
                                Text  = s.ToString()
                            })
                            .ToList();

            var state = Order.State ?? AUState.NSW;
            States = new SelectList(items, "Value", "Text", ((int)state).ToString());
        }
    }
}
