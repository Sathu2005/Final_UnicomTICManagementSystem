namespace SchoolManagementSystem.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; } // in months
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}