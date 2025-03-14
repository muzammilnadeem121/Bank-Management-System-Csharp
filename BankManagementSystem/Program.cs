using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace BankManagementSystem
{
    class User
    {

        public string Username { get; set; }
        public string Pin { get; set; }  // Store securely (e.g., hashed in real cases)
        public decimal Balance { get; set; }
        public List<string> TransactionHistory { get; set; }

        
        public User(string username, string pin)
        {
            this.Username = username;
            this.Pin = pin;
            this.Balance = 0;
            this.TransactionHistory = new List<string>();
        }

        // Methods
        public void Deposit()
        {
            Console.Write("Enter the amount to Deposit: ");
            decimal dptAmount = Convert.ToDecimal(Console.ReadLine());
            if (dptAmount > 0)
            {
                Console.Write("Confirm Transaction? (Y/N): ");
                string confirmation = Console.ReadLine().ToLower();
                if (confirmation != "y")
                {
                    Console.WriteLine("Transaction Cancelled.");
                    return;
                }
                this.Balance = this.Balance + dptAmount;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Transaction Successful , New Balance: {this.Balance}");
                Console.ResetColor();
                AddTransaction($"An amount of {dptAmount} has been Deposited successfully to {this.Username}.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid deposit amount.");
                Console.ResetColor();
            }
        }
        public bool Withdraw() 
        {
            Console.Write("Enter the amount to WithDraw: ");
            decimal wtdAmount = Convert.ToDecimal(Console.ReadLine());
            if (this.Balance >= wtdAmount && wtdAmount > 0)
            {
                Console.Write("Confirm Transaction? (Y/N): ");
                string confirmation = Console.ReadLine().ToLower();
                if (confirmation != "y")
                {
                    Console.WriteLine("Transaction Cancelled.");
                    return false;
                }
                this.Balance = this.Balance - wtdAmount;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Withdrawal Successful , Remaining Balance: {this.Balance}");
                Console.ResetColor();
                AddTransaction($"A Withdrawal of amount {wtdAmount} has been successfully done from {this.Username}.");
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Insufficient Balance or Invalid Amount.");
                Console.ResetColor();
                return false;
            }
        }
        public void AddTransaction(string transaction)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.TransactionHistory.Add($"[{timestamp}] {transaction}");
        }
    }

    class BankSystem
    {
        // Fields
        private List<User> users; // Stores all users
        private User loggedInUser; // Stores the current logged-in user
        private const string FilePath = "users.json"; // Path to store user data

        // Constructor
        public BankSystem()
        {
            users = new List<User>();
            LoadUsers();
        }

        // Methods
        public void LoadUsers()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
        }

        public void RegisterUser()
        {
            Console.Write("Enter Username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string pin = Console.ReadLine();

            if (users.Any(u => u.Username == username))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Username already exists. Please choose another.");
                Console.ResetColor();
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Note: user registered succesfully");
                Console.ResetColor();
                loggedInUser = new User(username , pin);
                users.Add(loggedInUser);
                saveUsers();
            }
        }

        public void saveUsers()
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public bool LoginUser()
        {
            Console.Write("Enter Username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string pin = Console.ReadLine();
            loggedInUser = users.FirstOrDefault(u => u.Username == username && u.Pin == pin);
            if (loggedInUser != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Login successful! Welcome, " + loggedInUser.Username);
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid username or PIN. Please try again.");
                Console.ResetColor();
                return false;
            }
        }
        public void LogoutUser()
        {
            loggedInUser = null;
        }
        public void DepositMoney()
        {
            if (loggedInUser != null)
            {
                loggedInUser.Deposit();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please Log-in to your account to perform this Operation.");
                Console.ResetColor();
                return;
            }
        }
        public void WithdrawMoney()
        {
            if (loggedInUser != null)
            {
                loggedInUser.Withdraw();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please Log-in to your account to perform this Operation.");
                Console.ResetColor();
                return;
            }
        }
        public void ShowBalance()
        {
            if (loggedInUser != null)
            {
                Console.Write("Your Current Balance is: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(loggedInUser.Balance);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please log in to check your balance.");
                Console.ResetColor();
            }
        }

        public void TransferMoney()
        {
            if (loggedInUser == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please log in to transfer money.");
                Console.ResetColor();
                return;
            }

            Console.Write("Enter recipient's username: ");
            string recipientUsername = Console.ReadLine();

            User recipient = users.FirstOrDefault(u => u.Username == recipientUsername);

            if (recipient == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Recipient does not exist.");
                Console.ResetColor();
                return;
            }

            Console.Write("Enter amount to transfer: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal transferAmount) && transferAmount > 0)
            {
                if (loggedInUser.Balance >= transferAmount)
                {
                    // Deduct from sender
                    loggedInUser.Balance -= transferAmount;
                    loggedInUser.AddTransaction($"Transferred {transferAmount} to {recipient.Username}.");

                    // Add to recipient
                    recipient.Balance += transferAmount;
                    recipient.AddTransaction($"Received {transferAmount} from {loggedInUser.Username}.");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Transfer Successful! Your new balance: {loggedInUser.Balance}");
                    Console.ResetColor();

                    // Save changes to JSON file
                    saveUsers();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Insufficient balance.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid amount entered.");
                Console.ResetColor();
            }
        }


        public void ShowTransactionHistory()
        {
            if (loggedInUser != null)
            {
                if (loggedInUser.TransactionHistory.Count > 0)
                {                    
                    Console.WriteLine("Transaction History:");
                    foreach (var transaction in loggedInUser.TransactionHistory)
                    {
                        Console.WriteLine(transaction);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("No Transactions Yet");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please log in to see your Transaction History.");
                Console.ResetColor();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BankSystem bank = new BankSystem();
            bool exit = false;

            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n==== Welcome to Bank Management System ====");
                Console.ResetColor();
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Deposit Money");
                Console.WriteLine("4. Withdraw Money");
                Console.WriteLine("5. Transfer Money");
                Console.WriteLine("6. Check Balance");
                Console.WriteLine("7. Transaction History");
                Console.WriteLine("8. Logout");
                Console.WriteLine("9. Exit");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        bank.RegisterUser();
                        break;

                    case "2":
                        if (bank.LoginUser())
                        {
                            Console.WriteLine("You are now logged in!");
                        }
                        break;

                    case "3":
                        bank.DepositMoney();
                        break;

                    case "4":
                        bank.WithdrawMoney();
                        break;

                    case "5":
                        bank.TransferMoney();
                        break;

                    case "6":
                        bank.ShowBalance();
                        break;

                    case "7":
                        bank.ShowTransactionHistory();
                        break;

                    case "8":
                        bank.LogoutUser();
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Logged out successfully.");
                        Console.ResetColor();
                        break;

                    case "9":
                        exit = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Thank you for using the Bank Management System.");
                        Console.ResetColor();
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option! Please try again.");
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
}
