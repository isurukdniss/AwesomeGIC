public class Account
{
    public string AccountId { get; set; }
    public List<Transaction>? Transactions { get; set; }
    public decimal Balance { get; set; }
}
