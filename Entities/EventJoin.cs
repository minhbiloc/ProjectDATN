using BigProject.Enums;

namespace BigProject.Entities
{
    public class EventJoin : EntityBase
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public EventJointEnum Status { get; set; } = EventJointEnum.registered;
        public Event Event { get; set; }
        public User User { get; set; }
    }
}
