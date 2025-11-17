using System.Xml.Linq;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Filtering;

public sealed class LinqFilteringStrategy : IXmlFilteringStrategy
{
    public List<ScientificWork> Filter(ScientificWork searchParams, string xmlFilePath)
    {
        var document = XDocument.Load(xmlFilePath);

        var queryResult = from work in document.Descendants("Work")
                          where
                          (work.Attribute("WorkTitle")?.Value == searchParams.WorkTitle || searchParams.WorkTitle == "") &&
                          (work.Attribute("Faculty")?.Value == searchParams.Faculty || searchParams.Faculty == "") &&
                          (work.Attribute("Department")?.Value == searchParams.Department || searchParams.Department == "") &&
                          (work.Attribute("Field")?.Value == searchParams.Field || searchParams.Field == "") &&
                          (work.Element("Lab")?.Value == searchParams.Lab || searchParams.Lab == "") &&
                          (work.Element("Client")?.Attribute("Name")?.Value == searchParams.ClientName || searchParams.ClientName == "") &&
                          (work.Element("Authors")!.Elements("Author").Any(a =>
                              (a.Attribute("FullName")!.Value == searchParams.AuthorFullName || searchParams.AuthorFullName == "") &&
                              (a.Attribute("Position")!.Value == searchParams.AuthorPosition || searchParams.AuthorPosition == "")
                          ) || searchParams is { AuthorFullName: "", AuthorPosition: "" })
                          select new ScientificWork
                          {
                              WorkTitle = (string)work.Attribute("WorkTitle")! ?? "",
                              Faculty = (string)work.Attribute("Faculty")! ?? "",
                              Department = (string)work.Attribute("Department")! ?? "",
                              Field = (string)work.Attribute("Field")! ?? "",
                              Lab = (string)work.Element("Lab")! ?? "",
                              ClientName = (string)work.Element("Client")?.Attribute("Name")! ?? "",
                              AuthorFullName = work.Element("Authors")!.Elements("Author").First().Attribute("FullName")!.Value ?? "",
                              AuthorPosition = work.Element("Authors")!.Elements("Author").First().Attribute("Position")!.Value ?? ""
                          };


        return queryResult.ToList<ScientificWork>();
    }
}
