namespace VapeBotApi.Models.NowPaymentsIO;
public class NowPaymentsInvoiceResponse
{
    public string id { get; set; }
    public string token_id { get; set; }
    public string order_id { get; set; }
    public string order_description { get; set; }
    public string price_amount { get; set; }
    public string price_currency { get; set; }
    public object pay_currency { get; set; }
    public string ipn_callback_url { get; set; }
    public string invoice_url { get; set; }
    public string success_url { get; set; }
    public string cancel_url { get; set; }
    public object customer_email { get; set; }
    public object partially_paid_url { get; set; }
    public object payout_currency { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public bool is_fixed_rate { get; set; }
    public bool is_fee_paid_by_user { get; set; }
    public object source { get; set; }
    public bool collect_user_data { get; set; }
}