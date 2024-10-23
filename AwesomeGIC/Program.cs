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
            Console.WriteLine(">");

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
}

