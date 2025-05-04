using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    // ovdje dolazimo sa account/register
    [HttpPost("register")] 
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) 
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken!");
        // koristimo using radi memory management,
        // mi ovdje ne instanciramo u klasi hmac, niti
        // vrsimo DI, vec uz pomoc using jednom instanciramo hmac i to je to.
        using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.Username.ToLower();

      //  var user = new AppUser
      //  {
      //      UserName = registerDto.Username.ToLower(),
      //      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
      //      PasswordSalt = hmac.Key
      //  };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            Gender = user.Gender,
            KnownAs = user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await context.Users.Include(x => x.Photos).
        FirstOrDefaultAsync(x => x.UserName == 
        loginDto.Username.ToLower());

        if(user == null || user.UserName == null) return Unauthorized("Invalid username");

//         using var hmac = new HMACSHA512(user.PasswordSalt);

//         var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

// // foreach petlja disclaimer
//         for (int i = 0; i < computedHash.Length; i++)
//         {
//             if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
//         }

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };

    }

    private async Task<bool> UserExists(string username) 
    {
        return await context.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }

}
