namespace JobHunter.Application.Contracts.Permissions
{
    public class ReqCreatePermissionDto
    {
        public string Name { get; set; } = null!;
        public string ApiPath { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Module { get; set; } = null!;
    }
}