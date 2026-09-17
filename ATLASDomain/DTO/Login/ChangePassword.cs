namespace ATLASDomain.DTO.Login
{
    public  class ChangePassword
    {
        public string? AcademicId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set;  }
    }
}
