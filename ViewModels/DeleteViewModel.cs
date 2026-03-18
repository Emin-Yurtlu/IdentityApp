using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
    public class DeleteViewModel
    {

        [EmailAddress]
        public string? Email { get; set; }

        public string Id { get; set; }

        public string? FullName { get; set; }
    }
}