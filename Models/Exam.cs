namespace SchoolManagementSystem.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int RoomId { get; set; }
        public int MaxMarks { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        // Navigation properties
        public string SubjectName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
    }
}