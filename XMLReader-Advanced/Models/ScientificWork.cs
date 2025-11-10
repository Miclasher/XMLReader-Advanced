namespace XMLReader_Advanced.Models;

public sealed class ScientificWork
{
    public string WorkTitle { get; set; } = "";
    public string Faculty { get; set; } = "";
    public string Department { get; set; } = "";
    public string Field { get; set; } = "";
    public string AuthorFullName { get; set; } = "";
    public string AuthorPosition { get; set; } = "";
    public string Lab { get; set; } = "";
    public string ClientName { get; set; } = "";

    public ScientificWork() { }


    //Constructor for LINQ filtering strategy
    public ScientificWork(string title, string faculty, string department, string field, string lab)
    {
        WorkTitle = title;
        Faculty = faculty;
        Department = department;
        Field = field;
        Lab = lab;
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(WorkTitle) &&
               string.IsNullOrEmpty(Faculty) &&
               string.IsNullOrEmpty(Department) &&
               string.IsNullOrEmpty(Field) &&
               string.IsNullOrEmpty(Lab);
    }
}