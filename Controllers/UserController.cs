using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestProject.Models;
using TestProject;
using System.Data;
using Serilog;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

public UserController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получение всех пользователей с фильтрами
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Id",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string? filterName = null,
        [FromQuery] int? filterAge = null,
        [FromQuery] string? filterEmail = null,
        [FromQuery] string? filterRoleName = null)
    {
        IQueryable<User> query = _context.Users.Include(u => u.Roles);

        if (!string.IsNullOrEmpty(filterName))
            query = query.Where(u => u.Name.Contains(filterName));

        if (filterAge != null)
            query = query.Where(u => u.Name.Equals(filterAge));

        if (!string.IsNullOrEmpty(filterEmail))
            query = query.Where(u => u.Name.Contains(filterEmail));

        if (!string.IsNullOrEmpty(filterRoleName))
            query = query.Where(u => u.Roles.Any(r => r.Name == filterRoleName));

        if (sortBy == "Id" && sortOrder == "asc")
            query = query.OrderBy(u => u.Id);
        else if (sortBy == "Id" && sortOrder == "desc")
            query = query.OrderByDescending(u => u.Id);

        int totalItems = await query.CountAsync();
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        var users = await query.ToListAsync();
        var response = new
        {
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            Users = users
        };

        return Ok(response);
    }

    /// <summary>
    /// Получение пользователя по Id
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
        if (id <= 0)
            return NotFound("поле id не должно быть пустым или содержать отрицательное число");

        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

        Log.Information("Поиск пользователя с id: {Id}", id);

        if (user == null)
            return NotFound();

        Log.Information("Пользователь найден");
        return Ok(user);
    }

    /// <summary>
    /// Добавление пользователю роли по Id роли
    /// </summary>
    /// <remarks>
    /// Роли и их Id:
    /// User - 1;
    /// Admin - 2;
    /// Support - 3;
    /// SuperAdmin - 4
    /// </remarks>
    /// <returns></returns>
    [HttpPost("AddRoleToUser")]
    public async Task<IActionResult> AddRoleToUser(int id, [FromBody] int roleId)
    {
        if (id <= 0)
            return NotFound("Пользователь не найден");

        if (roleId < 1 && roleId > 4)
            return NotFound("Роль не существует");

        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

        if (user == null || role == null)
            return NotFound();

        user.Roles.Add(role);
        await _context.SaveChangesAsync();

        Log.Information("Пользователю с id - {User} добалена роль {Role}", id, role.Name);

        return Ok(user);
    }

    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    /// /// <remarks>
    /// Роли и их Id:
    /// User - 1;
    /// Admin - 2;
    /// Support - 3;
    /// SuperAdmin - 4
    /// </remarks>
    /// <returns></returns>
    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [FromQuery] string name,
        [FromQuery] int age,
        [FromQuery] string email,
        [FromQuery] int roleId = 1)
    {        
        User user = new User();

        if (string.IsNullOrEmpty(name) || age <= 0 || string.IsNullOrEmpty(email))
            return NotFound("Не все обязательные поля заполнены.");

        if (_context.Users.Any(u => u.Email == email))
            return NotFound("Пользователь с таким Email уже существует.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

        user.Name = name;
        user.Age = age;
        user.Email = email;
        user.Roles = new List<Role>{ role };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        Log.Information("Создан пользователь с Name: {name}, Age: {age}, Email: {email} и ролью: {role}", name, age, email, role.Name);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Изменение данных о пользователе по Id пользователя
    /// </summary>
    /// <remarks>
    /// Роли и их Id:
    /// User - 1;
    /// Admin - 2;
    /// Support - 3;
    /// SuperAdmin - 4
    /// </remarks>
    /// <returns></returns>
    [HttpPut("UpdateUser")]
    public async Task<IActionResult> UpdateUser(int id,
        [FromQuery] string? name,
        [FromQuery] int? age,
        [FromQuery] string? email)
    {
        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("Пользователь не найден");

        if (_context.Users.Any(u => u.Email == email && u.Id != id))
            return BadRequest("Пользователь с таким Email уже существует.");

        if (!string.IsNullOrEmpty(name))
            user.Name = name;

        if (age != 0)
            user.Age = (int)age;

        if (!string.IsNullOrEmpty(email))
            user.Email = email;

        await _context.SaveChangesAsync();

        Log.Information("Информация о пользователе {Id} обновлена", id);

        return Ok(user);
    }

    /// <summary>
    /// Удаление пользователя по его Id
    /// </summary>
    /// <returns></returns>
    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUser(int id)
    {

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("Пользователь не найден");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        Log.Information("Пользователь с id: {Id}  удален", id);

        return NoContent();
    }
}