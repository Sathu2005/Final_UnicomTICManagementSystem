namespace SchoolManagementSystem.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int Credits { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation property
        public string CourseName { get; set; } = string.Empty;
    }
}