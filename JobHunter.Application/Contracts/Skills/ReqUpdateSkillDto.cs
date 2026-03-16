namespace JobHunter.Application.Contracts.Skills
{
    public class ReqUpdateSkillDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
}