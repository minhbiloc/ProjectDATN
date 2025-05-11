using BigProject.Enums;

namespace BigProject.PayLoad.DTO
{
    public class DTO_EventJoin
    {
        public int Id { get; set; }

        public string CourseIntake { get; set; }
        public string EventName { get; set; }
        public string FullName { get; set; }
        public string MaSV { get; set; }
        public string Class {  get; set; }
        public string Major {  get; set; }
        public EventJointEnum Status { get; set; } = EventJointEnum.registered;
    }
}
