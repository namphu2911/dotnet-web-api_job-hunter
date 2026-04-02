using System.Collections.Generic;

namespace JobHunter.Application.Contracts.Roles
{
    public class ReqCreateRoleDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public List<ReqObjectIdDto>? Permissions { get; set; }
    }
}