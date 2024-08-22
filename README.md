# Sarhne Application
## Project Overview

The Anonymous Messaging System allows users to send messages anonymously without revealing their identity. It includes features for managing messages, user authentication, and interactions like reactions and replies.

- *Favorites*: Users can mark messages as favorites and view them in one place.
- *Public Messages*: Users can make messages public, allowing anyone with the link to view and react to them.
- *Reactions*: Visitors can react to public messages with options like love, sad, care, or angry. The total number of reactions for each message is visible to all users.
- *Replies*: The system supports adding replies to public messages (AppearedMessage) and allows editing and deleting these replies.
- *User Management*: Users can update their details, including gender, name, email, password, and profile information.
- *Authentication*: Secure registration and login process with JWT and refresh token support.

## Features

### User Authentication & Registration

- *Registration*: Users register with a unique username, valid email, secure password, gender, optional profile link, and profile image.
- *Login*: User credentials are verified, JWT and refresh tokens are generated upon successful login. Tokens can be refreshed and revoked.
- *User Data Update*: Retrieve and update user details such as name, email, and personal information.

### Messaging

- *Send Messages*: Users can send messages with the option to reveal their identity or remain anonymous.
- *View Messages*: Users can view received messages and responses, and delete any received messages.
- *Favorites*: Users can mark and view all their favorite messages.
- *Public Messages*: Messages can be made public, allowing anyone with the link to view and react.
- *Replies*: Add, edit, and delete replies to public messages.

## Testing

- *AppearedMessageServiceTests*: Includes tests for adding, deleting, and updating replies, using NUnit, AutoMapper, and mocks.
- *AuthServiceTests*: Covers registration, login, and token management, validating scenarios like duplicate emails and valid/invalid login attempts.
- *ReactionServiceTests*: Validates reaction fetching and processing methods.
- *TokenServiceTests*: Ensures correct token generation and refresh token properties.
- *UserServiceTests*: Tests methods for retrieving and updating user data.
- *GenericRepositoryTests*: Utilizes an in-memory database to simulate interactions and verify repository methods.

## Dependencies

This project uses several essential packages to enhance functionality:

- *AutoMapper*: For object-object mapping.
- *Microsoft.EntityFrameworkCore.Tools*: Database management tools.
- *Microsoft.EntityFrameworkCore*: Core library for Entity Framework.
- *Microsoft.EntityFrameworkCore.SqlServer*: SQL Server support.
- *Microsoft.EntityFrameworkCore.InMemory*: In-memory database for unit testing.
- *Microsoft.AspNetCore.Identity.EntityFrameworkCore*: Identity and authentication.
- *Microsoft.NET.Test.Sdk*: Test SDK for .NET.
- *Moq*: For mocking dependencies.
- *NUnit*: Testing framework.
- *NUnit3TestAdapter*: Adapter for running NUnit tests.

## Installation

1. *Clone the repository:*

   ```bash
   git clone https://github.com/Yahya-Elebrashy/SarhneApp.git
