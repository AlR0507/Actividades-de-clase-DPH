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
    public class EventsController : ControllerBaseEx
    {
        private readonly ComprehensionContext _db;
        public EventsController(ComprehensionContext ctx) => _db = ctx;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvent()
        {
            var uid = CurrentUserId;

            var sharedIds = await _db.SharedAccess
                .Where(s => s.ResourceType == ResourceType.Event && s.GranteeUserId == uid)
                .Select(s => s.ResourceId)
                .ToListAsync();

            return await _db.Event
                .Where(e => e.OwnerUserId == uid || sharedIds.Contains(e.Id))
                .ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Event>> GetEvent(Guid id)
        {
            var e = await _db.Event.FindAsync(id);
            if (e == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Event, e.OwnerUserId))
                return Forbid();

            return e;
        }

        public record EventCreateDto(string Title, string Description, DateTime StartTime, DateTime EndTime, string[]? ShareWithUserNames);

        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(EventCreateDto dto)
        {
            var e = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                OwnerUserId = CurrentUserId
            };

            _db.Event.Add(e);
            await _db.SaveChangesAsync();

            if (dto.ShareWithUserNames is { Length: > 0 })
                await Share(ResourceType.Event, e.Id, dto.ShareWithUserNames);

            return CreatedAtAction(nameof(GetEvent), new { id = e.Id }, e);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutEvent(Guid id, Event dto)
        {
            var current = await _db.Event.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (current == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Event, current.OwnerUserId))
                return Forbid();

            dto.Id = id;
            dto.OwnerUserId = current.OwnerUserId;

            _db.Entry(dto).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var e = await _db.Event.FindAsync(id);
            if (e == null) return NotFound();

            if (e.OwnerUserId != CurrentUserId)
                return Forbid();

            _db.Event.Remove(e);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:guid}/share")]
        public async Task<IActionResult> ShareEvent(Guid id, [FromBody] string[] users)
        {
            var e = await _db.Event.FindAsync(id);
            if (e == null) return NotFound();

            if (e.OwnerUserId != CurrentUserId)
                return Forbid();

            await Share(ResourceType.Event, id, users);
            return Ok();
        }

        [HttpPost("{id:guid}/unshare")]
        public async Task<IActionResult> UnshareEvent(Guid id, [FromBody] string[] users)
        {
            var e = await _db.Event.FindAsync(id);
            if (e == null) return NotFound();

            if (e.OwnerUserId != CurrentUserId)
                return Forbid();

            var uids = await _db.User.Where(u => users.Contains(u.UserName))
                                     .Select(u => u.Id)
                                     .ToListAsync();

            var rows = _db.SharedAccess.Where(s =>
                s.ResourceType == ResourceType.Event &&
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
