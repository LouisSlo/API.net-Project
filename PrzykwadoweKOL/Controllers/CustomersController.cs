using Microsoft.AspNetCore.Mvc;
using PrzykwadoweKOL.DTOs;
using PrzykwadoweKOL.Exceptions;
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
    public async Task<IActionResult> AddRental(int id, [FromBody] CreateRentalRequestDTO request)
    {
        try
        {
            // Przesyłamy zamówienie do kucharza. On zajmie się transakcją w bazie.
            int newRentalId = await _repository.AddRentalAsync(id, request);
            // Jeśli się udało, zwracamy kod 201 Created 
            // Informujemy klienta, pod jakim numerem (ID) jest jego nowe wypożyczenie
            return Created($"api/customers/{id}/rentals/{newRentalId}", new { Id = newRentalId });
        }
        catch (NotFoundException ex)
        {
            // Kucharz nie znalazł klienta lub filmu! 
            // Zwracamy piękny kod 404 (Not Found)
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            // Klient zepsuł żądanie (np. zła cena)
            // Zwracamy kod 400 (Bad Request)
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Zwykły Exception łapie tu już TYLKO błędy krytyczne (np. padła baza danych)
            // Zwracamy kod 500 (Internal Server Error)
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera. Spróbuj ponownie później.");
        }
    }

    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> UpdateRental(int id, [FromBody] ReturnRentalRequest request)
    {
        ReturnRentalStatus status = await _repository.UpdateReturnRentalRequestAsync(id);

        switch (status)
        {
            case ReturnRentalStatus.NotFound:
                return NotFound($"Nie znaleziono wypożyczenia o ID: {id}");
            case ReturnRentalStatus.AlreadyReturned:
                return BadRequest($"Wypożyczenie o ID: {id} zostało już wcześniej zwrócone.");
            case ReturnRentalStatus.Success:
                return NoContent();
            default:
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }

}