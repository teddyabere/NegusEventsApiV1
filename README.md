# Negus Events

**Negus Events** is a comprehensive system for event management, built using C# and ASP.NET Core. It supports administrators, organizers, attendees, ticketing, and feedback while leveraging MongoDB and Redis for efficient storage and performance.

## Repository Structure

The repository is organized into the following folders:

- **`Controllers/`**: API endpoints to manage events, tickets, users, and feedback.
- **`DTO/`**: Data Transfer Objects for structured request/response models.
- **`Models/`**: Entity models defining database schemas and system logic.
- **`Services/`**: Contains system logic to handle operations like ticket purchases, event creation, and feedback analysis.
- **`Redis/`**: Implementation of Redis caching to improve application performance.
- **`wwwroot/`**: Static files and configuration for hosting.

---

## Features

- **Event Management**: Create, view, update, and delete events with organizer and attendee details.
- **Tickets System**: Purchase, reserve, and cancel tickets with real-time tracking of availability.
- **Feedback System**: Collect and analyze attendee feedback for events.
- **Redis Caching**: Accelerate performance by caching frequently accessed data.
- **JWT Authentication**: Secure endpoints with role-based access control.
- **Swagger Integration**: Explore and test APIs interactively.

---

## Getting Started

### Prerequisites

Ensure the following tools are installed:

- [.NET SDK 6.0](https://dotnet.microsoft.com/)
- [MongoDB](https://www.mongodb.com/)
- [Redis](https://redis.io/)

---

### Configuration

#### `appsettings.json`

The `appsettings.json` file configures MongoDB, Redis, and authentication:

```json
{
  "NegusEventsDbSettings": {
    "ConnectionString": "mongodb://10.1.1.54,10.1.1.48,10.1.1.46/?replicaSet=NegusReplicaSet&readPreference=secondaryPreferred"
    "DatabaseName": "NegusEvents"
  },
  "Jwt": {
    "Key": "YourSecretKeyForAuthenticationOfApplication",
    "Issuer": "YourCompanyIssuer.com"
  },
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
## Installation

### Clone the Repository

```bash
git clone https://github.com/teddyabere/NegusEventsApiV1.git
cd NegusEventsApiV1
```

### Restore Dependencies

```bash
dotnet restore
```

### Run the Application

```bash
dotnet run
```
### Access the API

- [http://localhost:5000](http://localhost:5000)(HTTP)
- [https://localhost:5001](https://localhost:5001)(HTTPs)
