using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BigProject.DataContext;

namespace BigProject.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly AppDbContext _dbContext;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        public TokenValidationMiddleware(RequestDelegate next,
            ILogger<TokenValidationMiddleware> logger)
            //AppDbContext dbContext)
        {
            _next = next;
            _logger = logger;
            //_dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var authAttribute = endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>();

            if (authAttribute == null)
            {
                await _next(context);
                return;
            }

            try
            {
                // Trích xuất và xác thực token.
                if (!TryExtractToken(context, out string token))
                    return;

              /*  // Kiểm tra token đã bị vô hiệu hóa hay chưa.
                if (await IsTokenInvalidated(_dbContext, token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Token đã hết hạn hoặc đã bị vô hiệu hóa" });
                    return;
                }*/

                // Xác thực quyền truy cập của admin nếu cần.
                var adminAttribute = endpoint?.Metadata?.GetMetadata<AdminAuthorizeAttribute>();
                if (adminAttribute != null && !IsAdmin(context))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "Bạn không có quyền truy cập chức năng này" });
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi xác thực token");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "Đã xảy ra lỗi trong quá trình xác thực token" });
            }
        }

        private static bool TryExtractToken(HttpContext context, out string token)
        {
            token = null;
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.WriteAsJsonAsync(new
                {
                    message = authHeader == null
                    ? "Bạn cần đăng nhập để truy cập API này"
                    : "Định dạng header Authorization không hợp lệ"
                }).Wait();
                return false;
            }

            token = authHeader.Substring("Bearer ".Length).Trim();
            return true;
        }

        /*private static async Task<bool> IsTokenInvalidated( string token)
        {
            var invalidatedTokenRepo = appUOW.GetRepository<InvalidatedToken, ObjectId>();
            return await invalidatedTokenRepo.Get(
                Builders<InvalidatedToken>.Filter.Eq(t => t.Token, token)
            ) != null;
        }*/

        private static bool IsAdmin(HttpContext context) =>
            context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Liên chi đoàn khoa";

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!IsAdmin(context.HttpContext))
                    context.Result = new ForbidResult();
            }
        }

    }
}
