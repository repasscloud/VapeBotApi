using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers.Public
{
    [ApiController]
    [Route("api/public/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _ordsvc;
        public OrderController(IOrderService orderService) => _ordsvc = orderService;

        [HttpGet("~/order/new/{userChatId:long}")]
        public async Task<IActionResult> CreateNewOrderAsync(long userChatId) =>
            Ok(await _ordsvc.CreateOrderGetIdAsync(userChatId));

        [HttpGet("~/cart/add/{userChatId:long}/{productId}/{qty:int}")]
        public async Task<IActionResult> AddToCartAsync(long userChatId, string productId, int qty) =>
            Ok(await _ordsvc.AddToCartAsync(userChatId, productId, qty));

        [HttpGet("~/cart/sub/{userChatId:long}/{productId}/{qty:int}")]
        public async Task<IActionResult> SubtractFromCartAsync(long userChatId, string productId, int qty) =>
            Ok(await _ordsvc.AddToCartAsync(userChatId, productId, qty));

        [HttpGet("~/cart/empty/{userChatId:long}")]
        public async Task<IActionResult> EmptyCartAsync(long userChatId) =>
            Ok(await _ordsvc.EmptyCartAsync(userChatId));

        [HttpGet("~/order/get/all/{userChatId:long}")]
        public async Task<IActionResult> GetAllOrdersAsync(long userChatId) =>
            Ok(await _ordsvc.GetUserOrdersAsync(userChatId));

        [HttpGet("~/order/get/byorderid/{orderId}")]
        public async Task<IActionResult> GetOrderByIdAsync(string orderId) =>
            Ok(await _ordsvc.GetOrderAsync(orderId));

        [HttpGet("~/order/requestcheckout/{userChatId:long}")]
        public async Task<IActionResult> GetOrderRequestCheckoutAsync(long userChatId) =>
            Ok(await _ordsvc.RequestCheckoutAsync(userChatId));

        [HttpGet("~/order/shipping/calculate/{userChatId:long}")]
        public async Task<IActionResult> GetShippingOptionsAsync(long userChatId) =>
            Ok(await _ordsvc.GetShippingOptionsAsync(userChatId));

        [HttpGet("~/order/setshippingcarrier/{userChatId:long}/{shippingCarrier}")]
        public async Task<IActionResult> SetShippingCarrierAsync(long userChatId, string shippingCarrier) =>
            Ok(await _ordsvc.SetShippingCarrierAsync(userChatId, shippingCarrier));

        [HttpGet("~/order/setpaymentmethod/{userChatId:long}/{paymentMethod}")]
        public async Task<IActionResult> SetPaymentMethodAsync(long userChatId, string paymentMethod) =>
            Ok(await _ordsvc.SetPaymentMethodAsync(userChatId, paymentMethod));

        [HttpGet("~/order/hasaccount/{userChatId:long}")]
        public async Task<IActionResult> GetAccountLinkAsync(long userChatId)
        {
            var link = await _ordsvc.GetAccountLinkAsync(userChatId);

            if (link is null)
                return NoContent();    // or NotFound() if you prefer

            return Ok(link);    
        }
    }
}
