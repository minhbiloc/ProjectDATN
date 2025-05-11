using BigProject.Enums;

namespace BigProject.PayLoad.DTO
{
    public class DTO_MemberInfo
    {
        public int Id { get; set; }
        public string CourseIntake { get; set; }
        public string Class { get; set; }
        public string Major { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string FullName { get; set; }    
        public string Nation { get; set; }
        public string religion { get; set; }
        public string PhoneNumber { get; set; }
        public string UrlAvatar { get; set; }
        public string? PoliticalTheory { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string PlaceOfJoining { get; set; }
        public string UserName { get; set; }
        public string MaSV { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string RoleName { get; set; }
    }
}
