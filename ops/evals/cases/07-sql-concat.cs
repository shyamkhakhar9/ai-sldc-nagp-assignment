// Review this lookup added for the month-end support screen.
// Question: is this safe?

public async Task<Invoice?> FindByVendorAsync(string vendorName)
{
    var sql = "SELECT * FROM Invoices WHERE VendorName = '" + vendorName + "'";
    return await _db.Invoices.FromSqlRaw(sql).FirstOrDefaultAsync();
}
