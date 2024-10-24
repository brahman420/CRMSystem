using System.ComponentModel.DataAnnotations;

namespace API
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AddCompanyRequest
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; }
    }

    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public int CompanyID { get; set; }

        [Required]
        public int UserID { get; set; }
    }
}
