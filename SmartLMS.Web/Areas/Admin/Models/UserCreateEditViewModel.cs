using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Web.Areas.Admin.Models;

public class UserCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Họ là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Họ")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Tên")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    [Display(Name = "Vai trò")]
    public string SelectedRole { get; set; } = string.Empty;

    [Display(Name = "Kích hoạt tài khoản")]
    public bool IsActive { get; set; } = true;

    public IList<string> AvailableRoles { get; set; } = new List<string>();
}