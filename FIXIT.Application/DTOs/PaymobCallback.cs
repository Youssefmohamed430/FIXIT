
namespace FIXIT.Application.DTOs;

public class PaymobCallback
{
    public string? type { get; set; }
    public PaymobObj? obj { get; set; }
}

public class PaymobObj
{
    public long? id { get; set; }
    public bool? pending { get; set; }
    public int? amount_cents { get; set; }
    public bool? success { get; set; }
    public bool? is_auth { get; set; }
    public bool? is_capture { get; set; }
    public bool? is_standalone_payment { get; set; }
    public bool? is_voided { get; set; }
    public bool? is_refunded { get; set; }
    public bool? is_3d_secure { get; set; }

    public int? integration_id { get; set; }
    public int? profile_id { get; set; }
    public bool? has_parent_transaction { get; set; }

    public PaymobOrder? order { get; set; }

    public string? created_at { get; set; }
    public string? currency { get; set; }

    public PaymobSourceData? source_data { get; set; }

    public string? api_source { get; set; }
    public int? terminal_id { get; set; }

    public int? merchant_commission { get; set; }
    public int? accept_fees { get; set; }

    public PaymobData? data { get; set; }

    public PaymobPaymentKeyClaims? payment_key_claims { get; set; }

    public bool? error_occured { get; set; }
    public bool? is_live { get; set; }

    public int? refunded_amount_cents { get; set; }
    public int? captured_amount { get; set; }

    public string? updated_at { get; set; }
    public bool? is_settled { get; set; }
    public bool? is_bill { get; set; }

    public int? owner { get; set; }
}

public class PaymobOrder
{
    public int? id { get; set; }
    public string? created_at { get; set; }
    public bool? delivery_needed { get; set; }

    public PaymobMerchant? merchant { get; set; }
    public PaymobShippingData? shipping_data { get; set; }

    public string? currency { get; set; }
    public int? amount_cents { get; set; }

    public List<object>? items { get; set; }
}

public class PaymobMerchant
{
    public int? id { get; set; }
    public string? created_at { get; set; }
    public List<string>? phones { get; set; }
    public List<string>? company_emails { get; set; }
    public string? company_name { get; set; }
    public string? country { get; set; }
    public string? city { get; set; }
}

public class PaymobShippingData
{
    public int? id { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? street { get; set; }
    public string? building { get; set; }
    public string? floor { get; set; }
    public string? apartment { get; set; }
    public string? city { get; set; }
    public string? country { get; set; }
    public string? email { get; set; }
    public string? phone_number { get; set; }
}

public class PaymobSourceData
{
    public string? type { get; set; }
    public string? pan { get; set; }
    public string? sub_type { get; set; }
}

public class PaymobData
{
    public string? message { get; set; }
    public string? txn_response_code { get; set; }
    public string? acq_response_code { get; set; }
}

public class PaymobPaymentKeyClaims
{
    public int? user_id { get; set; }
    public int? amount_cents { get; set; }
    public string? currency { get; set; }
    public int? integration_id { get; set; }
    public int? order_id { get; set; }

    public PaymobBillingData? billing_data { get; set; }
}

public class PaymobBillingData
{
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? apartment { get; set; }
    public string? email { get; set; }
}
