using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TaskManagementAPI.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<TaskItem>? Tasks { get; set; }
    }
}
