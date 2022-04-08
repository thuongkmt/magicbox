using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KonbiCloud.Categories.Dtos
{
    public class CreateCategoryInput
    {
        [Required]
        public string Name { get; set; }

        public string FileContent { get; set; }
        public string ImageUrl { get; set; }

    }
}
