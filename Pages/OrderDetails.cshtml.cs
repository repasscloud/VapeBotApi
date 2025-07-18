// // Pages/OrderDetails.cshtml.cs
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using VapeBotApi.Models;
// using VapeBotApi.Services.Interfaces;

// namespace VapeBotApi.Pages
// {
//     public class OrderDetailsModel : PageModel
//     {
//         private readonly IOrderService _ordsvc;

//         public OrderDetailsModel(IOrderService ordsvc)
//             => _ordsvc = ordsvc;

//         [BindProperty(SupportsGet = true)]
//         public string Id { get; set; } = default!;

//         [BindProperty]
//         public Order Order { get; set; } = default!;

//         public IEnumerable<OrderItem> LineItems => Order.Items;
//         public decimal ItemsTotal   => Order.SubTotal    ?? 0m;
//         public decimal CarrierCost  => Order.ShippingFee ?? 0m;
//         public decimal Tax          => Order.Tax         ?? 0m;
//         public decimal Total        => Order.Total       ?? (ItemsTotal + CarrierCost + Tax);

//         public SelectList States { get; private set; } = default!;

//         public async Task<IActionResult> OnGetAsync()
//         {
//             Order = await _ordsvc.GetOrderAsync(Id);
//             PopulateStates();
//             return Page();
//         }

//         public async Task<IActionResult> OnPostAsync()
//         {
//             // no status‑check here: form is disabled client‑side when not editable
//             var success = await _ordsvc.UpdateShippingDetailsAsync(Order);
//             if (success)
//                 return Content("<script>window.Telegram.WebApp.close();</script>", "text/html");

//             // if we got here, re‑populate dropdown and redisplay
//             PopulateStates();
//             return Page();
//         }

//         private void PopulateStates()
//         {
//             // build a SelectList whose option values are the enum's numeric values
//             var items = Enum.GetValues<AUState>()
//                             .Cast<AUState>()
//                             .Select(s => new SelectListItem
//                             {
//                                 Value = ((int)s).ToString(),
//                                 Text  = s.ToString()
//                             })
//                             .ToList();

//             // if Order.State is null, fall back to NSW
//             var state = Order.State ?? AUState.NSW;
//             var selected = ((int)state).ToString();

//             States = new SelectList(items, "Value", "Text", selected);
//         }
//     }
// }
