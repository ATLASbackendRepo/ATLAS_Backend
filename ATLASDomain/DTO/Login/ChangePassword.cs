using System.ComponentModel.DataAnnotations;

namespace ATLASDomain.DTO.Login
{
    /// <summary>
    /// Change Password.
    /// </summary>
    public  class ChangePassword
    {

        /// <summary>
        /// Old Password. The current password of the authenticated user.
        /// </summary>
        [Required]
        public string OldPassword { get; set; }
        /// <summary>
        /// New Password. Must differ from the old password and be at least
        /// 8 characters long (same minimum length as the default issued password).
        /// </summary>
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirm Password. Must match the new password.
        /// </summary>
        [Required]
        [MinLength(8)]
        public string ConfirmPassword { get; set;  }
    }
}
