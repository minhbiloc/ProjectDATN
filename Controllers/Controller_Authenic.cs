using BigProject.Entities;
using BigProject.PayLoad.Request;
using BigProject.Service.Implement;
using BigProject.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Core;
using BigProject.PayLoad.DTO;
namespace BigProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Controller_Authenic : ControllerBase
    {
        private readonly IService_Authentic service_Authentic;

        public Controller_Authenic(IService_Authentic service_Authentic)
        {
            this.service_Authentic = service_Authentic;
        }

        [HttpGet("Decode_Token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DecodeToken([FromQuery] string token)
        {
            
            return Ok(await service_Authentic.DecodeJwtTokenAsync(token));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] Request_Register request)
        {
            return  Ok(await service_Authentic.Register(request));
        }

        [HttpPut("Forget_Password")]
        public async Task<IActionResult> ForgotPassword([FromForm] Request_forgot request)
        {
            return  Ok(await service_Authentic.ForgotPassword(request));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] Request_Login request)
        {
            return Ok(await service_Authentic.Login(request));
        }

        [HttpPut("Active_Account")]
        public async Task<IActionResult> Activate([FromForm] string Opt,string email)
        {
            return Ok(await service_Authentic.Activate(Opt,email));
        }

        [HttpGet("Authorization")]
         [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Liên chi đoàn khoa")]
        public IActionResult Authorization([FromQuery] int RoleId)
        {
           
                return Ok(service_Authentic.Authorization(RoleId));
        }

        [HttpPut("Change_Password")]
        public async Task<IActionResult> ChangePassword([FromForm] Request_ChangePassword request)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok("Vui lòng đăng nhập !");
            }
            int userId = int.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await service_Authentic.ChangePassword(request,userId));
        }

        [HttpPut("Activate_Password")]  
        public IActionResult Activate_Password([FromForm] string code, string email)
        {
            return Ok(service_Authentic.Activate_Password(code,email));
        }
        private string HashOtp(string otp)  
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

      
        [HttpGet("Get_List_Member")]
        public IActionResult GetListMember(int pageSize = 10, int pageNumber = 1)
        {
            return Ok(service_Authentic.GetListMember(pageSize, pageNumber));
        }

        [HttpPost("renew_token")]
        public async Task<IActionResult> RenewToken([FromBody] DTO_Token request)
        {
            return Ok(await service_Authentic.RenewAccessToken(request));
        }
    }
        
  

}
