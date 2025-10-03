using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Web.Models.BindingModels;

public class RegisterModel
{
    [Required(ErrorMessage = "Họ là bắt buộc")]
    [Display(Name = "Họ")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên là bắt buộc")]
    [Display(Name = "Tên")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}