// Review this logging added around approval.
// Question: is this an acceptable audit log?

public void LogApproval(Invoice invoice, string actor)
{
    _logger.LogInformation(
        "Approved invoice {InvoiceId} actor={Actor} vendor={Vendor} iban={Iban} amount={Amount}",
        invoice.Id,
        actor,
        invoice.VendorName,
        invoice.VendorBankAccount.Iban,
        invoice.Amount);
}
