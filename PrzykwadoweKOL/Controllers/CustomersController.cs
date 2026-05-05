using Microsoft.AspNetCore.Mvc;
using PrzykwadoweKOL.DTOs;
using PrzykwadoweKOL.Repositories;

namespace PrzykwadoweKOL.Controllers;

[ApiController]
[Route("api/customers")] 
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;

    // Konstruktor: System sam daje nam kucharza (Repozytorium)
    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    // 1. ZADANIE GET: api/customers/{id}/rentals
    [HttpGet("{id:int}/rentals")] 
    public async Task<IActionResult> GetCustomerRentals(int id)
    {
        // Prosimy kucharza o dane
        var customerData = await _repository.GetCustomerWithRentalsAsync(id);

        // Jeśli kucharz nic nie znalazł, zwracamy kod 404 [cite: 226]
        if (customerData == null)
        {
            return NotFound($"Klient o ID {id} nie posiada wypożyczeń lub nie istnieje.");
        }

        // Jeśli są dane, zwracamy kod 200 OK z paczką JSON [cite: 229]
        return Ok(customerData);
    }

    // 2. ZADANIE POST: api/customers/{id}/rentals
    [HttpPost("{id:int}/rentals")]
    public async Task<IActionResult> AddRental(int id, [FromBody] CreateRentalRequestDTO request)  {
        try
        {
            // Przesyłamy zamówienie do kucharza. On zajmie się transakcją w bazie.
            int newRentalId = await _repository.AddRentalAsync(id, request);
            // Jeśli się udało, zwracamy kod 201 Created 
            // Informujemy klienta, pod jakim numerem (ID) jest jego nowe wypożyczenie
            return Created($"api/customers/{id}/rentals/{newRentalId}", new { Id = newRentalId });
        }
        catch (Exception ex)
        {
            // Jeśli kucharz zgłosił błąd (np. klient lub film nie istnieje), zwracamy 400 Bad Request [cite: 264]
            // Wyświetlamy klientowi treść błędu, którą przygotowaliśmy w Repozytorium
            return BadRequest(ex.Message);
        }
    }
}