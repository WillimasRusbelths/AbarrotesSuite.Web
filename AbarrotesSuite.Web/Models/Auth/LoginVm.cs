using System.ComponentModel.DataAnnotations;

namespace AbarrotesSuite.Web.Models.Auth;

public class LoginVm
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
