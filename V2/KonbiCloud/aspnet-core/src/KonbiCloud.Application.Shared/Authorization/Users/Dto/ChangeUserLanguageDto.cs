using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Authorization.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
