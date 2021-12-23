using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        public readonly DataContext _context;
        public UsersController(DataContext context)
        {
             _context = context;
        }

        // api/users
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() =>
         await _context.Users.ToListAsync();   
          
       
        [Authorize]
        [HttpGet("{id}")]   // api/users/1
        public async Task<AppUser> GetUser(int id) => await _context.Users.FindAsync(id); 
        
    }
}