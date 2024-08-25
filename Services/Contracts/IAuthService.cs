using Entities.Dtos;
using Microsoft.AspNetCore.Identity;


namespace Services.Contracts
{
    public interface IAuthService
    {
        Task<IdentityResult> CreateUserAsync(UserDTO userDto);
    }
}
