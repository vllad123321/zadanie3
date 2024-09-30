using System;
using System.Collections.Generic;
using System.IO;

// Класс Student
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<int> CourseIds { get; set; } = new List<int>();

    public override string ToString()
    {
        return $"student {Id} {Name} {string.Join(",", CourseIds)}";
    }
}

// Класс Teacher
public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Experience { get; set; }
    public List<int> CourseIds { get; set; } = new List<int>();

    public override string ToString()
    {
        return $"teacher {Id} {Name} {Experience} {string.Join(",", CourseIds)}";
    }
}

// Класс Course
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TeacherId { get; set; }
    public List<int> StudentIds { get; set; } = new List<int>();

    public override string ToString()
    {
        return $"course {Id} {Name} {TeacherId} {string.Join(",", StudentIds)}";
    }
}

public interface IObjectFactory
{
    object Create(string data);
}

// Фабрика для Student
public class StudentFactory : IObjectFactory
{
    public object Create(string data)
    {
        var parts = data.Split(' ');
        if (parts.Length < 4) throw new FormatException("Неверный формат данных для студента.");

        return new Student
        {
            Id = int.Parse(parts[1]),
            Name = string.Join(" ", parts.Skip(2).Take(parts.Length - 2)), // Объединяем части имени
            CourseIds = new List<int>(Array.ConvertAll(parts[parts.Length - 1].Split(','), int.Parse)) // Последняя часть - это CourseIds
        };
    }
}

// Фабрика для Teacher
public class TeacherFactory : IObjectFactory
{
    public object Create(string data)
    {
        var parts = data.Split(' ');
        if (parts.Length < 5) throw new FormatException("Неверный формат данных для учителя.");

        return new Teacher
        {
            Id = int.Parse(parts[1]),
            Name = string.Join(" ", parts.Skip(2).Take(parts.Length - 4)), // Объединяем части имени
            Experience = int.Parse(parts[parts.Length - 2]), // Предпоследняя часть - это стаж
            CourseIds = new List<int>(Array.ConvertAll(parts[parts.Length - 1].Split(','), int.Parse)) // Последняя часть - это CourseIds
        };
    }
}

// Фабрика для Course
public class CourseFactory : IObjectFactory
{
    public object Create(string data)
    {
        var parts = data.Split(' ');
        if (parts.Length < 5) throw new FormatException("Неверный формат данных для курса.");

        return new Course
        {
            Id = int.Parse(parts[1]),
            Name = string.Join(" ", parts.Skip(2).Take(parts.Length - 3)), // Объединяем части названия курса
            TeacherId = int.Parse(parts[parts.Length - 2]), // Предпоследняя часть - это TeacherId
            StudentIds = new List<int>(Array.ConvertAll(parts[parts.Length - 1].Split(','), int.Parse)) // Последняя часть - это StudentIds
        };
    }
}

// Класс управления данными
public class DataManager
{
    private Dictionary<string, IObjectFactory> _factories = new Dictionary<string, IObjectFactory>
    {
        { "student", new StudentFactory() },
        { "teacher", new TeacherFactory() },
        { "course", new CourseFactory() }
    };

    public List<Student> Students { get; set; } = new List<Student>();
    public List<Teacher> Teachers { get; set; } = new List<Teacher>();
    public List<Course> Courses { get; set; } = new List<Course>();

    // Сохранение данных в файл
    public void Save(string fileName)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach (var student in Students)
                writer.WriteLine(student.ToString());

            foreach (var teacher in Teachers)
                writer.WriteLine(teacher.ToString());

            foreach (var course in Courses)
                writer.WriteLine(course.ToString());
        }
    }

    // Загрузка данных из файла
    public void Load(string fileName)
    {
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(' ');
                string objectType = parts[0];
                if (_factories.ContainsKey(objectType))
                {
                    var obj = _factories[objectType].Create(line);
                    if (obj is Student student) Students.Add(student);
                    else if (obj is Teacher teacher) Teachers.Add(teacher);
                    else if (obj is Course course) Courses.Add(course);
                }
            }
        }
    }
}

// Пример 
class Program
{
    static void Main(string[] args)
    {
        DataManager dataManager = new DataManager();

        var student1 = new Student { Id = 10, Name = "Homer Simpson", CourseIds = new List<int> { 6, 8 } };
        var student2 = new Student { Id = 28, Name = "John Smith", CourseIds = new List<int> { 5 } };
        var teacher = new Teacher { Id = 19, Name = "Mikle Jordan", Experience = 52, CourseIds = new List<int> { 3 } };
        var course = new Course { Id = 5, Name = "PE 2207", TeacherId = 19, StudentIds = new List<int> { 1, 2 } };

        dataManager.Students.Add(student1);
        dataManager.Students.Add(student2);
        dataManager.Teachers.Add(teacher);
        dataManager.Courses.Add(course);

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "data.txt");
        dataManager.Save(filePath);
        Console.WriteLine($"Данные сохранены в файл: {filePath}");

        // Загружаем данные из файла
        DataManager newManager = new DataManager();
        newManager.Load(filePath);

        // Вывод загруженных данных
        Console.WriteLine("\nЗагруженные данные:");
        foreach (var student in newManager.Students)
            Console.WriteLine(student);

        foreach (var teacherLoaded in newManager.Teachers)
            Console.WriteLine(teacherLoaded);

        foreach (var courseLoaded in newManager.Courses)
            Console.WriteLine(courseLoaded);
    }
}
