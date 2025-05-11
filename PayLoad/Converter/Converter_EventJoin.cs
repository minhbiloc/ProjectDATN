using BigProject.DataContext;
using BigProject.Entities;
using BigProject.PayLoad.DTO;
using Microsoft.EntityFrameworkCore;

namespace BigProject.PayLoad.Converter
{
    public class Converter_EventJoin
    {
        private readonly AppDbContext _context;

        public Converter_EventJoin(AppDbContext context)
        {
            _context = context;
        }
        public DTO_EventJoin EntityToDTO(EventJoin eventJoint) 
        {
            var memberInfo = _context.memberInfos.SingleOrDefault(x => x.UserId == eventJoint.UserId);
            return new DTO_EventJoin()
            {
                 Id = eventJoint.Id,
                CourseIntake = memberInfo.CourseIntake,
                Class = memberInfo.Class,
                EventName = _context.events.SingleOrDefault(x => x.Id == eventJoint.EventId).EventName,
                FullName = memberInfo.FullName,
                MaSV = _context.users.SingleOrDefault(x => x.Id == eventJoint.UserId).MaSV,
                Major = memberInfo.Major
            };
        }
    }
}
