namespace VapeBotApi.Models.NowPaymentsIO;
public class NowPaymentsIpnPayload
{
    public string payment_id { get; set; } = string.Empty;
    public string payment_status { get; set; } = string.Empty;
    public string pay_address { get; set; } = string.Empty;
    public string price_amount { get; set; } = string.Empty;
    public string price_currency { get; set; } = string.Empty;
    public string pay_amount { get; set; } = string.Empty;
    public string actually_paid { get; set; } = string.Empty;
    public string pay_currency { get; set; } = string.Empty;
    public string order_id { get; set; } = string.Empty;
    public string purchase_id { get; set; } = string.Empty;
    public string outcome_amount { get; set; } = string.Empty;
    public string outcome_currency { get; set; } = string.Empty;
    public bool is_fixed_rate { get; set; }
    public bool is_fee_paid_by_user { get; set; }
    public string updated_at { get; set; } = string.Empty;
    public string created_at { get; set; } = string.Empty;
}