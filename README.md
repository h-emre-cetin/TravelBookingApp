# Travel Booking Application

A comprehensive travel booking platform built with ASP.NET Core 8 that allows users to search for and book flights, hotels, and rental cars. The application integrates with external travel APIs to provide real-time availability and pricing information.

## Features

- Search for flights by departure city, arrival city, and date
- Search for hotels by city, check-in, and check-out dates
- Search for rental cars by location, pickup, and return dates
- Book flights, hotels, and rental cars
- View and manage bookings
- User registration and authentication

## Technologies Used

- ASP.NET Core 8 Web API
- Entity Framework Core with MySQL
- Amadeus Travel API (free tier)
- AutoMapper for object mapping
- FluentValidation for request validation
- Serilog for logging
- Polly for HTTP resilience
- xUnit, Moq, and FluentAssertions for testing

## Prerequisites

- .NET 8 SDK
- MySQL Server
- Amadeus API credentials (sign up at https://developers.amadeus.com/)

## Setup and Installation

1. Clone the repository:
   - git clone https://github.com/h-emre-cetin/travel-booking-app.git cd travel-booking-app
2. Set up the MySQL database:
- Create a database named `TravelBookingDb`
- Update the connection string in `appsettings.json` with your MySQL credentials

3. Set up Amadeus API credentials:
- Sign up for a free account at https://developers.amadeus.com/
- Create a new API key
- Update the `ExternalApi` section in `appsettings.json` with your API key and secret

4. Restore NuGet packages:
   - dotnet restore

5. Apply database migrations:
   - cd src/TravelBookingApp.API dotnet ef database update
6. Run the application:
   -dotnet run

7. Access the API at `https://localhost:7283/swagger`

## Project Structure

- **TravelBookingApp.API**: Main API project with controllers and configuration
- **TravelBookingApp.Core**: Core business logic and service interfaces
- **TravelBookingApp.Domain**: Domain models and entities
- **TravelBookingApp.Infrastructure**: Data access, repositories, and external service integrations
- **TravelBookingApp.UnitTests**: Unit tests for the application
- **TravelBookingApp.IntegrationTests**: Integration tests for the application

## API Endpoints

### Flights

- `GET /api/flights/search?departureCity={city}&arrivalCity={city}&date={date}`: Search for flights
- `GET /api/flights/{id}`: Get flight details
- `POST /api/flights/{id}/book`: Book a flight

### Hotels

- `GET /api/hotels/search?city={city}&checkIn={date}&checkOut={date}`: Search for hotels
- `GET /api/hotels/{id}`: Get hotel details
- `POST /api/hotels/{id}/book`: Book a hotel

### Rental Cars

- `GET /api/rentalcars/search?location={location}&pickupDate={date}&returnDate={date}`: Search for rental cars
- `GET /api/rentalcars/{id}`: Get rental car details
- `POST /api/rentalcars/{id}/book`: Book a rental car

### Bookings

- `GET /api/bookings/user/{userId}`: Get user's bookings
- `GET /api/bookings/{id}`: Get booking details
- `POST /api/bookings/{id}/cancel`: Cancel a booking

### Users

- `POST /api/users/register`: Register a new user
- `POST /api/users/login`: Authenticate a user
- `GET /api/users/{id}`: Get user details

## Running Tests
- dotnet test

## License

This project is licensed under the MIT License - see the LICENSE file for details.
