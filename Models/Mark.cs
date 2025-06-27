namespace SchoolManagementSystem.Models
{
    public class Mark
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public decimal MarksObtained { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public DateTime RecordedDate { get; set; }
        public int RecordedBy { get; set; }
        
        // Navigation properties
        public string StudentName { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal MaxMarks { get; set; }
        public decimal Percentage => MaxMarks > 0 ? (MarksObtained / MaxMarks) * 100 : 0;
    }
}