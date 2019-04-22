using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController: ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles() 
        {
            var userList=await (from user in _context.Users orderby user.UserName
                                    select new {
                                        Id=user.Id,
                                        UserName=user.UserName,
                                        Roles=(from userRole in user.UserRoles
                                            join role in _context.Roles
                                            on userRole.RoleId equals role.Id
                                            select role.Name).ToList()
                                    }).ToListAsync();
            
            return Ok(userList);
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpPost("editRoles/{username}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto) 
        {
            var user=await _userManager.FindByNameAsync(userName);

            var userRoles=await _userManager.GetRolesAsync(user);

            var selectedRoles=roleEditDto.RoleNames;
            selectedRoles=selectedRoles ?? new string[] {};

            var result=await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded)
                return BadRequest("Failed to add the roles for user");
            
            result=await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded)
                return BadRequest("Failed to remove the roles for user");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration() 
        {
            return Ok("Admins or moderators only can see this!");
        }
    }
}