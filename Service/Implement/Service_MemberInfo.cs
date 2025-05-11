using Azure;
using BigProject.DataContext;
using BigProject.Payload.Response;
using BigProject.PayLoad.Converter;
using BigProject.PayLoad.DTO;
using BigProject.PayLoad.Request;
using BigProject.Service.Interface;
using Microsoft.EntityFrameworkCore;
using BigProject.Entities;
using BigProject.Helper;
using Azure.Core;

namespace BigProject.Service.Implement
{
    public class Service_MemberInfo : IService_MemberInfo
    {
        private readonly AppDbContext DbContext;
        private readonly ResponseObject<DTO_MemberInfo> responseObject;
        private readonly Converter_MemberInfo converter_MemberInfo;
        private readonly ResponseBase responseBase;

        public Service_MemberInfo(AppDbContext DbContext, ResponseObject<DTO_MemberInfo> responseObject, Converter_MemberInfo converter_MemberInfo, ResponseBase responseBase)
        {
            this.DbContext = DbContext;
            this.responseObject = responseObject;
            this.converter_MemberInfo = converter_MemberInfo;
            this.responseBase = responseBase;
        }

        //public async Task<ResponseObject<DTO_MemberInfo>> AddMenberInfo(Request_AddMemberInfo request, int userId)
        //{
        //    var Check_UserId = await DbContext.users.FirstOrDefaultAsync(x => x.Id == userId);
        //    if (Check_UserId == null)
        //    {
        //        return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Đoàn viên không tồn tại", null);
        //    }
        //    string UrlAvt = null;
        //    var cloudinary = new CloudinaryService();
        //    if (request.UrlAvatar == null)
        //    {
        //        UrlAvt = "https://media.istockphoto.com/id/1300845620/vector/user-icon-flat-isolated-on-white-background-user-symbol-vector-illustration.jpg?s=612x612&w=0&k=20&c=yBeyba0hUkh14_jgv1OKqIH0CCSWU_4ckRkAoy2p73o=";
        //    }
        //    else
        //    {
        //        if (!CheckInput.IsImage(request.UrlAvatar))
        //        {
        //            return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Định dạng ảnh không hợp lệ !", null);
        //        }

        //        UrlAvt = await cloudinary.UploadImage(request.UrlAvatar);
        //    }
        //    var memberInfo = new MemberInfo();
        //    memberInfo.Class = request.Class;
        //    memberInfo.Birthdate = request.Birthdate;
        //    memberInfo.PhoneNumber = request.PhoneNumber;
        //    memberInfo.Nation = request.Nation;
        //    memberInfo.DateOfJoining = request.DateOfJoining;
        //    memberInfo.FullName = request.FullName;
        //    memberInfo.religion = request.religion;
        //    memberInfo.UrlAvatar = UrlAvt;
        //    memberInfo.PlaceOfJoining = request.PlaceOfJoining;
        //    memberInfo.PoliticalTheory = request.PoliticalTheory;
        //    memberInfo.UserId = userId;

        //    DbContext.memberInfos.Add(memberInfo);
        //    await DbContext.SaveChangesAsync();
        //    return responseObject.ResponseObjectSuccess("Thêm thành công", converter_MemberInfo.EntityToDTO(memberInfo));
        //}


        public PagedResult<DTO_MemberInfo> GetListMenberInfo(int pageSize, int pageNumber)
        {
            // Lọc chỉ lấy memberInfo có User đã xác nhận email
            var filteredMembers = DbContext.memberInfos
                .Where(x => DbContext.emailConfirms.Any(e => e.UserId == x.User.Id && e.IsConfirmed));

            // Tính tổng số phần tử sau khi lọc
            int totalItems = filteredMembers.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy danh sách đã phân trang
            var items = filteredMembers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => converter_MemberInfo.EntityToDTO(x))
                .ToList();
            // Chuyển thành List<T>

            return new PagedResult<DTO_MemberInfo>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };
        }

        public async Task<ResponseObject<DTO_MemberInfo>> GetMemberInfo(int userId)
        {
            var memberInfo = await DbContext.memberInfos.FirstOrDefaultAsync(x => x.UserId == userId);
            if (memberInfo == null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Đoàn viên không tồn tại", null);
            }
            return responseObject.ResponseObjectSuccess("Lấy thông tin thành công!", converter_MemberInfo.EntityToDTO(memberInfo));
        }

        public async Task<ResponseObject<PagedResult<DTO_MemberInfo>>> SearchMembers(Request_Search_Member request)
        {
            var listMembers = DbContext.memberInfos.AsQueryable(); // Danh sách chưa lọc

            listMembers = listMembers
                .Where(x => DbContext.emailConfirms
                .Any(e => e.UserId == x.User.Id && e.IsConfirmed)); //Bỏ qua tài khoản chưa xác nhận Email 

            // Lọc dữ liệu theo điều kiện
            if (!string.IsNullOrEmpty(request.MaSV))
                listMembers = listMembers.Where(x => x.User.MaSV.Equals(request.MaSV));
            if (!string.IsNullOrEmpty(request.FullName))
                listMembers = listMembers.Where(x => x.FullName.Contains(request.FullName));
            if (!string.IsNullOrEmpty(request.Email))
                listMembers = listMembers.Where(x => x.User.Email.Contains(request.Email));
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                listMembers = listMembers.Where(x => x.PhoneNumber.Equals(request.PhoneNumber));
            if (!string.IsNullOrEmpty(request.Major))
                listMembers = listMembers.Where(x => x.Major.Contains(request.Major));
            if (!string.IsNullOrEmpty(request.CourseIntake))
                listMembers = listMembers.Where(x => x.CourseIntake.Contains(request.CourseIntake));

            // Tổng số phần tử sau khi lọc
            int totalItems = await listMembers.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            // Lấy danh sách sau khi phân trang
            var items = await listMembers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => converter_MemberInfo.EntityToDTO(x))
                .ToListAsync();

            // Trả về kết quả dưới dạng `PagedResult<T>`
            var pagedResult = new PagedResult<DTO_MemberInfo>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = request.PageNumber
            };

            return new ResponseObject<PagedResult<DTO_MemberInfo>>().ResponseObjectSuccess("Danh sách Member:", pagedResult);
        }

        public async Task<ResponseObject<DTO_MemberInfo>> UpdateMenberInfo(Request_UpdateMemberInfo request, int userId)
        {
            var memberInfo = await DbContext.memberInfos.FirstOrDefaultAsync(x => x.UserId == userId);

            if (memberInfo == null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Đoàn viên không tồn tại", null);
            }

            // Chỉ cập nhật nếu request có dữ liệu, giữ lại giá trị cũ nếu request không có
            memberInfo.Class = request.Class ?? memberInfo.Class;
            memberInfo.Birthdate = request.Birthdate ?? memberInfo.Birthdate;
            memberInfo.PhoneNumber = request.PhoneNumber ?? memberInfo.PhoneNumber;
            memberInfo.Nation = request.Nation ?? memberInfo.Nation;
            memberInfo.DateOfJoining = request.DateOfJoining ?? memberInfo.DateOfJoining;
            memberInfo.FullName = request.FullName ?? memberInfo.FullName;
            memberInfo.religion = request.religion ?? memberInfo.religion;
            memberInfo.PlaceOfJoining = request.PlaceOfJoining ?? memberInfo.PlaceOfJoining;
            memberInfo.PoliticalTheory = request.PoliticalTheory ?? memberInfo.PoliticalTheory;
            memberInfo.Major = request.Major ?? memberInfo.Major;
            memberInfo.Gender = request.Gender ?? memberInfo.Gender;

            DbContext.memberInfos.Update(memberInfo);
            await DbContext.SaveChangesAsync();

            return responseObject.ResponseObjectSuccess("Cập nhật thành công", converter_MemberInfo.EntityToDTO(memberInfo));
        }

        public async Task<ResponseObject<DTO_MemberInfo>> UpdateUserImg(IFormFile? UrlAvatar, int userId)
        {
            var memberInfo = await DbContext.memberInfos.FirstOrDefaultAsync(x => x.UserId == userId);
            if (memberInfo == null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Đoàn viên không tồn tại", null);
            }
            string UrlAvt = null;
            var cloudinary = new CloudinaryService();
            if (UrlAvatar == null)
            {
                UrlAvt = "https://media.istockphoto.com/id/1300845620/vector/user-icon-flat-isolated-on-white-background-user-symbol-vector-illustration.jpg?s=612x612&w=0&k=20&c=yBeyba0hUkh14_jgv1OKqIH0CCSWU_4ckRkAoy2p73o=";
            }
            else
            {
                if (!CheckInput.IsImage(UrlAvatar))
                {
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Định dạng ảnh không hợp lệ !", null);
                }

                UrlAvt = await cloudinary.UploadImage(UrlAvatar);
            }
            memberInfo.UrlAvatar = UrlAvt;
            DbContext.memberInfos.Update(memberInfo);
            await DbContext.SaveChangesAsync();
            return responseObject.ResponseObjectSuccess("Thêm thành công", converter_MemberInfo.EntityToDTO(memberInfo));
        }
    }
}
