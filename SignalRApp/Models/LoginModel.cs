using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace SignalRApp.Models
{
    public class LoginModel
    {
        private string? _returnUrl;

        [Required(ErrorMessage = "UserName is required")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string ReturnUrl
        {
            get
            {
                if (_returnUrl == null)
                {
                    return "/";
                }
                else
                {
                    return _returnUrl;
                }
            }
            set
            {
                _returnUrl = value;
            }

        }
    }
}
