namespace AwesomeGIC.Services
{
    public class GICService
    {
        public List<Account> Accounts { get; set; }
        public List<InterestRule> Rules { get; set; }

        public GICService()
        {
            Accounts = new List<Account>();
            Rules = new List<InterestRule>();
        }

        public void InputTransaction(List<Account> accounts)
        {
            Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format");

            string? transactionDetails = Console.ReadLine();
            // check for valid input
            if (transactionDetails == null)
            {
                Console.WriteLine("Invalid input. Please try again.");
                return;
            }

            var tranSplit = transactionDetails?.Trim().Split(" ");

            if (tranSplit?.Length != 4)
            {
                Console.WriteLine("Invalid input. Please try again.");
                return;
            }

            var account = accounts.Where(x => x.AccountId == tranSplit[1]).FirstOrDefault();

            if (account == null)
            {
                account = new Account
                {
                    AccountId = tranSplit[1]
                };
            }

            // validations
            string dateFormat = "yyyyMMdd";
            DateTime date;

            bool isValidDate = DateTime.TryParseExact(tranSplit[0], dateFormat, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date);

            if (!isValidDate)
            {
                Console.WriteLine("Please enter a valid date for your transaction.");
                return;
            }

            decimal amount;
            if (!decimal.TryParse(tranSplit[3], out amount) || amount < 0 || decimal.Round(amount, 2) != amount)
            {
                Console.WriteLine("Invalid amount, Please try again");
                return;  
            }
            
            // validate type and handle invalid types
            string transactionType = tranSplit[2];
            if (transactionType != "D" && transactionType != "W")
            {
                Console.WriteLine("Invalid operation type. Please try again");
                return;
            }

            if (transactionType == "W" && account.Balance == 0)
            {
                Console.WriteLine("Your account balance is zero.");
                return;
            }

            if (transactionType == "W" && account.Balance < amount)
            {
                Console.WriteLine("Insufficient balance.");
                return;
            }

            string transactionId = "";
            var latestTransactionByDate = account.Transactions?
                .Where(x => x.Date == DateOnly.FromDateTime(date))
                .OrderByDescending(x => x.TransactionId)
                .FirstOrDefault();

            if (latestTransactionByDate == null)
            {
                transactionId = date.ToString("yyyyMMdd") + "-" + "01";
            }
            else
            {
                var lastCharacterStr = latestTransactionByDate.TransactionId[latestTransactionByDate.TransactionId.Length - 1].ToString();
                int lastCharacter = 0;

                if (int.TryParse(lastCharacterStr, out lastCharacter))
                {
                    transactionId = date.ToString("yyyyMMdd") + "-" + (lastCharacter + 1).ToString("00");
                }
            }

            var transaction = new Transaction
            {
                TransactionId = transactionId,
                Date = DateOnly.FromDateTime(date),
                Type = transactionType,
                Amount = amount,
            };

            //assuming user always enter newer date than existing dates (as transaction date)
            if (transactionType == "D")
            {
                account.Balance += amount;          
            }
            else
            {
                account.Balance -= amount;
            }
            transaction.Balance = account.Balance; 

            account.AccountId = tranSplit[1];

            if (account.Transactions == null)
            {
                account.Transactions = new List<Transaction>();
            }
            account.Transactions.Add(transaction);

            if (!accounts.Contains(account))
            {
                accounts.Add(account);
            }

            PrintTransactions(account);
            Console.WriteLine("Is there anything else you'd like to do?");
        }

        private void PrintTransactions(Account account)
        {
            Console.WriteLine("Account: " + account.AccountId);

            Console.Write("| Date \t\t");
            Console.Write("| Txn Id \t");
            Console.Write("| Type \t");
            Console.Write("| Amount \t\t");
            Console.Write("| \t\n");

            foreach (var transaction in account.Transactions)
            {
                Console.Write($"| {transaction.Date.ToString("yyyyMMdd")} \t");
                Console.Write($"| {transaction.TransactionId} \t");
                Console.Write($"| {transaction.Type} \t");
                Console.Write($"| {transaction.Amount} \t\t");
                Console.Write("| \t\n");
            }
            Console.WriteLine();
        }

        public void DefineInterestRule(List<InterestRule> rules)
        {
            Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format");
            Console.WriteLine("(or enter blank to go back to main menu)");
            Console.WriteLine(">");

            string? interestRule = Console.ReadLine();
            // check for valid input
            if (string.IsNullOrEmpty(interestRule))
            {
                return;
            }

            var ruleSplit = interestRule?.Trim().Split(" ");
            if (ruleSplit?.Length != 3)
            {
                Console.WriteLine("Invalid input. Please try again.");
                return;
            }

            string dateStr = ruleSplit[0];
            string ruleIdStr = ruleSplit[1];
            string rateStr = ruleSplit[2];

            string dateFormat = "yyyyMMdd";
            DateTime date;

            bool isValidDate = DateTime.TryParseExact(dateStr, dateFormat, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date);

            if (!isValidDate)
            {
                Console.WriteLine("Please enter a valid date for your transaction.");
                return;
            }

            decimal interestRate;
            if (!decimal.TryParse(rateStr, out interestRate)|| interestRate <= 0 || interestRate >= 100)
            {
                Console.WriteLine("Invalid rate, Please try again");   
            }
            
            InterestRule rule = new InterestRule
            {
                RuleId = ruleIdStr,
                Date = DateOnly.FromDateTime(date),
                Rate = interestRate,
            };

            // check if a rule for the same date exists
            var todayCreatedRule = rules.Where(r => r.Date == DateOnly.FromDateTime(date)).FirstOrDefault();
            if (todayCreatedRule != null)
            {
                rules.Remove(todayCreatedRule);
            }
            rules.Add(rule);

            PrintRules(rules);
            Console.WriteLine("Is there anything else you'd like to do?");
        }

        private void PrintRules(List<InterestRule> rules)
        {
            Console.Write("| Date \t\t");
            Console.Write("| Rule Id \t");
            Console.Write("| Rate (%) \t");
            Console.Write("| \t\n");

            var orderedRules = rules.OrderBy(x => x.Date);

            foreach (var rule in orderedRules)
            {
                Console.Write($"| {rule.Date.ToString("yyyyMMdd")} \t");
                Console.Write($"| {rule.RuleId} \t");
                Console.Write($"| {rule.Rate} \t");
                Console.Write("| \t\n");
            }
            Console.WriteLine();
        }

        public void PrintStatement(List<Account> accounts, List<InterestRule> rules)
        {
            Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
            Console.WriteLine("(or enter blank to go back to main menu)");
            Console.WriteLine(">");

            string? printStatementInput = Console.ReadLine();
            // check for valid input
            if (string.IsNullOrEmpty(printStatementInput))
            {
                return;
            }

            var inputSplit = printStatementInput?.Trim().Split(" ");
            if (inputSplit?.Length != 2)
            {
                Console.WriteLine("Invalid input. Please try again.");
                return;
            }

            string accountStr = inputSplit[0];
            string yearMonthStr = inputSplit[1];

            if (yearMonthStr.Length != 6|| !int.TryParse(yearMonthStr.Substring(0, 4), out int year) ||
                !int.TryParse(yearMonthStr.Substring(4, 2), out int month))
            {
                Console.WriteLine("Invalid <Year><Month>. Please try again.");
                return;
            }

            var account = accounts.FirstOrDefault(x => x.AccountId == accountStr);

            if (account == null)
            {
                Console.WriteLine("Invalid Account.");
                return;
            }

            var transactions = account.Transactions?
                .Where(t => t.Date.Year == year && t.Date.Month == month);

            if (transactions == null || !transactions.Any())
            {
                Console.WriteLine($"No transactions found for {yearMonthStr}.");
                return;
            }

            Console.WriteLine("Account: " + account?.AccountId);

            Console.Write("| Date \t\t");
            Console.Write("| Txn Id \t");
            Console.Write("| Type \t");
            Console.Write("| Amount \t");
            Console.Write("| Balance \t");
            Console.Write("| \t\n");

            foreach (var transaction in transactions)
            {
                Console.Write($"| {transaction.Date.ToString("yyyyMMdd")} \t");
                Console.Write($"| {transaction.TransactionId} \t");
                Console.Write($"| {transaction.Type} \t");
                Console.Write($"| {transaction.Amount} \t\t");
                Console.Write($"| {transaction.Balance} \t\t");
                Console.Write("| \t\n");
            }

            // Calculate interest
            decimal interest = CalculateInterestForMonth(year, month, account, rules);
            decimal totalAmount = account.Balance + interest;

            var monthEndDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            // Print interest statement
            Console.Write($"| {monthEndDate:yyyyMMdd} \t");
            Console.Write($"|  \t\t");
            Console.Write($"| I \t");
            Console.Write($"| {interest:0.00} \t");
            Console.Write($"| {totalAmount:0.00} \t");
            Console.WriteLine();
        }

        public decimal CalculateInterestForMonth(int year, int month, Account account, List<InterestRule> rules)
        {
            decimal totalInterest = 0m;

            var monthStartDate = new DateOnly(year, month, 1);
            var monthEndDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var transactions = account.Transactions?
                .Where(t => t.Date >= monthStartDate && t.Date <= monthEndDate)
                .OrderBy(t => t.Date);

            // Get the starting interest rate
            var currentRate = GetApplicableInterestRate(monthStartDate, rules);
            DateOnly lastInterestDate = monthStartDate;

            if (transactions == null)
            {
                CalculateInterestForGivenPeriod(lastInterestDate, monthEndDate, account.Balance, currentRate);
            } 
            else
            {
                // starting balance should be the balance of the transaction before the 1st transaction of the current month
                // if no previous transactions exists, the starting balance should be zero.
                var mostRecentTransaction = transactions
                                            .Where(t => t.Date <= monthStartDate)           
                                            .OrderByDescending(t => t.Date)            
                                            .FirstOrDefault();
                decimal balance;
                if (mostRecentTransaction == null)
                {
                    balance = 0;
                }
                else
                {
                    balance = mostRecentTransaction.Balance;
                }

                // Get the rate changes before the first transaction
                var rateChangesBeforeFirstTransaction = rules
                    .Where(r => r.Date > lastInterestDate && r.Date <= transactions?.FirstOrDefault()?.Date)
                    .OrderBy(r => r.Date);

                // Calculate the interest gained till first transaction date
                foreach (var rateChange in rateChangesBeforeFirstTransaction)
                {
                    totalInterest += CalculateInterestForGivenPeriod(lastInterestDate, rateChange.Date.AddDays(-1), balance, currentRate);
                    lastInterestDate = rateChange.Date;
                    currentRate = rateChange.Rate;
                }

                // Calculate interest earned from 1st transaction to last transaction
                foreach (var transaction in transactions)
                {
                    // Get the list of rate changes before the current transaction date
                    var rateChanges = rules
                        .Where(r => r.Date > lastInterestDate && r.Date <= transaction.Date)
                        .OrderBy(r => r.Date);

                    // Calculate the interest gained till last interest change date
                    foreach (var rateChange in rateChanges)
                    {
                        totalInterest += CalculateInterestForGivenPeriod(lastInterestDate, rateChange.Date.AddDays(-1), balance, currentRate);
                        lastInterestDate = rateChange.Date;
                        currentRate = rateChange.Rate;
                    }

                    // Calculate the interest gained from last interest change date to the transaction date
                    totalInterest += CalculateInterestForGivenPeriod(lastInterestDate, transaction.Date.AddDays(-1), balance, currentRate);
                    lastInterestDate = transaction.Date;
                    balance = transaction.Balance;

                }

                // Get the rate changes after the last transaction
                var rateChangesAfterLastTransaction = rules
                    .Where(r => r.Date > lastInterestDate && r.Date <= monthEndDate)
                    .OrderBy(r => r.Date);

                // Calculate the interest gained till last rate change date
                foreach (var rateChange in rateChangesAfterLastTransaction)
                {
                    totalInterest += CalculateInterestForGivenPeriod(lastInterestDate, rateChange.Date.AddDays(-1), balance, currentRate);
                    lastInterestDate = rateChange.Date;
                    currentRate = rateChange.Rate;
                }

                // Calculate the interest for the remaining days of the month
                totalInterest += CalculateInterestForGivenPeriod(lastInterestDate, monthEndDate, balance, currentRate);
            }

            return totalInterest / 365;
        }

        private decimal GetApplicableInterestRate(DateOnly date, List<InterestRule> interestRules)
        {
            return interestRules
                .Where(r => r.Date <= date)
                .OrderByDescending(r => r.Date)
                .FirstOrDefault()?.Rate ?? 0;
        }

        private decimal CalculateInterestForGivenPeriod(DateOnly start, DateOnly end, decimal balance, decimal rate)
        {
            int days = end.DayNumber - start.DayNumber + 1;
            return balance * (rate / 100) * days;
        }

        public void Quit()
        {
            Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
            Console.WriteLine("Have a nice day!");
            System.Environment.Exit(1);
        }

    }
}
