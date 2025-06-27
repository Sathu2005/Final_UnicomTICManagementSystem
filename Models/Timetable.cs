namespace SchoolManagementSystem.Models
{
    public class Timetable
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int RoomId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int LecturerId { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public string SubjectName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string LecturerName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}