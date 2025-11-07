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
    public class NotesController : ControllerBaseEx
    {
        private readonly ComprehensionContext _db;
        public NotesController(ComprehensionContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNote()
        {
            var uid = CurrentUserId;

            var sharedIds = await _db.SharedAccess
                .Where(s => s.ResourceType == ResourceType.Note && s.GranteeUserId == uid)
                .Select(s => s.ResourceId)
                .ToListAsync();

            return await _db.Note
                .Where(n => n.OwnerUserId == uid || sharedIds.Contains(n.Id))
                .ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Note>> GetNote(Guid id)
        {
            var note = await _db.Note.FindAsync(id);
            if (note == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Note, note.OwnerUserId))
                return Forbid();

            return note;
        }

        public record NoteCreateDto(string Title, string Content, string[]? ShareWithUserNames);

        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(NoteCreateDto dto)
        {
            var now = DateTime.UtcNow;

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = now,
                UpdatedAt = now,
                OwnerUserId = CurrentUserId
            };

            _db.Note.Add(note);
            await _db.SaveChangesAsync();

            if (dto.ShareWithUserNames is { Length: > 0 })
                await Share(ResourceType.Note, note.Id, dto.ShareWithUserNames);

            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutNote(Guid id, Note dto)
        {
            var current = await _db.Note.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (current == null) return NotFound();

            if (!await CanAccess(id, ResourceType.Note, current.OwnerUserId))
                return Forbid();

            dto.Id = id;
            dto.OwnerUserId = current.OwnerUserId;
            dto.CreatedAt = current.CreatedAt;
            dto.UpdatedAt = DateTime.UtcNow;

            _db.Entry(dto).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            var note = await _db.Note.FindAsync(id);
            if (note == null) return NotFound();

            if (note.OwnerUserId != CurrentUserId)
                return Forbid();

            _db.Note.Remove(note);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:guid}/share")]
        public async Task<IActionResult> ShareNote(Guid id, [FromBody] string[] users)
        {
            var note = await _db.Note.FindAsync(id);
            if (note == null) return NotFound();

            if (note.OwnerUserId != CurrentUserId)
                return Forbid();

            await Share(ResourceType.Note, id, users);
            return Ok();
        }

        [HttpPost("{id:guid}/unshare")]
        public async Task<IActionResult> UnshareNote(Guid id, [FromBody] string[] users)
        {
            var note = await _db.Note.FindAsync(id);
            if (note == null) return NotFound();

            if (note.OwnerUserId != CurrentUserId)
                return Forbid();

            var uids = await _db.User.Where(u => users.Contains(u.UserName))
                                     .Select(u => u.Id).ToListAsync();

            var rows = _db.SharedAccess
                .Where(s => s.ResourceType == ResourceType.Note &&
                            s.ResourceId == id &&
                            uids.Contains(s.GranteeUserId));

            _db.SharedAccess.RemoveRange(rows);
            await _db.SaveChangesAsync();

            return Ok();
        }

        private async Task<bool> CanAccess(Guid resourceId, ResourceType type, Guid ownerId)
        {
            if (ownerId == CurrentUserId)
                return true;

            return await _db.SharedAccess.AnyAsync(s =>
                s.ResourceType == type &&
                s.ResourceId == resourceId &&
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
