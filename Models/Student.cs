namespace SchoolManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public string FullName => $"{FirstName} {LastName}";
        public string CourseName { get; set; } = string.Empty;
    }
}