using BigProject.PayLoad.DTO;
using BigProject.PayLoad.Request;
using BigProject.Payload.Response;
using BigProject.Entities;

namespace BigProject.Service.Interface
{
    public interface IService_MemberInfo
    {
        //Task<ResponseObject<DTO_MemberInfo>> AddMenberInfo(Request_AddMemberInfo request, int userId);
        Task<ResponseObject<DTO_MemberInfo>> UpdateMenberInfo(Request_UpdateMemberInfo request, int userId);
        Task<ResponseObject<DTO_MemberInfo>> UpdateUserImg(IFormFile? UrlAvatar, int userId);
        PagedResult<DTO_MemberInfo> GetListMenberInfo(int pageSize, int pageNumber);
        Task<ResponseObject<DTO_MemberInfo>> GetMemberInfo(int userId);
        Task<ResponseObject<PagedResult<DTO_MemberInfo>>> SearchMembers(Request_Search_Member request);
    }
}
