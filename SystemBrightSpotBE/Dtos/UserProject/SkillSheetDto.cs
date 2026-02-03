namespace SystemBrightSpotBE.Dtos.UserProject
{
    public class SkillSheetDto
    {
        public string jid { get; set; } = String.Empty;
        public string full_name { get; set; }  = String.Empty;
        public int age { get; set; }
        public string gender_name { get; set; } = String.Empty;
        public List<UserProjectDto> projects { get; set; } = new List<UserProjectDto>();
    }
}
