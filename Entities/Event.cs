namespace BigProject.Entities
{
    public class Event : EntityBase
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public string UrlAvatar { get; set; }
        //public int EventTypeId { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string EventLocation { get; set; }
        //public EventType EventType { get; set; }
        public ICollection<EventJoin> eventJoints { get; set; }
    }
}
