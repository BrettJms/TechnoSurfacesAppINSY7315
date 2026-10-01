using System.ComponentModel.DataAnnotations;

namespace TechnoSurfacesApp.Models;

/// <summary>
/// The sign-in form. Validation rules live here as data annotations and are
/// re-checked on the server (Task 1 §8.4).
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [StringLength(128)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Display(Name = "Keep me signed in on this device")]
    public bool RememberMe { get; set; }

    /// <summary>The page the user asked for before being sent to sign in.</summary>
    public string? ReturnUrl { get; set; }
}