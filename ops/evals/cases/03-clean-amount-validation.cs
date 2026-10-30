// Review this submit validation helper.
// Question: is the validation itself defective?

public static class InvoiceValidators
{
    public static void EnsureSubmittable(decimal amount, string currency, string vendorId)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));
        }

        if (string.IsNullOrWhiteSpace(vendorId))
        {
            throw new ArgumentException("VendorId is required.", nameof(vendorId));
        }
    }
}
