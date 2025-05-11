using BigProject.DataContext;
using BigProject.Entities;
using BigProject.PayLoad.DTO;
using Microsoft.EntityFrameworkCore;

namespace BigProject.PayLoad.Converter
{
    public class Converter_Register
    {
        private readonly AppDbContext _context;
        public Converter_Register(AppDbContext context)
        {
            _context = context;
        }
        public DTO_Register EntityToDTO(User register)
        {
            return new DTO_Register
            {
                Id = register.Id,
                Email = register.Email,
                Password = register.Password,
                Username = register.Username,
                MaSV = register.MaSV,
                 RoleName = _context.roles.SingleOrDefault(x => x.Id == register.RoleId).Name,
            };
        }
    }
}
