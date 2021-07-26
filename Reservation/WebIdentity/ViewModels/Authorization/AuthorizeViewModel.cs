using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebIdentity.ViewModels.Authorization
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }

        [Display(Name = "Granted Scope")]
        public List<string>  GrantedScope { get; set; }
    }
}
