namespace JobHunter.Application.Contracts.Companies
{
    public class ReqUpdateCompanyDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Logo { get; set; }
    }
}