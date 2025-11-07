using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Comprehension.Auth;
using Comprehension.Data;
using Comprehension.Models;

namespace Comprehension.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireUser]
    public class RemindersController : ControllerBaseEx
    {
        private readonly ComprehensionContext _db;
        public RemindersController(ComprehensionContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reminder>>> GetReminder()
        {
            var uid = CurrentUserId;

            var sharedIds = await _db.SharedAccess
                .Where(s => s.ResourceType == ResourceType.Reminder && s.GranteeUserId == uid)
                .Select(s => s.ResourceId)
                .ToListAsync();

            return await _db.Reminder
                .Where(r => r.OwnerUserId == uid || sharedIds.Contains(r.Id))
                .ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Reminder>> GetReminder(Guid id)
        {
            var r = await _db.Reminder.FindAsync(id);
            if (r == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Reminder, r.OwnerUserId))
                return Forbid();

            return r;
        }

        public record ReminderCreateDto(string Message, DateTime ReminderTime, string[]? ShareWithUserNames);

        [HttpPost]
        public async Task<ActionResult<Reminder>> PostReminder(ReminderCreateDto dto)
        {
            var r = new Reminder
            {
                Message = dto.Message,
                ReminderTime = dto.ReminderTime,
                OwnerUserId = CurrentUserId,
                IsCompleted = false
            };

            _db.Reminder.Add(r);
            await _db.SaveChangesAsync();

            if (dto.ShareWithUserNames is { Length: > 0 })
                await Share(ResourceType.Reminder, r.Id, dto.ShareWithUserNames);

            return CreatedAtAction(nameof(GetReminder), new { id = r.Id }, r);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutReminder(Guid id, Reminder dto)
        {
            var current = await _db.Reminder.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (current == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Reminder, current.OwnerUserId))
                return Forbid();

            dto.Id = id;
            dto.OwnerUserId = current.OwnerUserId;

            _db.Entry(dto).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteReminder(Guid id)
        {
            var r = await _db.Reminder.FindAsync(id);
            if (r == null) return NotFound();

            if (r.OwnerUserId != CurrentUserId)
                return Forbid();

            _db.Reminder.Remove(r);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:guid}/share")]
        public async Task<IActionResult> ShareReminder(Guid id, [FromBody] string[] users)
        {
            var r = await _db.Reminder.FindAsync(id);
            if (r == null) return NotFound();

            if (r.OwnerUserId != CurrentUserId)
                return Forbid();

            await Share(ResourceType.Reminder, id, users);
            return Ok();
        }

        [HttpPost("{id:guid}/unshare")]
        public async Task<IActionResult> UnshareReminder(Guid id, [FromBody] string[] users)
        {
            var r = await _db.Reminder.FindAsync(id);
            if (r == null) return NotFound();

            if (r.OwnerUserId != CurrentUserId)
                return Forbid();

            var uids = await _db.User.Where(u => users.Contains(u.UserName))
                                     .Select(u => u.Id)
                                     .ToListAsync();

            var rows = _db.SharedAccess.Where(s =>
                s.ResourceType == ResourceType.Reminder &&
                s.ResourceId == id &&
                uids.Contains(s.GranteeUserId));

            _db.SharedAccess.RemoveRange(rows);
            await _db.SaveChangesAsync();

            return Ok();
        }

        private async Task<bool> CanAccess(Guid resId, ResourceType type, Guid ownerId)
        {
            if (ownerId == CurrentUserId)
                return true;

            return await _db.SharedAccess.AnyAsync(s =>
                s.ResourceType == type &&
                s.ResourceId == resId &&
                s.GranteeUserId == CurrentUserId);
        }

        private async Task Share(ResourceType type, Guid id, string[] users)
        {
            var uids = await _db.User.Where(u => users.Contains(u.UserName))
                                     .Select(u => u.Id)
                                     .ToListAsync();

            foreach (var uid in uids)
            {
                _db.SharedAccess.Add(new SharedAccess
                {
                    ResourceType = type,
                    ResourceId = id,
                    GranteeUserId = uid
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
