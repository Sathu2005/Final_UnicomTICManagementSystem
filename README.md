# School Management System

A comprehensive Windows Forms application built with C# and .NET 6 for managing school operations including courses, subjects, students, exams, marks, and timetables.

## Features

### User Management
- Role-based access control (Admin, Lecturer, Staff, Student)
- Secure authentication system
- User profile management

### Course Management
- Add, edit, and delete courses
- Course duration tracking
- Course code management

### Subject Management
- Subject creation and management
- Course-subject relationships
- Credit system

### Student Management
- Student registration and profile management
- Course enrollment
- Student information tracking

### Exam Management
- Exam scheduling
- Room assignment
- Subject-based examinations

### Mark Management
- Grade recording and calculation
- Performance tracking
- Report generation

### Timetable Management
- Class scheduling
- Room allocation
- Lecturer assignment

## Technology Stack

- **Framework**: .NET 6 Windows Forms
- **Database**: SQLite
- **Language**: C#
- **Architecture**: Repository Pattern with Service Layer

## Database Schema

The application uses SQLite database with the following main tables:
- Users (Authentication and role management)
- Courses (Course information)
- Subjects (Subject details and course relationships)
- Students (Student profiles and enrollment)
- Rooms (Facility management)
- Exams (Examination scheduling)
- Marks (Grade recording)
- Timetables (Schedule management)

## Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET 6 SDK
- Windows OS

### Installation
1. Clone the repository
2. Open `SchoolManagementSystem.sln` in Visual Studio
3. Restore NuGet packages
4. Build the solution
5. Run the application

### Default Login Credentials
- **Admin**: Username: `admin`, Password: `admin123`
- **Lecturer**: Username: `lecturer1`, Password: `lect123`
- **Staff**: Username: `staff1`, Password: `staff123`
- **Student**: Username: `student1`, Password: `stud123`

## Project Structure

```
SchoolManagementSystem/
├── Data/
│   └── DatabaseContext.cs          # Database connection and initialization
├── Forms/
│   ├── BaseForm.cs                 # Base form with common functionality
│   ├── LoginForm.cs                # Authentication form
│   ├── DashboardForm.cs            # Main dashboard
│   ├── CourseManagementForm.cs     # Course management
│   ├── SubjectManagementForm.cs    # Subject management
│   ├── StudentManagementForm.cs    # Student management
│   └── [Other Forms]               # Additional management forms
├── Models/
│   ├── User.cs                     # User entity
│   ├── Course.cs                   # Course entity
│   ├── Subject.cs                  # Subject entity
│   ├── Student.cs                  # Student entity
│   └── [Other Models]              # Additional entities
├── Repositories/
│   ├── CourseRepository.cs         # Course data access
│   ├── SubjectRepository.cs        # Subject data access
│   └── [Other Repositories]       # Additional repositories
├── Services/
│   └── UserService.cs              # User business logic
├── Interfaces/
│   ├── IRepository.cs              # Generic repository interface
│   └── IUserService.cs             # User service interface
└── Program.cs                      # Application entry point
```

## Features by Role

### Admin
- Full access to all modules
- User management
- System configuration
- All CRUD operations

### Lecturer
- Subject management
- Student information access
- Exam management
- Mark recording
- Timetable viewing

### Staff
- Course management
- Subject management
- Student management
- Room management
- Timetable management

### Student
- View timetable
- View marks and grades
- Limited profile access

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please create an issue in the repository.