EventDrivenBookstoreFunction
# Overview
This Azure Function application is designed to handle events related to books in a bookstore. It triggers when a book is added to an Azure Cosmos DB container and sends email notifications to subscribers interested in the book's genre using the Brevo API.

# Features
Trigger Event: Responds to changes in the Cosmos DB container Books.
Email Notification: Sends notifications to subscribers based on their preferred genres using the Brevo API.

# Technologies Used
Azure Functions: For serverless computing and executing code in response to events.
Azure Cosmos DB: For a NoSQL database solution to store and manage book and subscriber data.
C# (.NET 6): The programming language and framework used for developing the Azure Function.
Brevo API: For sending transactional emails to subscribers.
SibApiV3Sdk: The SDK for interacting with the Brevo email API.

# Function Logic
# BookTrigger Class
Handles the Cosmos DB trigger and sends email notifications:

Constructor: Initializes the logger, Cosmos DB client, and Brevo email API client.
Run Method: Triggered by changes in the Books container.
NotifySubscribers Method: Queries subscribers interested in the book's genre and sends notifications.
SendEmail Method: Sends an email using the Brevo API.

# Example Usage
# Add a New Book
When a new book is added to the Books container in Cosmos DB, the function triggers and notifies subscribers interested in the book's genre.

# Subscriber Notification
Subscribers receive an email with details of the new book, including the title, author, and genre.
