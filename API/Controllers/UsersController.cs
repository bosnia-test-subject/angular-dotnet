using System;
using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() 
    {
        var users = await userRepository.GetUsersAsync();

        return Ok(users);
    }
    // moramo staviti curly brackets 
    // unutar http metode zbog dynamic assigning.
    [HttpGet("{username}")] //api/users/3
    public async Task<ActionResult<AppUser>> GetUser(string username) 
    {
        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return NotFound();

        return user;
    }
}
