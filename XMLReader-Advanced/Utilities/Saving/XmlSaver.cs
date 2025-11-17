using Google.Apis.Drive.v3;
using System.Xml.Linq;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Saving;

public sealed class XmlSaver : IFileSaver
{
    private readonly GoogleDriveService _driveService;

    public XmlSaver(GoogleDriveService service)
    {
        _driveService = service;
    }

    public void Save(List<ScientificWork> works, string fileName)
    {
        var doc = new XDocument(new XElement("ScientificWorks"));

        foreach (var work in works)
        {
            var workElement = new XElement("Work",
                new XAttribute("WorkTitle", work.WorkTitle ?? ""),
                new XAttribute("Faculty", work.Faculty ?? ""),
                new XAttribute("Department", work.Department ?? ""),
                new XAttribute("Field", work.Field ?? "")
            );

            if (!string.IsNullOrEmpty(work.Lab))
                workElement.Add(new XElement("Lab", work.Lab));

            if (!string.IsNullOrEmpty(work.ClientName))
                workElement.Add(new XElement("Client", new XAttribute("Name", work.ClientName)));

            if (!string.IsNullOrEmpty(work.AuthorFullName))
            {
                workElement.Add(new XElement("Authors",
                    new XElement("Author",
                        new XAttribute("FullName", work.AuthorFullName),
                        new XAttribute("Position", work.AuthorPosition ?? "")
                    )
                ));
            }

            doc.Root?.Add(workElement);
        }

        var content = doc.ToString();
        _driveService.SaveFile(content, fileName + ".xml", "application/xml");
    }
}
