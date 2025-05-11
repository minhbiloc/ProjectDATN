using BigProject.Enums;

namespace BigProject.Entities
{
    public class MemberInfo : EntityBase
    {
        public string? CourseIntake { get; set; }
        public string? Class { get; set; }
        public string? Gender { get; set; }
        public string? Major {  get; set; }
        public DateTime? Birthdate { get; set; } = null;
        public string? FullName { get; set; }
        public string? Nation { get; set; }  
        public string? religion { get; set; }
        public string? PhoneNumber { get; set; }
        public string UrlAvatar { get; set; }
        public string? PoliticalTheory{ get; set; }
        public DateTime? DateOfJoining { get; set; } = null;
        public string? PlaceOfJoining { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

    }
}   
