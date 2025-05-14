using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CheckIfUserExistsAsync(string username)
        {
            try
            {
                return await _userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if user exists: {username}", username);
                throw;
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                if (await CheckIfUserExistsAsync(registerDto.Username))
                    throw new InvalidOperationException("Username is taken!");

                var user = _mapper.Map<AppUser>(registerDto);
                user.UserName = registerDto.Username.ToLower();

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                    throw new InvalidOperationException("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

                return new UserDto
                {
                    Username = user.UserName,
                    Token = await _tokenService.CreateToken(user),
                    Gender = user.Gender,
                    KnownAs = user.KnownAs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                throw;
            }
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.Users.Include(x => x.Photos)
                    .FirstOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());

                if (user == null || user.UserName == null)
                    throw new UnauthorizedAccessException("Invalid username.");

                var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

                if (!result)
                    throw new UnauthorizedAccessException("Invalid password.");

                return new UserDto
                {
                    Username = user.UserName,
                    KnownAs = user.KnownAs,
                    Token = await _tokenService.CreateToken(user),
                    Gender = user.Gender,
                    PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in user.");
                throw;
            }
        }
    }
}