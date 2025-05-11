using BigProject.DataContext;
using BigProject.Entities;
using BigProject.PayLoad.DTO;

namespace BigProject.PayLoad.Converter
{
    public class Converter_Event
    {
        private readonly AppDbContext _context;

        public Converter_Event(AppDbContext context)
        {
            _context = context;
        }
        public DTO_Event EntityToDTO(Event event1)
        {
            return new DTO_Event
            {
                Id = event1.Id,
                Description = event1.Description,
                EventEndDate = event1.EventEndDate,
                EventLocation = event1.EventLocation,
                EventName = event1.EventName,
                EventStartDate = event1.EventStartDate,
                UrlAvatar = event1.UrlAvatar,
                //EventType = _context.eventTypes.SingleOrDefault(x => x.Id == event1.EventTypeId).EventTypeName,
            };
        }
    }
}
