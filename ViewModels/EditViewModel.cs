using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
    public class EditViewModel
    {

      
        public string? FullName { get; set; }

        public string Id { get; set; }

        [EmailAddress]
        public string? Email { get; set; } 


     
        [DataType(DataType.Password)]
        public string? Password { get; set; }


   
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parola Eşleşmiyor")]
        public string? ConfirmPassword { get; set; } 
    }
}