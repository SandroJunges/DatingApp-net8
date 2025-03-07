using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUserWithRoles() 
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new 
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromBody]List<string> roles) 
    {
        if (!roles.Any()) return BadRequest("you must select at least one role");

        // var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);

        if(user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, roles.Except(userRoles));
        
        if(!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(roles));

        if(!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotoForModeration() 
    {
        return Ok("Admins or moderators can see this");
    }
}
