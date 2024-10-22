public class Transaction
{
    public required string TransactionId { get; set; }
    public DateOnly Date { get; set; }
    public string Type { get; set; } //convert to enum
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}
