using System;

namespace Comprehension.Models
{
    public enum ResourceType { Note, Reminder, Event }

    public class SharedAccess
    {
        public Guid Id { get; set; }
        public ResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }     // Id del recurso compartido
        public Guid GranteeUserId { get; set; }  // Usuario que recibe acceso
    }
}
