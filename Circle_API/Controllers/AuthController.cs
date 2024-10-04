using Circle_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly CircleDbContext _context;

    public AuthController(CircleDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username == loginUser.Username && u.Password == loginUser.Password);

        if (user == null)
        {
            Console.WriteLine("Invalid login attempt for: " + loginUser.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }
        
        return Ok(new User 
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
            
        });
    }


    [HttpPut("edit")]
    public async Task<IActionResult> EditUser([FromBody] User updatedUser)
    {
        var user = await _context.Users.FindAsync(updatedUser.Id);
        if (user == null) return NotFound();

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        if (!string.IsNullOrEmpty(updatedUser.Password))
        {
            user.Password = updatedUser.Password;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "User updated successfully" });
    }

    [HttpGet("product/list")]
    public async Task<IActionResult> GetProducts()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://www.pqstec.com/InvoiceApps/values/GetProductListAll");

        if (!response.IsSuccessStatusCode) return BadRequest("Unable to fetch products");

        var productData = await response.Content.ReadAsStringAsync();
        return Ok(productData);
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(user);
    }



}
