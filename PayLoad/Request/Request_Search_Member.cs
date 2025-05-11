namespace BigProject.PayLoad.Request
{
    public class Request_Search_Member
    {
        public string? MaSV { get; set; }
        public string? Major { get; set; }
        public string? CourseIntake { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
