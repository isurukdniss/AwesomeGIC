using AwesomeGIC.Services;
using System.Security.Principal;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        GICService gicService = new GICService();

        while (true)
        {
            Console.WriteLine("[T] Input Transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");

            string? userInput = Console.ReadLine();

            if (userInput == null)
            {
                Console.WriteLine("Invalid input, please try again ...");
                continue;
            }

            switch (userInput.ToLower())
            {
                case "t":
                    gicService.InputTransaction(gicService.Accounts);
                    break;
                case "i":
                    gicService.DefineInterestRule(gicService.Rules);
                    break;
                case "p":
                    gicService.PrintStatement(gicService.Accounts, gicService.Rules);
                    break;
                case "q":
                    gicService.Quit();
                    break;
                default:
                    Console.WriteLine("Invalid input, please try again");
                    break;
            }
        }
    }

    //TODO remove
    private static void SeedData(List<Account> accounts, List<InterestRule> rules)
    {
        accounts.Add(new Account { 
            AccountId = "ACC01",
            Balance = 130m,
            Transactions = new List<Transaction>
            {
                new Transaction
                {
                    TransactionId = "20230601-01",
                    Date = new DateOnly(2023,6,1),
                    Type = "D",
                    Amount = 150m,
                    Balance = 250m,
                },
                new Transaction
                {
                    TransactionId = "20230626-01",
                    Date = new DateOnly(2023,6,26),
                    Type = "W",
                    Amount = 20m,
                    Balance = 230m,
                },
                new Transaction
                {
                    TransactionId = "20230626-02",
                    Date = new DateOnly(2023,6,26),
                    Type = "W",
                    Amount = 100m,
                    Balance = 130m,
                }
            }
        });

        rules.Add(new InterestRule { RuleId = "RULE01", Date = new DateOnly(2023, 1, 1), Rate = 1.95m });
        rules.Add(new InterestRule { RuleId = "RULE02", Date = new DateOnly(2023, 5, 20), Rate = 1.90m });
        rules.Add(new InterestRule { RuleId = "RULE03", Date = new DateOnly(2023, 6, 15), Rate = 2.20m });
    }

}

public enum TransactionType
{
    D,
    W
}
