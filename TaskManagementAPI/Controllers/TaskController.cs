using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Create a new task (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
        {
            if (task == null) return BadRequest("Invalid task data.");

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // Retrieve a task (Admins can see all, Users can see only assigned tasks)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            // Fix: Check for null `AssignedUserId` before comparing to `user.Id`
            if (User.IsInRole("Admin") || (task.AssignedUserId != null && task.AssignedUserId == user.Id))
                return Ok(task);

            return Forbid();
        }

        // List all tasks (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetTasks()
        {
            return Ok(_context.Tasks.ToList());
        }

        // Update a task (Admin can update all fields, User can only update status)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem updatedTask)
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _context.Tasks.FindAsync(id);

            if (task == null) return NotFound();

            if (User.IsInRole("Admin"))
            {
                task.Title = updatedTask.Title;
                task.Description = updatedTask.Description;
                task.Status = updatedTask.Status;
                task.AssignedUserId = updatedTask.AssignedUserId;
            }
            else if (task.AssignedUserId == user.Id)
            {
                task.Status = updatedTask.Status; // Users can only update status
            }
            else
            {
                return Forbid();
            }

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // Delete a task (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}