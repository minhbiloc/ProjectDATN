using Azure.Core;
using BigProject.DataContext;
using BigProject.Entities;
using BigProject.Helper;
using BigProject.Payload.Response;
using BigProject.PayLoad.Converter;
using BigProject.PayLoad.DTO;
using BigProject.PayLoad.Request;
using BigProject.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BigProject.Service.Implement
{
    public class Service_Authentic : IService_Authentic
    {
        private readonly AppDbContext dbContext;

        private readonly ResponseObject<DTO_Register> responseObject;
        private readonly Converter_Register converter_Register;
        private readonly ResponseBase responseBase;
        private readonly ResponseObject<DTO_Token> responseObjectToken;
        private readonly IConfiguration configuration;
        private readonly ResponseObject<List<DTO_Register>> responseObjectList;
        private readonly ResponseObject<DTO_Login> responseObjectLogin;
        private readonly Converter_Login converter_Login;

        public Service_Authentic(AppDbContext dbContext, ResponseObject<DTO_Register> responseObject, Converter_Register converter_Register, ResponseBase responseBase, ResponseObject<DTO_Token> responseObjectToken, IConfiguration configuration, ResponseObject<List<DTO_Register>> responseObjectList, ResponseObject<DTO_Login> responseObjectLogin, Converter_Login converter_Login)
        {
            this.dbContext = dbContext;
            this.responseObject = responseObject;
            this.converter_Register = converter_Register;
            this.responseBase = responseBase;
            this.responseObjectToken = responseObjectToken;
            this.configuration = configuration;
            this.responseObjectList = responseObjectList;
            this.responseObjectLogin = responseObjectLogin;
            this.converter_Login = converter_Login;
        }

        public async Task<ResponseBase> Activate(string Opt, string email)
        {
            var checkUser = await dbContext.users.FirstOrDefaultAsync(x => x.Email.Equals(email));
            var comfirmEmail = await dbContext.emailConfirms
                .Where(x => x.Code.Equals(Opt) && x.IsActiveAccount == true && x.UserId == checkUser.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (comfirmEmail == null)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận không đúng!");
            }

            if (comfirmEmail.Exprired < DateTime.Now)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận đã hết hạn!");
            }

            if (comfirmEmail.IsConfirmed)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận đã được sử dụng!");
            }

            comfirmEmail.IsConfirmed = true;
            dbContext.emailConfirms.Update(comfirmEmail);
            await dbContext.SaveChangesAsync();

            return responseBase.ResponseBaseSuccess("Kích hoạt tài khoản thành công!");
        }

        public async Task<ResponseObject<DTO_Register>> ForgotPassword(Request_forgot request)
        {
            var user = await dbContext.users.FirstOrDefaultAsync(x => x.Email.Equals(request.Email));
            if (user == null)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status404NotFound, "Email không tồn tại!", null);
            }

            var oldConfirm = await dbContext.emailConfirms
                .FirstOrDefaultAsync(x =>
                  x.UserId == user.Id && 
                  !x.IsConfirmed && 
                  x.IsActiveAccount == false);

            if (oldConfirm != null)
            {
                dbContext.emailConfirms.Remove(oldConfirm);
                await dbContext.SaveChangesAsync();
            }

            Random random = new Random();
            int code = random.Next(100000, 999999);
            EmailTo emailTo = new EmailTo();
            emailTo.Mail = request.Email;
            emailTo.Subject = "MÃ XÁC NHẬN QUÊN MẬT KHẨU";
            emailTo.Content = $"Mã xác nhận của bạn là: {code} mã sẽ hết hạn sau 5 phút!";
            await emailTo.SendEmailAsync(emailTo);

            EmailConfirm confirmEmail = new EmailConfirm();
            confirmEmail.UserId = user.Id;
            confirmEmail.Code = $"{code}";
            confirmEmail.Exprired = DateTime.Now.AddMinutes(5);
            confirmEmail.IsActiveAccount = false;
            dbContext.emailConfirms.Add(confirmEmail);
            await dbContext.SaveChangesAsync();

            return responseObject.ResponseObjectSuccess("Gửi thành công! Vui lòng kiểm tra email để lấy mã xác nhận.", null);
        }

        public async Task<ResponseObject<DTO_Token>> Login(Request_Login request)
        {
            var user = await dbContext.users.FirstOrDefaultAsync(x => 
                x.Username.Equals(request.UserName) || 
                x.Email.Equals(request.UserName) || 
                x.MaSV.Equals(request.UserName));

            if (user == null)
            {
                return responseObjectToken.ResponseObjectError(404, "Tài khoản không tồn tại!", null);
            }

            if (user.IsActive == false)
            {
                return responseObjectToken.ResponseObjectError(400, "Tài khoản đã bị đóng!", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return responseObjectToken.ResponseObjectError(400, "Sai mật khẩu!", null);
            }

            var activationEmail = await dbContext.emailConfirms
                .Where(x => x.UserId == user.Id && x.IsActiveAccount == true)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (activationEmail == null || !activationEmail.IsConfirmed)
            {
                if (activationEmail == null || activationEmail.Exprired < DateTime.Now)
                {
                    Random random = new Random();
                    int code = random.Next(100000, 999999);

                    EmailTo emailTo = new EmailTo();
                    emailTo.Mail = user.Email;
                    emailTo.Subject = "MÃ KÍCH HOẠT TÀI KHOẢN";
                    emailTo.Content = $"Mã kích hoạt tài khoản của bạn là: {code} mã sẽ hết hạn sau 5 phút!";
                    await emailTo.SendEmailAsync(emailTo);

                    if (activationEmail == null)
                    {
                        activationEmail = new EmailConfirm
                        {
                            UserId = user.Id,
                            Code = code.ToString(),
                            Exprired = DateTime.Now.AddMinutes(5),
                            IsActiveAccount = true
                        };
                        dbContext.emailConfirms.Add(activationEmail);
                    }
                    else
                    {
                        activationEmail.Code = code.ToString();
                        activationEmail.Exprired = DateTime.Now.AddMinutes(5);
                        dbContext.emailConfirms.Update(activationEmail);
                    }
                    await dbContext.SaveChangesAsync();
                }

                return responseObjectToken.ResponseObjectError(400, 
                    "Tài khoản chưa được kích hoạt! Vui lòng kiểm tra email và kích hoạt tài khoản.",
                      new DTO_Token { requireActivation = true,Email = user.Email });
            }

            return responseObjectToken.ResponseObjectSuccess("Đăng nhập thành công!", 
                GenerateAccessToken(user));
        }

        public async Task<ResponseObject<DTO_Register>> Register(Request_Register request)
        {
            var existingUser = await dbContext.users.Where(x => x.MaSV.Equals(request.MaSV)
            && x.Username.Equals(request.Username)
            && x.Email.Equals(request.Email))
            .FirstOrDefaultAsync();
            if (existingUser != null)
            {
                var existingCode = await dbContext.emailConfirms
                    .FirstOrDefaultAsync(x => 
                      x.UserId == existingUser.Id && 
                      x.IsActiveAccount == true && 
                      x.IsConfirmed == false);

                if (existingCode != null)
                {
                    var existingMemberInfo = await dbContext.memberInfos
                        .FirstOrDefaultAsync(x => x.UserId == existingUser.Id);

                    dbContext.memberInfos.Remove(existingMemberInfo);
                    dbContext.users.Remove(existingUser);
                    dbContext.emailConfirms.Remove(existingCode);
                    await dbContext.SaveChangesAsync();
                }
            }   

            var CheckUser = await dbContext.users
                            .Where(x => x.MaSV.Equals(request.MaSV)
                            || x.Username.Equals(request.Username)
                            || x.Email.Equals(request.Email))
                            .FirstOrDefaultAsync();

            if (CheckUser != null)
            {
                if (CheckUser.Username.Equals(request.Username))
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Tên tài khoản đã tồn tại!", null);

                if (CheckUser.MaSV.Equals(request.MaSV))
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Mã Sinh viên đã tồn tại!", null);

                if (CheckUser.Email.Equals(request.Email))
                    return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Email đã tồn tại!", null);
            }

            var checkEmail = CheckInput.IsValiEmail(request.Email);
            if (!checkEmail)
            {
                return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, "Email không hợp lệ (thiếu ký tự đặc biệt hoặc sai định dạng)!", null);
            }

            if (CheckInput.IsPassWord(request.Password) != request.Password)
                return responseObject.ResponseObjectError(StatusCodes.Status400BadRequest, CheckInput.IsPassWord(request.Password), null);

            var register = new User();
            register.MaSV = request.MaSV;
            register.Username = request.Username;
            register.Email = request.Email;
            register.IsActive = true;
            register.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            Random random = new Random();
            int code = random.Next(100000, 999999);
            EmailTo emailTo = new EmailTo();
            emailTo.Mail = request.Email;
            emailTo.Subject = "MÃ XÁC NHẬN !";
            emailTo.Content = $"Mã xác nhận của bạn là: {code} mã sẽ hết hạn sau 5 phút!";
            emailTo.SendEmailAsync(emailTo);

            register.Email = request.Email;
            register.RoleId = 1;
            dbContext.users.Add(register);
            await dbContext.SaveChangesAsync();

            var user = await dbContext.users
                .FirstOrDefaultAsync(x => x.Username.Equals(request.Username));

            var memberInfo = new MemberInfo();
            memberInfo.UserId = user.Id;
            memberInfo.FullName = request.FullName;
            memberInfo.UrlAvatar = "https://media.istockphoto.com/id/1300845620/vector/user-icon-flat-isolated-on-white-background-user-symbol-vector-illustration.jpg?s=612x612&w=0&k=20&c=yBeyba0hUkh14_jgv1OKqIH0CCSWU_4ckRkAoy2p73o=";
            memberInfo.CourseIntake = (DateTime.Now.Year - 1955).ToString();
            dbContext.memberInfos.Add(memberInfo);

            EmailConfirm comfirmEmail = new EmailConfirm();
            comfirmEmail.UserId = register.Id;
            comfirmEmail.Code = $"{code}";
            comfirmEmail.Exprired = DateTime.Now.AddMinutes(5);
            comfirmEmail.IsActiveAccount = true;
            dbContext.emailConfirms.Add(comfirmEmail);
            await dbContext.SaveChangesAsync();

            return responseObject.ResponseObjectSuccess("Đăng kí thành công!", converter_Register.EntityToDTO(register));
        }

        public async Task<ResponseObject<DTO_Token>> RenewAccessToken(DTO_Token request)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:SecretKey").Value);

            var tokenValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:SecretKey").Value)),
                ValidateLifetime = false
            };

            try
            {
                var tokenAuthentication = jwtTokenHandler.ValidateToken(request.AccessToken, tokenValidation, out var validatedToken);
                if (validatedToken is not JwtSecurityToken jwtSecurityToken || jwtSecurityToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                {
                    return responseObjectToken.ResponseObjectError(StatusCodes.Status400BadRequest, "Token không hợp lệ!", null);
                }

                var expClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
                if (expClaim != null && long.TryParse(expClaim, out long exp))
                {
                    var tokenExpiry = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    if (tokenExpiry > DateTime.UtcNow)
                    {
                        return responseObjectToken.ResponseObjectError(StatusCodes.Status400BadRequest, "AccessToken chưa hết hạn!", null);
                    }
                }

                RefreshToken refreshToken = await dbContext.refreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
                if (refreshToken == null)
                {
                    return responseObjectToken.ResponseObjectError(StatusCodes.Status404NotFound, "RefreshToken không tồn tại trong database!", null);
                }

                if (refreshToken.Exprited < DateTime.Now)
                {
                    return responseObjectToken.ResponseObjectError(StatusCodes.Status401Unauthorized, "Token dã hết hạn!", null);
                }

                var user = dbContext.users.FirstOrDefault(x => x.Id == refreshToken.UserId);
                if (user == null)
                {
                    return responseObjectToken.ResponseObjectError(StatusCodes.Status404NotFound, "Người dùng không tồn tại!", null);
                }

                var newToken = GenerateAccessToken(user);

                refreshToken.Token = GenerateRefreshToken();
                refreshToken.Exprited = DateTime.UtcNow.AddDays(7);
                dbContext.refreshTokens.Update(refreshToken);
                await dbContext.SaveChangesAsync();

                return responseObjectToken.ResponseObjectSuccess("Làm mới token thành công!", newToken);
            }
            catch (Exception ex)
            {
                return responseObjectToken.ResponseObjectError(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        private DTO_Token GenerateAccessToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:SecretKey").Value);

            var decentralization = dbContext.roles.FirstOrDefault(x => x.Id == user.RoleId);

            var get_FullName = dbContext.memberInfos.FirstOrDefault(x => x.UserId == user.Id);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
             new Claim("username", user.Username),
             new Claim("Id", user.Id.ToString()),
             new Claim("MaSV", user.MaSV),
             new Claim("Email", user.Email),
             new Claim("RoleId", user.RoleId.ToString()),
             new Claim(ClaimTypes.Role, decentralization?.Name ?? ""),
             new Claim("FullName", get_FullName?.FullName ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            RefreshToken rf = new RefreshToken
            {
                Token = refreshToken,
                Exprited = DateTime.Now.AddDays(7),
                UserId = user.Id
            };

            dbContext.refreshTokens.Add(rf);
            dbContext.SaveChanges();

            DTO_Token tokenDTO = new DTO_Token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return tokenDTO;
        }

        public async Task<ResponseBase> ChangePassword(Request_ChangePassword requset, int userId)
        {
            var change = await dbContext.users.FirstOrDefaultAsync(x => x.Id == userId);

            if (!BCrypt.Net.BCrypt.Verify(requset.Password, change.Password))
            {
                return responseBase.ResponseBaseError(404, "Mật khẩu không chính xác!");
            }

            if (!requset.newpassword.Equals(requset.renewpassword))
            {
                return responseBase.ResponseBaseError(400, "Mật khẩu không trùng nhau!");
            }

            if (requset.newpassword.Equals(requset.Password))
            {
                return responseBase.ResponseBaseError(400, "Mật khẩu mới trùng mật khẩu cũ!");
            }

            if (!CheckInput.IsPassWord(requset.newpassword).Equals(requset.newpassword))
            {
                return responseBase.ResponseBaseError(404, CheckInput.IsPassWord(requset.newpassword));
            }

            change.Password = BCrypt.Net.BCrypt.HashPassword(requset.newpassword);
            dbContext.users.Update(change);
            await dbContext.SaveChangesAsync();

            return responseBase.ResponseBaseSuccess("Đổi mật khẩu thành công!");
        }

        public async Task<ResponseObject<List<DTO_Register>>> Authorization(int RoleId)
        {
            var listUserForRoleInput = dbContext.users
                .Include(user => user.Role)
                .AsNoTracking()
                .Where(user => user.Role.Id == RoleId);

            if (!listUserForRoleInput.Any())
            {
                return responseObjectList.ResponseObjectError(StatusCodes.Status404NotFound, "Bảng không tồn tại!", null);
            }
            return responseObjectList.ResponseObjectSuccess("Hiện thành công!", listUserForRoleInput.Select(x => converter_Register.EntityToDTO(x)).ToList());
        }

        public PagedResult<DTO_Register> GetListMember(int pageSize, int pageNumber)
        {
            int totalItems = dbContext.users.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = dbContext.users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => converter_Register.EntityToDTO(x))
                .ToList();

            return new PagedResult<DTO_Register>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };
        }

        public ResponseBase Activate_Password(string code, string email)
        {
            var user = dbContext.users.FirstOrDefault(x => x.Email.Equals(email));
            if (user == null)
            {
                return responseBase.ResponseBaseError(404, "Email không tồn tại!");
            }

            var confirmEmailCheck = dbContext.emailConfirms
                .FirstOrDefault(x => x.Code.Equals(code) && 
                                    x.IsActiveAccount == false && 
                                    x.UserId == user.Id);

            if (confirmEmailCheck == null)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận không đúng!");
            }

            if (confirmEmailCheck.Exprired < DateTime.Now)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận đã hết hạn!");
            }

            if (confirmEmailCheck.IsConfirmed)
            {
                return responseBase.ResponseBaseError(400, "Mã xác nhận đã được sử dụng!");
            }

            confirmEmailCheck.IsConfirmed = true;
            dbContext.SaveChanges();

            string newPassword = GenerateRandomPassword(8);

            EmailTo emailTo = new EmailTo
            {
                Mail = email,
                Subject = "MẬT KHẨU MỚI",
                Content = $"Mật khẩu mới của bạn là: {newPassword}"
            };
            emailTo.SendEmailAsync(emailTo);

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            dbContext.users.Update(user);
            dbContext.SaveChanges();

            return responseBase.ResponseBaseSuccess("Đã gửi mật khẩu mới qua Email!");
        }

        public static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<ResponseObject<object>> DecodeJwtTokenAsync(string token)
        {
            {
                var response = new ResponseObject<object>();

                if (string.IsNullOrWhiteSpace(token))
                {
                    return await Task.FromResult(response.ResponseObjectError(StatusCodes.Status400BadRequest, "Token không được để trống!", null));
                }

                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

                    var expClaim = claims.FirstOrDefault(c => c.Key == "exp").Value;
                    if (!string.IsNullOrEmpty(expClaim))
                    {
                        var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
                        if (expDate < DateTime.UtcNow)
                        {
                            return response.ResponseObjectError(StatusCodes.Status401Unauthorized, "Token đã hết hạn!", null);
                        }
                    }

                    var data = new Dictionary<string, string>();
                    if (claims.ContainsKey("Id")) data["Id"] = claims["Id"];
                    if (claims.ContainsKey("username")) data["Username"] = claims["username"];
                    if (claims.ContainsKey("FullName")) data["FullName"] = claims["FullName"];
                    if (claims.ContainsKey("MaSV")) data["MaSV"] = claims["MaSV"];
                    if (claims.ContainsKey("Email")) data["Email"] = claims["Email"];
                    if (claims.ContainsKey("RoleId")) data["RoleId"] = claims["RoleId"];

                    return await Task.FromResult(response.ResponseObjectSuccess("Giải mã token thành công!", data));
                }
                catch
                {
                    return await Task.FromResult(response.ResponseObjectError(StatusCodes.Status400BadRequest, "Token không hợp lệ hoặc bị lỗi!", null));
                }
            }
        }
    }
}