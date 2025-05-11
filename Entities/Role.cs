namespace BigProject.Entities
{
    public class Role : EntityBase
    {
        public string Name { get; set; }
        ICollection<User> users { get; set; }
    }
}
