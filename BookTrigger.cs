using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Linq;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using EventDrivenBookstoreFunction.Models;
using Microsoft.Azure.Cosmos;

namespace EventDrivenBookstoreFunction
{
    // Class handling the trigger for book events in the bookstore
    public class BookTrigger
    {
        private readonly ILogger<BookTrigger> _logger; // Logger for logging information and errors
        private readonly CosmosClient _cosmosClient; // Client for interacting with Azure Cosmos DB
        private readonly IConfiguration _configuration; // Configuration for accessing app settings
        private readonly TransactionalEmailsApi _emailApi; // API for sending transactional emails

        // Constructor to initialize dependencies
        public BookTrigger(ILoggerFactory loggerFactory, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<BookTrigger>(); // Initialize logger
            _cosmosClient = cosmosClient; // Initialize Cosmos DB client
            _configuration = configuration; // Initialize configuration

            // Initialize Brevo email API client with API key from configuration
            Configuration.Default.AddApiKey("api-key", _configuration["Brevo:ApiKey"]);
            _emailApi = new TransactionalEmailsApi(); // Initialize email API client
        }

        // Function triggered by changes in the Cosmos DB container
        [Function("BookTrigger")]
        public async System.Threading.Tasks.Task Run(
            [CosmosDBTrigger(
                databaseName: "BookstoreDB", // Database name in Cosmos DB
                containerName: "Books", // Container name in Cosmos DB
                Connection = "jayazure_DOCUMENTDB", // Connection string to Cosmos DB
                LeaseContainerName = "leases", // Lease container for managing change feed
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Book> books) // List of modified books
        {
            if (books != null && books.Count > 0) // Check if there are modified books
            {
                // Iterate over each modified book
                foreach (var book in books)
                {
                    await NotifySubscribers(book); // Notify subscribers about the new book
                }
            }
        }

        // Method to notify subscribers about a new book
        private async System.Threading.Tasks.Task NotifySubscribers(Book book)
        {
            var databaseName = _configuration["databaseName"]; // Get database name from configuration
            var containerName = "Subscribers"; // Container name for subscribers
            var container = _cosmosClient.GetContainer(databaseName, containerName); // Get Cosmos DB container for subscribers

            // Query to get subscribers interested in the book's genre
            var query = container.GetItemLinqQueryable<Subscriber>()
                .Where(sub => sub.PreferredGenres.Contains(book.Genre))
                .ToFeedIterator();

            // Process each batch of query results
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                foreach (var subscriber in response) // Iterate over each subscriber
                {
                    await SendEmail(subscriber.Email, book); // Send email notification to the subscriber
                }
            }
        }

        // Method to send email notification about the new book
        private async System.Threading.Tasks.Task SendEmail(string recipientEmail, Book book)
        {
            var senderEmail = _configuration["Brevo:SenderEmail"]; // Get sender email from configuration
            if (string.IsNullOrEmpty(senderEmail)) // Check if sender email is missing
            {
                return; // Exit method if sender email is not configured
            }

            // Create email content
            var sendSmtpEmail = new SendSmtpEmail
            {
                To = new List<SendSmtpEmailTo>
                {
                    new SendSmtpEmailTo(recipientEmail) // Recipient's email address
                },
                Sender = new SendSmtpEmailSender { Email = senderEmail }, // Sender's email address
                Subject = $"New Book Notification: {book.Title}", // Email subject
                TextContent = $"A new book has been added to the {book.Genre} genre: {book.Title} by {book.Author}." // Email body
            };

            try
            {
                // Send the email using the email API
                var response = _emailApi.SendTransacEmail(sendSmtpEmail);
                _logger.LogInformation($"Notification sent to {recipientEmail}. Response: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send notification to {recipientEmail}. Error: {ex.Message}");
            }
        }
    }
}
