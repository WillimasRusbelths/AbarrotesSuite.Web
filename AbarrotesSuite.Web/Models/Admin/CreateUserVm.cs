using System.ComponentModel.DataAnnotations;

namespace AbarrotesSuite.Web.Models.Admin;

public class CreateUserVm
{
    [Required, StringLength(40)]
    public string Username { get; set; } = "";

    [Required, StringLength(80, MinimumLength = 6)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    public string Role { get; set; } = "cajero";
}
