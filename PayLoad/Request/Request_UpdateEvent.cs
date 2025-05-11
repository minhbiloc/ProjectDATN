namespace BigProject.PayLoad.Request
{
    public class Request_UpdateEvent
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        //public int EventTypeId { get; set; }
        public IFormFile? UrlAvatar { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string EventLocation { get; set; }
    }
}
