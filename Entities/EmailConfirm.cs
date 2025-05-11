namespace BigProject.Entities
{
    public class EmailConfirm : EntityBase
    {
        public int UserId {  get; set; }
        public string Code { get; set; }
        public DateTime CreateTime {  get; set; } = DateTime.Now;
        public DateTime Exprired {  get; set; }
        public bool IsConfirmed { get; set; }=false;
        public bool IsActiveAccount { get; set; }
        public User? user { get; set; }
    }
}
