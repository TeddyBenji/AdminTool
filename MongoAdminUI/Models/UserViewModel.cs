using System;
using System.Collections.Generic;

namespace MongoAdminUI.Models
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public List<string>? Roles { get; set; }
        public string? Email { get; set; }
    }
}
