using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum ParticipationTypeEnum
    {
        [Display(Name = "Participation Position")]
        participation_position = 1,

        [Display(Name = "Participation Process")]
        participation_process = 2,
    }
}
