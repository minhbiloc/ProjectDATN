using BigProject.DataContext;
using BigProject.PayLoad.Converter;
using BigProject.PayLoad.DTO;
using BigProject.Payload.Response;
using BigProject.Service.Interface;
using BigProject.PayLoad.Request;
using BigProject.Entities;
using Microsoft.EntityFrameworkCore;
using BigProject.Helper;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace BigProject.Service.Implement
{
    public class Service_Event : IService_Event
    {
        private readonly AppDbContext dbContext;
        private readonly ResponseObject<DTO_Event> responseObject;
        private readonly ResponseObject<DTO_EventJoin> responseObjectEventJoin;
        private readonly Converter_Event converter_Event;
        private readonly Converter_EventJoin converter_EventJoin;
        private readonly ResponseBase responseBase;

        public Service_Event(AppDbContext dbContext, ResponseObject<DTO_Event> responseObject, ResponseObject<DTO_EventJoin> responseObjectEventJoin, Converter_Event converter_Event, Converter_EventJoin converter_EventJoin, ResponseBase responseBase)
        {
            this.dbContext = dbContext;
            this.responseObject = responseObject;
            this.responseObjectEventJoin = responseObjectEventJoin;
            this.converter_Event = converter_Event;
            this.converter_EventJoin = converter_EventJoin;
            this.responseBase = responseBase;
        }

        public async Task<ResponseObject<DTO_Event>> AddEvent(Request_AddEvent request)
        {
            //var eventType_check = await dbContext.eventTypes.FirstOrDefaultAsync(x => x.Id == request.EventTypeId);
            //if (eventType_check == null)
            //{
            //    return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, " Loại hoạt động không tồn tại! ", null);
            //}
            var eventName_check = await dbContext.events
                .FirstOrDefaultAsync(x => x.EventName.Equals(request.EventName));

            if (eventName_check != null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, " Tên hoạt động không được trùng! ", null);
            }
            string UrlAvt = null;
            var cloudinary = new CloudinaryService();
            if (request.UrlAvatar == null)
            {
                UrlAvt = "https://media.istockphoto.com/id/1300845620/vector/user-icon-flat-isolated-on-white-background-user-symbol-vector-illustration.jpg?s=612x612&w=0&k=20&c=yBeyba0hUkh14_jgv1OKqIH0CCSWU_4ckRkAoy2p73o=";
            }
            else
            {
                if (!CheckInput.IsImage(request.UrlAvatar))
                {
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Định dạng ảnh không hợp lệ !", null);
                }

                UrlAvt = await cloudinary.UploadImage(request.UrlAvatar);
            }
            var event1 = new Event();
            event1.EventName = request.EventName;
            event1.EventLocation = request.EventLocation;
            event1.EventStartDate = request.EventStartDate;
            event1.EventEndDate = request.EventEndDate;
            event1.Description = request.Description;
            //event1.EventTypeId = request.EventTypeId;
            event1.UrlAvatar = UrlAvt;
            dbContext.events.Add(event1);
            await dbContext.SaveChangesAsync();
            return responseObject.ResponseObjectSuccess("Thêm thành công!", converter_Event.EntityToDTO(event1));
        }

        public async Task<ResponseBase> DeleteEvent(int eventId)
        {
            var event1 = await dbContext.events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (event1 == null)
            {
                return responseBase.ResponseBaseError(StatusCodes.Status404NotFound,"Hoạt động không tồn tại!");
            }
            dbContext.events.Remove(event1);
            await dbContext.SaveChangesAsync();
            return responseBase.ResponseBaseSuccess("Xóa thành công!");
        }

        public PagedResult<DTO_Event> GetListEvent(int pageSize, int pageNumber)
        {
            var query = dbContext.events;

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => converter_Event.EntityToDTO(x))
                .ToList(); // Chuyển thành List<T>

            return new PagedResult<DTO_Event>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };
        }


        public async Task<ResponseObject<DTO_Event>> UpdateEvent(Request_UpdateEvent request)
        {
            var event1 = await dbContext.events.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (event1 == null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Hoạt động không tồn tại!", null);
            }
            //var eventType_check = await dbContext.eventTypes.FirstOrDefaultAsync(x => x.Id == request.EventTypeId);
            //if (eventType_check == null)
            //{
            //    return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Loại hoạt động không tồn tại!", null);
            //}
            var eventName_check = await dbContext.events
                .FirstOrDefaultAsync(x => x.EventName.Equals(request.EventName));

            if (eventName_check != null && !event1.EventName.Equals(request.EventName))
            {
                return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Tên hoạt động không được trùng! ", null);
            }
            string UrlAvt = null;
            var cloudinary = new CloudinaryService();
            if (request.UrlAvatar == null)
            {
                UrlAvt = "https://media.istockphoto.com/id/1300845620/vector/user-icon-flat-isolated-on-white-background-user-symbol-vector-illustration.jpg?s=612x612&w=0&k=20&c=yBeyba0hUkh14_jgv1OKqIH0CCSWU_4ckRkAoy2p73o=";
            }
            else
            {
                if (!CheckInput.IsImage(request.UrlAvatar))
                {
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Định dạng ảnh không hợp lệ !", null);
                }

                UrlAvt = await cloudinary.UploadImage(request.UrlAvatar);
            }
            event1.EventName = request.EventName;
            event1.EventLocation = request.EventLocation;
            event1.EventStartDate = request.EventStartDate;
            event1.EventEndDate = request.EventEndDate;
            event1.Description = request.Description;
            //event1.EventTypeId = request.EventTypeId;
            event1.UrlAvatar = UrlAvt;
            dbContext.events.Update(event1);
            await dbContext.SaveChangesAsync();
            return responseObject.ResponseObjectSuccess("Thêm thành công!", converter_Event.EntityToDTO(event1));
        }

        public async Task<ResponseObject<DTO_EventJoin>> JoinAnEvent(int userId, int eventId)
        {
            var eventCheck = await dbContext.events.FirstOrDefaultAsync(x=>x.Id==eventId);
            if(eventCheck == null)
            {
                return responseObjectEventJoin.ResponseObjectError(StatusCodes.Status404NotFound, "Hoạt động này không tồn tại!",null);
            }
            var eventJoinCheck = await dbContext.eventJoins
                .FirstOrDefaultAsync(x=>x.EventId==eventId && x.UserId==userId);

            if(eventJoinCheck != null)
            {
                return responseObjectEventJoin.ResponseObjectError(StatusCodes.Status400BadRequest, "Bạn đã tham gia hoạt động này!", null);
            }
            var eventJoin = new EventJoin();
            eventJoin.EventId = eventId;
            eventJoin.UserId = userId;
            eventJoin.Status = Enums.EventJointEnum.registered;
            dbContext.eventJoins.Add(eventJoin);
            await dbContext.SaveChangesAsync();
            return responseObjectEventJoin.ResponseObjectSuccess("Tham gia thành công!", converter_EventJoin.EntityToDTO(eventJoin));
        }

        public async Task<ResponseBase> WithdrawFromAnEvent(int eventId, int userId)
        {
            var eventJoin = await dbContext.eventJoins
                .FirstOrDefaultAsync(x=>x.UserId == userId && x.EventId ==eventId);

            if(eventJoin == null)
            {
                return responseBase.ResponseBaseError(StatusCodes.Status404NotFound, "Bạn chưa tham gia hoạt động này!");
            }
            dbContext.eventJoins.Remove(eventJoin);
            await dbContext.SaveChangesAsync();
            return responseBase.ResponseBaseSuccess("Bỏ tham gia thành công!");
        }
        public PagedResult<DTO_EventJoin> GetListAllEventUserJoin(int pageSize, int pageNumber, int userId)
        {
            var query = dbContext.eventJoins.Where(x => x.UserId == userId);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => converter_EventJoin.EntityToDTO(x))
                .ToList(); // Chuyển thành List<T>

            return new PagedResult<DTO_EventJoin>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };
        }

        public PagedResult<DTO_EventJoin> GetListAllParticipantInAnEvent(int pageSize, int pageNumber, int eventId)
        {
            var query = dbContext.eventJoins.Where(x => x.EventId == eventId);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => converter_EventJoin.EntityToDTO(x))
                .ToList(); // Chuyển thành List<T>

            return new PagedResult<DTO_EventJoin>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };
        }

        public bool CheckStatus(int userId, int eventId)
        {
            var eventCheck = dbContext.eventJoins.FirstOrDefault(x=> x.UserId == userId && x.EventId == eventId);
            if (eventCheck == null || eventCheck.Status != Enums.EventJointEnum.registered) 
            {
                return false;
            }
            return true;
        }
    }
}
