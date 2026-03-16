using System.Collections.Generic;

namespace JobHunter.Application.Contracts.Roles
{
    public class ReqUpdateRoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public List<long>? Permissions { get; set; }
    }
}