namespace MongoAdminUI.Models.RoleModels
{
    public class CreateRole
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<string> Claims { get; set; }
    }
}
