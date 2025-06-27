namespace SchoolManagementSystem.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Equipment { get; set; } = string.Empty;
    }

    public enum RoomType
    {
        LectureHall = 1,
        Laboratory = 2,
        ComputerLab = 3,
        Library = 4,
        Auditorium = 5
    }
}