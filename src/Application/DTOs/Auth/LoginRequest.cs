using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Application key cannot exceed 50 characters")]
    public string? ApplicationKey { get; set; }

    public bool RememberMe { get; set; } = false;
}
