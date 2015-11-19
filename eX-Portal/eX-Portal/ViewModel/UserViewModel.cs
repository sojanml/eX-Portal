using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eX_Portal.ViewModel
{
    public class UserViewModel
    {
        public class LoginViewModel
        {
            public class UserLogon
            {
                [Required]
                [Display(Name = "User Login")]
                public string UserName { get; set; }

                [Required]
                [DataType(DataType.Password)]
                [Display(Name = "Password")]
                public string Password { get; set; }
            }
        }
    }
}