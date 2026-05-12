using PrzykwadoweKOL.DTOs;

namespace PrzykwadoweKOL.Repositories;

public interface ICustomerRepository
{
    Task<CustomerRentalDTO> GetCustomerWithRentalsAsync(int customerId);
    Task<int> AddRentalAsync(int customerId, CreateRentalRequestDTO request);
    
    Task<ReturnRentalStatus> UpdateReturnRentalRequestAsync(int rentalId);
    
    Task<ReturnRentalStatus> DeleteRentalAsync(int rentalId);
}