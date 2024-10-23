using AwesomeGIC.Services;

namespace AwesomeGICTests
{
    public class GICServiceTests
    {
        private readonly GICService gicService = new GICService();

        [Fact]
        public void InputTransaction_InvalidInput()
        {
            // Arrange
            var accounts = new List<Account>();
            var input = "Test";

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.InputTransaction(accounts);

            // Assert
            Assert.Contains("Invalid input", stringWriter.ToString());
        }

        [Fact]
        public void InputTransaction_Deposit_Success()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = "ACC01", Balance = 200 }
            };

            var input = "20240101 ACC01 W 100";

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.InputTransaction(accounts);

            // Assert
            Assert.Equal(100, accounts[0].Balance); 
            Assert.Contains("ACC01", stringWriter.ToString());
        }

        [Fact]
        public void InputTransaction_Withdraw_Success()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = "ACC01", Balance = 200 }
            };
            
            var input = "20240101 ACC01 D 100";

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.InputTransaction(accounts);

            // Assert
            Assert.Equal(300, accounts[0].Balance); 
            Assert.Contains("ACC01", stringWriter.ToString());
        }

        [Fact]
        public void InputTransaction_Withdraw_InsufficientBalance()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = "ACC01", Balance = 50 }
            };
            
            var input = "20240101 ACC01 W 100";

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.InputTransaction(accounts);

            // Assert
            Assert.Contains("Insufficient balance", stringWriter.ToString());
            Assert.Equal(50, accounts[0].Balance);
        }

        [Fact]
        public void DefineInterestRule_InvalidFormat()
        {
            // Arrange
            var rules = new List<InterestRule>();
            var input = "Test"; 

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.DefineInterestRule(rules);

            // Assert
            Assert.Empty(rules);
            Assert.Contains("Invalid input. Please try again.", stringWriter.ToString());
        }

        [Fact]
        public void DefineInterestRule_AddInterestRule_Success()
        {
            // Arrange
            var rules = new List<InterestRule>();
            var input = "20240101 RULE01 5"; 

            // Redirect Console input/output for testing
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.DefineInterestRule(rules);

            // Assert
            Assert.Single(rules);
            Assert.Equal("RULE01", rules[0].RuleId);
            Assert.Equal(5, rules[0].Rate);
            Assert.Equal(DateOnly.FromDateTime(new DateTime(2024, 1, 1)), rules[0].Date);
            Assert.Contains("RULE01", stringWriter.ToString());
        }

        [Fact]
        public void DefineInterestRule_ReplaceExistingInterestRule()
        {
            // Arrange
            var rules = new List<InterestRule>
            {
                new InterestRule { 
                    RuleId = "RULE01", 
                    Date = DateOnly.FromDateTime(new DateTime(2024, 1, 1)), 
                    Rate = 5 
                }
            };

            var input = "20240101 RULE01 10";

            // Redirect Console input/output for testing
            var stringReader = new System.IO.StringReader(input);
            Console.SetIn(stringReader);
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            gicService.DefineInterestRule(rules);

            // Assert
            Assert.Single(rules); // Should still have only one rule
            Assert.Equal("RULE01", rules[0].RuleId);
            Assert.Equal(10, rules[0].Rate); // Updated rate
            Assert.Contains("RULE01", stringWriter.ToString()); // PrintRules output
        }

        [Fact]
        public void CalculateInterestForMonth_CalculateInterest_SingleTransaction()
        {
            // Arrange
            var account = new Account
            {
                AccountId = "ACC01",
                Transactions = new List<Transaction>
                {
                    new Transaction {TransactionId = "20240101-01", Date = new DateOnly(2024, 1, 15), Balance = 1000m }
                }
            };
            var rules = new List<InterestRule>
            {
                new InterestRule { RuleId = "RULE01", Date = new DateOnly(2024, 1, 1), Rate = 5 } 
            };

            // Act
            decimal result = gicService.CalculateInterestForMonth(2024, 1, account, rules);

            // Assert
            decimal expectedInterest = 1000m * 5 / 100 / 365 * 17; 
            Assert.Equal(expectedInterest, result, 2); 
        }

        [Fact]
        public void CalculateInterestForMonth_CalculateInterest_MultipleTransactions()
        {
            // Arrange
            var account = new Account
            {
                AccountId = "ACC01",
                Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "20240110-01", Date = new DateOnly(2024, 1, 10), Balance = 1000m },
                new Transaction { TransactionId = "20240120-01", Date = new DateOnly(2024, 1, 20), Balance = 1500m }
            }
            };
            var rules = new List<InterestRule>
            {
                new InterestRule {RuleId = "RULE01", Date = new DateOnly(2024, 1, 1), Rate = 5 }
            };

            // Act
            decimal result = gicService.CalculateInterestForMonth(2024, 1, account, rules);

            // Assert
            decimal expectedInterestFirstPeriod = 1000m * 5 / 100 / 365 * 10; // Jan 10 to Jan 20
            decimal expectedInterestSecondPeriod = 1500m * 5 / 100 / 365 * 12; // Jan 20 to Jan 31
            decimal expectedInterest = expectedInterestFirstPeriod + expectedInterestSecondPeriod;

            Assert.Equal(expectedInterest, result, 2);
        }

        [Fact]
        public void CalculateInterestForMonth_CalculateInterest_InterestRateRates()
        {
            // Arrange
            var account = new Account
            {
                AccountId = "ACC01",
                Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "20230601-01", Date = new DateOnly(2023, 6, 1), Balance = 250m },
                new Transaction { TransactionId = "20230626-01", Date = new DateOnly(2023, 6, 26), Balance = 230m },
                new Transaction { TransactionId = "20230626-02", Date = new DateOnly(2023, 6, 26), Balance = 130m },
            }
            };
            var rules = new List<InterestRule>
            {
                new InterestRule { RuleId = "RULE01", Date = new DateOnly(2023, 1, 1), Rate = 1.95m }, 
                new InterestRule { RuleId = "RULE02", Date = new DateOnly(2023, 5, 20), Rate = 1.9m },
                new InterestRule { RuleId = "RULE03", Date = new DateOnly(2023, 6, 15), Rate = 2.2m }
            };

            // Act
            decimal result = gicService.CalculateInterestForMonth(2023, 6, account, rules);

            // Assert
            decimal expectedInterestFirstPeriod = 250 * 1.9m / 100 / 365 * 14; // June 1 to June 14 at 1.9%
            decimal expectedInterestSecondPeriod = 250 * 2.2m / 100 / 365 * 11; // June 15 to June 25 at 2.2%
            decimal expectedInterestThirdPeriod = 130 * 2.2m / 100 / 365 * 5; // June 26 to June 30 at 2.2%
            decimal expectedInterest = expectedInterestFirstPeriod + expectedInterestSecondPeriod + expectedInterestThirdPeriod;

            Assert.Equal(expectedInterest, result, 2);
        }
    }
}