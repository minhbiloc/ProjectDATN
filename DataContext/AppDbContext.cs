using Azure.Core;
using BigProject.Entities;
using BigProject.Enums;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using System;

namespace BigProject.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mật khẩu là 123456789@A
            var password ="$2a$12$umDEKg3yORpv174r7kzKxO7Z.BVbw0HDzb44jCsvgjHGGn5rM6/Ky";
            var user1 = new User { Id =1 ,Username="admin",Email ="admin@gmail.com", Password = password, RoleId=3,MaSV="1111111111", IsActive = true, };
            var user2 = new User { Id = 2,Username = "member", Email = "member@gmail.com", Password = password, RoleId = 1, MaSV = "1111111112", IsActive = true, };
            var user3 = new User { Id = 3,Username = "secretary", Email = "secretary@gmail.com", Password = password, RoleId = 2, MaSV = "1111111113", IsActive = true, };
            modelBuilder.Entity<User>().HasData(user1, user2, user3);

            var user1MemberInfo = new MemberInfo { Id = 1, Class = "string", Birthdate = new DateTime(2025,1,1), FullName = "string", Nation = "string", religion = "string", PhoneNumber = "string", UrlAvatar = "string", PoliticalTheory = "string", DateOfJoining = new DateTime(2025, 1, 1), PlaceOfJoining = "string", UserId = 1 };
            var user2MemberInfo = new MemberInfo { Id = 2, Class = "string", Birthdate = new DateTime(2025, 1, 1), FullName = "string", Nation = "string", religion = "string", PhoneNumber = "string", UrlAvatar = "string", PoliticalTheory = "string", DateOfJoining = new DateTime(2025, 1, 1), PlaceOfJoining = "string",  UserId = 2 };
            var user3MemberInfo = new MemberInfo { Id = 3, Class = "string", Birthdate = new DateTime(2025, 1, 1), FullName = "string", Nation = "string", religion = "string", PhoneNumber = "string", UrlAvatar = "string", PoliticalTheory = "string", DateOfJoining = new DateTime(2025, 1, 1), PlaceOfJoining = "string",  UserId = 3 };
            modelBuilder.Entity<MemberInfo>().HasData(user1MemberInfo, user2MemberInfo, user3MemberInfo);

            var role1 = new Role { Id = 1, Name = "Đoàn viên" };
            var role2 = new Role { Id = 2, Name = "Bí thư đoàn viên" };
            var role3 = new Role { Id = 3, Name = "Liên chi đoàn khoa" };
            modelBuilder.Entity<Role>().HasData(role1,role2, role3);

            var user1AccountActive = new EmailConfirm { Id = 1, Code ="123456", IsActiveAccount=true , CreateTime= new DateTime(2025, 1, 1),IsConfirmed=true,Exprired = new DateTime(2025, 1, 1),UserId=1 };
            var user2AccountActive = new EmailConfirm { Id = 2, Code = "123456", IsActiveAccount = true, CreateTime = new DateTime(2025, 1, 1), IsConfirmed = true, Exprired = new DateTime(2025, 1, 1), UserId = 2 };
            var user3AccountActive = new EmailConfirm { Id = 3, Code = "123456", IsActiveAccount = true, CreateTime = new DateTime(2025, 1, 1), IsConfirmed = true, Exprired = new DateTime(2025, 1, 1), UserId = 3 };
            modelBuilder.Entity<EmailConfirm>().HasData(user1AccountActive, user2AccountActive, user3AccountActive);

            base.OnModelCreating(modelBuilder);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<EmailConfirm> emailConfirms { get; set; }
        public DbSet<Event> events { get; set; }
        public DbSet<EventJoin> eventJoins { get; set; }
        //public DbSet<EventType> eventTypes { get; set; }
        public DbSet<RefreshToken> refreshTokens { get; set; }
        //public DbSet<RewardDisciplineType> rewardDisciplineTypes { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<MemberInfo> memberInfos { get; set; }

    }
}
