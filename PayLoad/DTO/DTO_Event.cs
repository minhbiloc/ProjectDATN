namespace BigProject.PayLoad.DTO
{
    public class DTO_Event
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        //public string EventType { get; set; }
        public string UrlAvatar { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string EventLocation { get; set; }
    }
}
