using System;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers() 
    {
        var users = await userRepository.GetMembersAsync();

        return Ok(users);
    }
    // moramo staviti curly brackets 
    // unutar http metode zbog dynamic assigning.
    [HttpGet("{username}")] //api/users/3
    public async Task<ActionResult<MemberDto>> GetUser(string username) 
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(username == null) return BadRequest("No username found inside token!");

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("Could not find user!");

        mapper.Map(memberUpdateDto, user);
        // pointless line ako korisnik ne promjeni nista vrati mu se badrequest sto je i okej.
       // userRepository.Update(user);

        if(await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update the user!");
    }
}
