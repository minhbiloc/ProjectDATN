using Azure.Core;
using BigProject.PayLoad.Request;
using BigProject.Service.Implement;
using BigProject.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigProject.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class Controller_Event : ControllerBase
    {
        private readonly IService_Event service_Event;

        public Controller_Event(IService_Event service_Event)
        {
            this.service_Event = service_Event;
        }

        [HttpPost("Add_Event")]
        [Authorize(Roles = "Liên chi đoàn khoa")]
        public async Task<IActionResult> AddEvent([FromForm] Request_AddEvent request)
        {
            return Ok(await service_Event.AddEvent(request));
        }

        [HttpPut("Update_event")]
        [Authorize(Roles = "Liên chi đoàn khoa")]
        public async Task<IActionResult> UpdateEvent([FromForm] Request_UpdateEvent request)
        {
            return Ok(await service_Event.UpdateEvent(request));
        }

        [HttpDelete("Delete_Event")]
        [Authorize(Roles = "Liên chi đoàn khoa")]
        public async Task<IActionResult> DeleteEvent([FromForm] int eventId)
        {
            return Ok(await service_Event.DeleteEvent(eventId));
        }

        [HttpGet("Get_List_Event")]

        public IActionResult GetListProductFull( int pageSize = 10, int pageNumber = 1)
        {
            return Ok(service_Event.GetListEvent(pageSize, pageNumber));
        }

        [HttpPost("Sign_up_for_the_activity")]
        public async Task<IActionResult> JoinAnEvent([FromForm] int eventId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return  Ok("Vui lòng đăng nhập !");
            }
            int userId = int.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await service_Event.JoinAnEvent(userId, eventId));
        }

        [HttpDelete("Unsubscribe_from_activities")]
        public async Task<IActionResult> WithdrawFromAnEvent([FromForm] int eventId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok("Vui lòng đăng nhập !");
            }
            int userId = int.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await service_Event.WithdrawFromAnEvent(eventId,userId));
        }
        
        [HttpGet("Get_List_All_Participant_In_An_Event")]
       
        public IActionResult GetListAllParticipantInAnEvent( int eventId,int pageSize = 10, int pageNumber = 1)
        {
            return Ok(service_Event.GetListAllParticipantInAnEvent(pageSize, pageNumber,eventId));
        }

        [HttpGet("Get_List_All_Event_User_Join")]
 
        public IActionResult GetListAllEventUserJoin( int pageSize = 10, int pageNumber = 1)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok("Vui lòng đăng nhập !");
            }
            int userId = int.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(service_Event.GetListAllEventUserJoin(pageSize, pageNumber, userId));
        }

        [HttpGet("Check_EventJoint_Status")]
        public IActionResult CheckUserEventStatus(int eventId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok("Vui lòng đăng nhập !");
            }
            int userId = int.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(service_Event.CheckStatus(userId,eventId));
        }

    }
}
