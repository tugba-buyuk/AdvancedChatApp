using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public record UserDTO
    {
        [Required(ErrorMessage ="UserName is a required field.")]
        public string UserName { get; init; } = string.Empty;

        [Required(ErrorMessage ="Email is a required field.")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is a required field.")]
        public string Password {  get; init; } = string.Empty;

        [Required(ErrorMessage ="PhoneNumber is a required field.")]
        public string PhoneNumber {  get; init; } = string.Empty;

    }
}
