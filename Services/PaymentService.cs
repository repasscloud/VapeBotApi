using Microsoft.EntityFrameworkCore;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Services.Interfaces;
using Stripe.Checkout;
using VapeBotApi.Models.Admin;
using Microsoft.Extensions.Options;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using VapeBotApi.Models.NowPaymentsIO;

namespace VapeBotApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly PayPalSettings _paypalSettings;
        private readonly NowPaymentsSettings _nowpaymentsSettings;
        private readonly IHttpClientFactory _http;

        public PaymentService(
            AppDbContext db,
            IOptions<PayPalSettings> paypal,
            IOptions<NowPaymentsSettings> nowpayments,
            IHttpClientFactory http)
        {
            _db = db;
            _paypalSettings = paypal.Value;
            _nowpaymentsSettings = nowpayments.Value;
            _http = http;
        }

        public async Task<string> GetStripePaymentLink(string orderId)
        {
            // retrieve the order
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order is null)
                return WebAppBase.NullOrderIdUrl;

            long shippingInCents = 0;
            if (order.ShippingFee is not null)
                shippingInCents = (long)(order.ShippingFee * 100);
            else
                return WebAppBase.NullOrderIdUrl; //"shippingInCents empty";

            long orderInCents = 0;
            if (order.SubTotal is not null)
                orderInCents = (long)(order.SubTotal * 100);
            else
                return WebAppBase.NullOrderIdUrl; //"orderInCents empty";

            long taxInCents = 0;
            if (order.Tax is not null)
                taxInCents = (long)(order.Tax * 100);
            else
                return WebAppBase.NullOrderIdUrl; //"taxInCents is empty";

            string shippingText = string.Empty;
            string shippingSubText = string.Empty;
            switch (order.Carrier)
            {
                case ShippingCarrier.AustPost:
                    shippingText = "AustPost ParcelPost";
                    shippingSubText = "3-5 days delivery";
                    break;
                case ShippingCarrier.ExpressPost:
                    shippingText = "AustPost ExpressPost";
                    shippingSubText = "next business day delivery";
                    break;
                default:
                    return WebAppBase.NullOrderIdUrl; //"incorrect shipping";
            }

            var sessionOpts = new SessionCreateOptions
            {
                SuccessUrl = WebAppBase.PaymentSuccessUrl,
                CancelUrl = WebAppBase.PaymentFailedUrl,
                Mode = "payment",

                Expand = new List<string> { "payment_intent" },

                // product
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = order.Currency,
                            UnitAmount  = orderInCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = "Purchase",
                                Description = $"Order: {order.OrderId}"
                            },
                        },
                        Quantity = 1,
                    },

                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency    = order.Currency,
                            UnitAmount  = shippingInCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = shippingText,
                                Description = shippingSubText,
                            },
                        },
                        Quantity = 1,
                    },

                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = order.Currency,
                            UnitAmount = taxInCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Tax",
                                Description = "GST (10%)",
                            },
                        },
                        Quantity = 1,
                    }
                },

                CustomerEmail = order.EmailAddress,
                Metadata = new Dictionary<string, string>
                {
                    ["userChatId"] = order.UserChatId.ToString(),
                    ["orderRef"] = order.OrderId
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOpts);

            if (session.Url is not null)
            {
                order.Status = OrderStatus.PaymentPending;
                order.StripePaymentIntentId = session.PaymentIntentId ?? "not available";
                order.StripePaymentUrl = session.Url;
                order.LastUpdated = DateTime.UtcNow;
                _db.Orders.Update(order);
                await _db.SaveChangesAsync();
                return session.Url;
            }
            else
            {
                return WebAppBase.StripePaymentGenerationErrorUrl;
            }
        }
        public async Task ProcessCheckoutSessionAsync(string paymentIntentId, string orderRef)
        {
            // retrieve the order
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderRef);

            if (order is null)
            {
                // log an error to admin
                return;
            }

            order.Status = OrderStatus.PaymentReceived;
            order.StripePaymentIntentId = paymentIntentId;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        public async Task<string?> GetPayPalInvoiceAsync(string customerEmail, string description, decimal amount)
        {
            var client = _http.CreateClient("PayPal");

            // 1. Get access token
            var byteArray = Encoding.ASCII.GetBytes($"{_paypalSettings.ClientID}:{_paypalSettings.Secret}");
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "client_credentials" }
            });

            var tokenResponse = await client.SendAsync(tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenObj = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenObj.GetProperty("access_token").GetString();

            if (accessToken is null)
                return null;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // 2. Create invoice draft
            var invoicePayload = new
            {
                detail = new
                {
                    currency_code = "AUD",
                    note = "Thank you for your purchase!",
                    terms_and_conditions = "Refunds available within 24 hours before shipping"
                },
                invoicer = new { },
                primary_recipients = new[]
                {
                    new
                    {
                        billing_info = new { email_address = customerEmail }
                    }
                },
                items = new[]
                {
                    new {
                        name = $"Order ID: {description}",
                        quantity = "1",
                        unit_amount = new { currency_code = "AUD", value = amount.ToString("F2") }
                    }
                }
            };

            var invoiceResp = await client.PostAsJsonAsync("/v2/invoicing/invoices", invoicePayload);
            if (!invoiceResp.IsSuccessStatusCode) return null;

            var invoiceJson = await invoiceResp.Content.ReadAsStringAsync();
            var invoiceData = JsonSerializer.Deserialize<JsonElement>(invoiceJson);

            var invoiceId = invoiceData.GetProperty("id").GetString();
            if (invoiceId is null) return null;

            var custOrder = await _db.Orders.Where(o => o.OrderId == description).FirstOrDefaultAsync();

            // 3. Send invoice (make it payable)
            var sendResp = await client.PostAsync($"/v2/invoicing/invoices/{invoiceId}/send", null);
            if (!sendResp.IsSuccessStatusCode) return null;

            // 4. Return invoice link
            if (custOrder != null)
            {
                custOrder.PayPalInvoiceId = invoiceId;
                custOrder.PayPalPaymentUrl = invoiceData.GetProperty("href").GetString() ?? $"https://www.sandbox.paypal.com/invoice/payerView/details/{invoiceId}";
                await _db.SaveChangesAsync();

                return custOrder.PayPalPaymentUrl;
            }
            return null;
        }
        public async Task<string> GetNowPaymentsIOPaymentLinkAsync(string orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order is null)
                return WebAppBase.NullOrderIdUrl;

            if (order.SubTotal is null || order.ShippingFee is null || order.Tax is null)
                return WebAppBase.NullOrderIdUrl;

            var total = order.SubTotal.Value + order.ShippingFee.Value + order.Tax.Value;

            var payload = new
            {
                price_amount = total,
                price_currency = order.Currency.ToLower(),
                order_id = order.OrderId,
                order_description = $"Order: {order.OrderId}",
                ipn_callback_url = $"{WebAppBase.Url}/api/Payments/nowpayments/ipn",
                success_url = WebAppBase.PaymentSuccessUrl,
                cancel_url = WebAppBase.PaymentFailedUrl,
                partially_paid_url = WebAppBase.PaymentFailedUrl,
                customer_email = order.EmailAddress,
                is_fixed_rate = true,
                is_fee_paid_by_user = true
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_nowpaymentsSettings.BaseUrl}/v1/invoice");
            request.Headers.Add("x-api-key", _nowpaymentsSettings.SecretKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _http.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return WebAppBase.NowPaymentsPaymentGenerationErrorUrl;

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<NowPaymentsInvoiceResponse>(stream);

            if (result?.invoice_url is null)
                return WebAppBase.NowPaymentsPaymentGenerationErrorUrl;

            var intent = new CryptoPaymentIntent
            {
                OrderId = order.OrderId,
                InvoiceId = result.id,
                TokenId = result.token_id,
                InvoiceUrl = result.invoice_url,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at
            };

            _db.CryptoPaymentIntents.Add(intent);
            await _db.SaveChangesAsync();

            return result.invoice_url;
        }




        public async Task<SupportedCurrenciesResponse?> GetNowPaymentsIOSupportedCurrenciesAsync()
        {
            if (string.IsNullOrWhiteSpace(_nowpaymentsSettings.SecretKey))
                throw new InvalidOperationException("NowPayments SecretKey is not configured.");

            var client = _http.CreateClient("NowPayments");
            if (client.BaseAddress == null)
                throw new InvalidOperationException("NowPayments client BaseAddress is not set.");
            var request = new HttpRequestMessage(HttpMethod.Get, "currencies?fixed_rate=true");
            request.Headers.Add("x-api-key", _nowpaymentsSettings.SecretKey);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return await JsonSerializer.DeserializeAsync<SupportedCurrenciesResponse>(stream, options);
        }
        public async Task<List<PaymentCurrencyInfo>> GetAllNowPaymentsIOCurrencyInfosAsync()
        {
            return await _db.PaymentCurrencyInfos.AsNoTracking().ToListAsync();
        }
        public async Task<PaymentCurrencyInfo?> GetNowPaymentsIOCurrencyInfoAsync(string currencyCodeFull)
        {
            return await _db.PaymentCurrencyInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CurrencyCodeFull == currencyCodeFull);
        }
        public async Task AddOrUpdateNowPaymentsIOCurrencyInfoAsync(PaymentCurrencyInfo info)
        {
            var existing = await _db.PaymentCurrencyInfos.FindAsync(info.CurrencyCodeFull);
            if (existing is null)
            {
                _db.PaymentCurrencyInfos.Add(info);
            }
            else
            {
                existing.CoinName     = info.CoinName;
                existing.CurrencyCode = info.CurrencyCode;
                existing.Network      = info.Network;
                existing.ImageUrl     = info.ImageUrl;
                _db.PaymentCurrencyInfos.Update(existing);
            }

            await _db.SaveChangesAsync();
        }
        public async Task<bool> DeleteNowPaymentsIOCurrencyInfoAsync(string currencyCodeFull)
        {
            var entity = await _db.PaymentCurrencyInfos.FindAsync(currencyCodeFull);
            if (entity is null)
                return false;

            _db.PaymentCurrencyInfos.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
