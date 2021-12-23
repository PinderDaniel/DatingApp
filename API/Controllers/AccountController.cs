using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService) => 
            (_context, _tokenService) = (context, tokenService);
        
        [HttpPost("register")] // api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
          
            if(await UserExists(registerDto.UserName)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();
            
            var user = new AppUser{
                UserName = registerDto.UserName.ToLower().Trim(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return Created("", new UserDto
            {
                UserName = user.UserName, 
                Token = _tokenService.CreateToken(user)
            });
        }

        [HttpPost("login")] // api/account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _context.Users
            .SingleOrDefaultAsync( u => u.UserName.ToLower() == loginDto.UserName);

            if(user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
                        
            for(int i = 0; i < user.PasswordHash.Length; i++)            
                if(user.PasswordHash[i] != computedHash[i]) return Unauthorized("Invalid password");

            return new UserDto
            {
                UserName = user.UserName, 
                Token = _tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExists(string username) =>
            await _context.Users.AnyAsync( u => u.UserName == username.ToLower().Trim());
        
    }
}