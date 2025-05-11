using BigProject.DataContext;
using BigProject.PayLoad.Converter;
using BigProject.PayLoad.DTO;
using BigProject.Payload.Response;
using BigProject.Service.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BigProject.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Swagger eShop Solution", Version = "v1" });
    x.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Làm theo mẫu này. Example: Bearer {Token} ",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
                          {
                            {
                              new OpenApiSecurityScheme
                              {
                                Reference = new OpenApiReference
                                  {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                  },
                                  Scheme = "oauth2",
                                  Name = "Bearer",
                                  In = ParameterLocation.Header,
                                },
                                new List<string>()
                              }
                            });
    //x.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("AppSettings:SecretKey").Value!))
    };
});




builder.Services.AddScoped<AppDbContext>();

builder.Services.AddScoped<ResponseBase>();


builder.Services.AddScoped<Converter_Register>();
builder.Services.AddScoped<Converter_Login>();
builder.Services.AddScoped<Converter_Event>();
builder.Services.AddScoped<Converter_MemberInfo>();
builder.Services.AddScoped<Converter_EventJoin>();


builder.Services.AddScoped<IService_Authentic, Service_Authentic>();
builder.Services.AddScoped<IService_Event, Service_Event>();
builder.Services.AddScoped<IService_MemberInfo, Service_MemberInfo>();

builder.Services.AddScoped<ResponseObject<DTO_Register>>();
builder.Services.AddScoped<ResponseObject<DTO_Login>>();
builder.Services.AddScoped<ResponseObject<DTO_Token>>();
builder.Services.AddScoped<ResponseObject<List<DTO_Register>>>();
builder.Services.AddScoped<ResponseObject<DTO_Event>>();
builder.Services.AddScoped<ResponseObject<DTO_MemberInfo>>();
builder.Services.AddScoped<ResponseObject<DTO_EventJoin>>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
