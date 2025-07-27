using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Models;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers.Public
{
    [ApiController]
    [Route("api/public/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _ordsvc;
        public OrderController(IOrderService orderService) => _ordsvc = orderService;

        #region create_order
        [HttpGet("~/order/current/{userChatId:long}")]
        public async Task<IActionResult> GetCurrentNewOrderFromChatIdAsync(long userChatId) =>
            Ok(await _ordsvc.GetCurrentNewOrderFromChatIdAsync(userChatId));

        [HttpGet("~/order/new/{userChatId:long}")]
        public async Task<IActionResult> GenerateNewOrderFromChatIdAsync(long userChatId) =>
            Ok(await _ordsvc.GenerateNewOrderFromChatIdAsync(userChatId));

        [HttpGet("~/category/list/dto")]
        public async Task<IActionResult> GetListCategoryDtoListAsync() =>
            Ok(await _ordsvc.GetListCategoryDtoListAsync());

        [HttpGet("~/category/list/products/fromcategoryid/{catId:int}")]
        public async Task<IActionResult> GetProductDtoListFromCategoryIdAsync(int categoryId) =>
            Ok(await _ordsvc.GetProductDtoListFromCategoryIdAsync(categoryId));

        [HttpGet("~/cart/add/{userChatId:long}/{productId}/{qty:int}")]
        public async Task<IActionResult> AddItemToCurrentNewOrderAsync(long userChatId, string productId, int qty) =>
            Ok(await _ordsvc.AddItemToCurrentNewOrderAsync(userChatId, productId, qty));

        [HttpGet("~/cart/sub/{userChatId:long}/{productId}/{qty:int}")]
        public async Task<IActionResult> RemoveItemFromCurrentNewOrderAsync(long userChatId, string productId, int qty) =>
            Ok(await _ordsvc.RemoveItemFromCurrentNewOrderAsync(userChatId, productId, qty));

        [HttpGet("~/cart/empty/{userChatId:long}")]
        public async Task<IActionResult> EmptyCurrentNewOrderAsync(long userChatId) =>
            Ok(await _ordsvc.EmptyCurrentNewOrderAsync(userChatId));
        #endregion create_order

        #region show_cart
        [HttpGet("~/cart/show/{userChatId:long}")]
        public async Task<IActionResult> ShowCurrentNewOrderItemsAsync(long userChatId) =>
            Ok(await _ordsvc.ShowCurrentNewOrderItemsAsync(userChatId));
        #endregion show_cart

        #region checkout
        [HttpGet("~/order/checkout/request/{userChatId:long}")]
        public async Task<IActionResult> RequestCheckoutAsync(long userChatId) =>
            Ok(await _ordsvc.RequestCheckoutAsync(userChatId));
            
        [HttpGet("~/order/checkout/shipping/options/{userChatId:long}")]
        public async Task<IActionResult> GetShippingOptionsAsync(long userChatId) =>
            Ok(await _ordsvc.GetShippingOptionsAsync(userChatId));

        [HttpGet("~/order/checkout/shipping/set/{userChatId:long}/{carrier}")]
        public async Task<IActionResult> SetShippingCarrierAsync(long userChatId, string carrier) =>
            Ok(await _ordsvc.SetShippingCarrierAsync(userChatId, carrier));

        [HttpGet("~/order/checkout/payment/set/{userChatId:long}/{method}")]
        public async Task<IActionResult> SetPaymentMethodAsync(long userChatId, string method) =>
            Ok(await _ordsvc.SetPaymentMethodAsync(userChatId, method));

        [HttpGet("~/order/tracking/{userChatId:long}")]
        public async Task<IActionResult> GetOrdersTrackingAsync(long userChatId) =>
            Ok(await _ordsvc.GetOrdersTrackingAsync(userChatId));

        [HttpGet("~/order/history/{userChatId:long}")]
        public async Task<IActionResult> GetOrdersHistoryAsync(long userChatId) =>
            Ok(await _ordsvc.GetOrdersHistoryAsync(userChatId));

        [HttpGet("~/order/cancel/{orderId}")]
        public async Task<IActionResult> CancelOrderAsync(string orderId) =>
            Ok(await _ordsvc.CancelOrderAsync(orderId));
        #endregion checkout

        #region webapp
        [HttpGet("~/order/checkout/finalize")]
        public async Task<IActionResult> FinalizeWebAppOrderAsync([FromBody] Order order) =>
            Ok(await _ordsvc.FinalizeWebAppOrderAsync(order));
        #endregion
    }
}
