using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Localization.Dto
{
    public class CreateOrUpdateLanguageInput
    {
        [Required]
        public ApplicationLanguageEditDto Language { get; set; }
    }
}