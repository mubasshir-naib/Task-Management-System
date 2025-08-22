using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Task_Management.Data;
using Task_Management.Models;
using Task_Management.Services;


namespace Task_Management.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public TasksController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        //// GET: Tasks
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Tasks.ToListAsync());
        //}
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var tasks= await _context.Tasks
                .Where(t=>t.UserId == userId).ToListAsync();
            return View(tasks);
        }
        // New: Get tasks as JSON for AJAX
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
            return Json(tasks);
        }

        // New: Update status via AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User))
            {
                return Json(new { success = false, message = "Task not found or unauthorized" });
            }

            var oldStatus = taskItem.Status;

            taskItem.Status = newStatus;
            _context.Update(taskItem);
            await _context.SaveChangesAsync();

            if (oldStatus != newStatus)
            {
                var user = await _userManager.FindByIdAsync(taskItem.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendEmailAsync(user.Email, "Task Status Changed", $"Your task '{taskItem.Title}' status has changed from {oldStatus} to {newStatus}.");
                }
            }

            return Json(new { success = true, message = "Status updated successfully" });
        }


        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasksEntity = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasksEntity == null)
            {
                return NotFound();
            }

            return View(tasksEntity);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,DueDate,Status,UserId")] TasksEntity tasksEntity)
        {
            if (ModelState.IsValid)
            {
                tasksEntity.UserId = _userManager.GetUserId(User);
                _context.Add(tasksEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tasksEntity);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasksEntity = await _context.Tasks.FindAsync(id);
            if (tasksEntity == null|| tasksEntity.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            return View(tasksEntity);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,Status,UserId")] TasksEntity tasksEntity)
        {
            if (id != tasksEntity.Id || tasksEntity.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldStatus = tasksEntity.Status;
                    _context.Update(tasksEntity);
                    await _context.SaveChangesAsync();

                    if (oldStatus != tasksEntity.Status) {
                        var user = await _userManager.FindByIdAsync(tasksEntity.UserId);
                        if(user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailService.SendEmailAsync(user.Email, "Task Status Changed", $"Your task '{tasksEntity.Title}' status has changed from {oldStatus} to {tasksEntity.Status}.");
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasksEntityExists(tasksEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tasksEntity);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasksEntity = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasksEntity == null || tasksEntity.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            return View(tasksEntity);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tasksEntity = await _context.Tasks.FindAsync(id);
            if (tasksEntity != null && tasksEntity.UserId==_userManager.GetUserId(User))
            {
                _context.Tasks.Remove(tasksEntity);
                await _context.SaveChangesAsync();
            }

     
            return RedirectToAction(nameof(Index));
        }

        private bool TasksEntityExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
