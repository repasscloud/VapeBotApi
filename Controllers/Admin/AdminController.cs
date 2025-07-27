using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Models.Dto;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        private readonly IBotMessageStore _msg;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminService admin, IBotMessageStore msg, ILogger<AdminController> logger)
        {
            _admin = admin;
            _msg = msg;
            _logger = logger;
        }

        #region Category
        [HttpGet("~/api/admin/category")]
        public async Task<ActionResult<List<Category>>> GetAllCategory()
            => Ok(await _admin.GetAllCategoriesAsync());


        [HttpGet("~/api/admin/category/everything")]
        public async Task<ActionResult<List<Category>>> GetEverything()
            => Ok(await _admin.GetAllCategoriesWithProductsAsync());


        [HttpGet("~/api/admin/category/only")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategoriesOnly()
        {
            var list = await _admin.GetCategoriesOnlyAsync();
            if (list.Count == 0) return NotFound();
            return list;
        }

        [HttpGet("~/api/admin/category/nameonly/{id:int}")]
        public async Task<ActionResult<CategoryNameOnlyDto>> GetCategoryNameOnly(int id)
        {
            var dto = await _admin.GetCategoryNameOnlyAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("~/api/admin/category/{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var cat = await _admin.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return Ok(cat);
        }

        [HttpPost("~/api/admin/category")]
        public async Task<IActionResult> CreateCategory(Category cat)
        {
            var category = await _admin.CreateCategoryAsync(cat);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPut("~/api/admin/category/{id:int}")]
        public async Task<IActionResult> UpdateCategoryById(int id, Category cat)
        {
            if (id != cat.CategoryId) return BadRequest();
            if (!await _admin.UpdateCategoryAsync(cat)) return NotFound();
            return NoContent();
        }

        [HttpDelete("~/api/admin/category/{id:int}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            if (!await _admin.DeleteCategoryAsync(id)) return NotFound();
            return NoContent();
        }
        #endregion

        #region Product
        [HttpGet("~/api/admin/product")]
        public async Task<IActionResult> GetAllProduct() =>
            Ok(await _admin.GetAllProductsAsync());

        [HttpGet("~/api/admin/product/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var product = await _admin.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpGet("~/api/admin/product/bycatid/{id:int}")]
        public async Task<ActionResult<List<ProductDto>>> GetProductsByCategoryId(int id)
        {
            var prods = await _admin.GetProductsByCategoryIdAsync(id);
            if (prods.Count == 0) return NotFound();
            return Ok(prods);
        }

        [HttpPost("~/api/admin/product")]
        public async Task<IActionResult> CreateProductById(ProductCreateDto dto)
        {
            var product = await _admin.CreateProductAsync(dto);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPut("~/api/admin/product/{id}")]
        public async Task<IActionResult> UpdateProductById(string id, Product update)
        {
            if (id != update.ProductId)
                return BadRequest();

            var ok = await _admin.UpdateProductAsync(update);
            if (!ok)
                return NotFound();

            return Ok(true);
        }

        [HttpDelete("~/api/admin/product/{id}")]
        public async Task<IActionResult> DeleteProductById(string id)
        {
            var ok = await _admin.DeleteProductAsync(id);
            if (!ok)
                return NotFound();

            return Ok(true);
        }
        #endregion

        #region Order
        // [HttpGet("~/api/admin/order")]
        // public async Task<IActionResult> GetAllOrdersAsync() =>
        //     Ok(await _ordsvc.GetAllOrdersAsync());

        // [HttpGet("~/api/admin/order/{id}")]
        // public async Task<IActionResult> GetOrderByIdAsync(string id) =>
        //     Ok(await _ordsvc.GetOrderAsync(id));
        #endregion

        #region Shipping
        [HttpPost("~/api/admin/shippingquote")]
        public async Task<IActionResult> CreateShippingQuote(ShippingQuote quote)
        {
            var shippingQuote = await _admin.CreateShippingQuoteAsync(quote);
            if (shippingQuote == null)
                return NotFound();
            return Ok(shippingQuote);
        }

        [HttpPut("~/api/admin/shippingquote/{id:int}")]
        public async Task<IActionResult> UpdateShippingQuoteById(int id, ShippingQuote update)
        {
            if (id != update.Id)
                return BadRequest();

            var ok = await _admin.UpdateShippingQuoteAsync(update);
            if (!ok)
                return NotFound();

            return Ok(true);
        }

        [HttpDelete("~/api/admin/shippingquote/{id:int}")]
        public async Task<IActionResult> DeleteShippingQuoteById(int id)
        {
            var ok = await _admin.DeleteShippingQuoteAsync(id);
            if (!ok)
                return NotFound();

            return Ok(true);
        }

        [HttpGet("~/api/admin/shippingquote/{id:int}")]
        public async Task<ActionResult<ShippingQuote>> GetShippingQuoteById(int id)
        {
            var dto = await _admin.GetShippingQuoteByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
        #endregion

        [HttpGet("~/api/admin/log/{level:int}/{message}")]
        public ActionResult PostLog(int level, string message)
        {
            switch (level)
            {
                case 0:
                    _logger.LogDebug(message);
                    break;
                case 1:
                    _logger.LogTrace(message);
                    break;
                case 2:
                    _logger.LogInformation(message);
                    break;
                case 3:
                    _logger.LogWarning(message);
                    break;
                case 4:
                    _logger.LogError(message);
                    break;
                case 5:
                    _logger.LogCritical(message);
                    break;
                default:
                    _logger.LogDebug(message);
                    break;
            }
            return Ok();
        }


        // Task DeleteLastMessageAsync(long chatId);

        [HttpGet("~/api/admin/msgstore/add/{chatId:long}/{msgId:int}")]
        public async Task<ActionResult> AddMessageAsync(long chatId, int msgId)
        {
            await _msg.AddMessageAsync(chatId, msgId);
            return Ok();
        }

        [HttpGet("~/api/admin/msgstore/getlast/{chatId:long}")]
        public async Task<ActionResult<int>> GetLastMessageIdAsync(long chatId)
        {
            var msgId = await _msg.GetLastMessageIdAsync(chatId);
            return Ok(msgId);
        }

        [HttpGet("~/api/admin/msgstore/getall/{chatId:long}")]
        public async Task<ActionResult<List<int>>> GetAllCurrentMessageIdsAsync(long chatId)
        {
            var msgIds = await _msg.GetAllCurrentMessageIdsAsync(chatId);
            return Ok(msgIds);
        }

        [HttpGet("~/api/admin/msgstore/delete/{chatId:long}/{msgId:int}")]
        public async Task<ActionResult> DeleteMessageAsync(long chatId, int msgId)
        {
            await _msg.DeleteMessageAsync(chatId, msgId);
            return Ok();
        }

        [HttpGet("~/api/admin/msgstore/deletelast/{chatId:long}")]
        public async Task<ActionResult> DeleteLastMessageAsync(long chatId)
        {
            await _msg.DeleteLastMessageAsync(chatId);
            return Ok();
        }

        [HttpGet("~/api/admin/msgstore/clearhistory/{chatId:long}")]
        public async Task<ActionResult> ClearMessageHistoryByChatIdAsync(long chatId)
        {
            await _msg.ClearMessageHistoryByChatIdAsync(chatId);
            return Ok();
        }

        [HttpGet("~/api/admin/deleteaccount/{chatId:long}")]
        public async Task<ActionResult> DeleteAccountByChatIdAsync(long chatId)
        {
            var result = await _admin.DeleteAccountByChatIdAsync(chatId);
            return Ok(result);
        }

        [HttpPost("~/api/admin/customermsg")]
        public async Task<ActionResult<bool>> CreateContactSupportMsgAsync([FromBody] CustomerMessage cm)
        {
            var result = await _admin.CreateContactSupportMsgAsync(cm);
            return Ok(result);
        }
    }
}





