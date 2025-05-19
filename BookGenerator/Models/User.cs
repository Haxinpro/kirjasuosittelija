using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string PasswordHash { get; set; } // SHA-256-hashattu salasana
}
