using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Services.Contracts;


namespace Services
{
    public class AuthManager : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public AuthManager(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IdentityResult> CreateUserAsync(UserDTO userDto)
        {
            var user= _mapper.Map<User>(userDto);
            var result= await _userManager.CreateAsync(user,userDto.Password);
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            user.CreatedAt= DateTime.Now;

            if (!result.Succeeded || !roleResult.Succeeded)
            {
                throw new Exception("Registration failed.");
            }
            return result;
        }

        
    }
}
