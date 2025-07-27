namespace VapeBotApi.Models.Admin;

public class NowPaymentsSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string IpnKey { get; set; } = string.Empty;
}
