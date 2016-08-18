using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JsonWebTokensWebApi.Entities
{
    public class Audience
    {
        [Key]
        [MaxLength(32)]
        public string ClientId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Base64Secret { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}