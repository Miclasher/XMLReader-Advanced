using System.Xml;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Filtering;

public class DomxmlFilteringStrategy : IXmlFilteringStrategy
{
    public List<ScientificWork> Filter(ScientificWork searchParams, string xmlFilePath)
    {
        var works = new List<ScientificWork>();
        var doc = new XmlDocument();
        doc.Load(xmlFilePath);

        var workNodes = doc.DocumentElement?.SelectNodes("Work");

        foreach (XmlNode node in workNodes!)
        {
            var isMatch = !(searchParams.WorkTitle != "" && node.Attributes!["WorkTitle"]?.Value != searchParams.WorkTitle);

            if (isMatch && searchParams.Faculty != "" && node.Attributes!["Faculty"]?.Value != searchParams.Faculty)
            {
                isMatch = false;
            }

            if (isMatch && searchParams.Department != "" && node.Attributes!["Department"]?.Value != searchParams.Department)
            {
                isMatch = false;
            }

            if (isMatch && searchParams.Field != "" && node.Attributes!["Field"]?.Value != searchParams.Field)
            {
                isMatch = false;
            }

            var labNode = node.SelectSingleNode("Lab");
            if (isMatch && searchParams.Lab != "" && labNode?.InnerText != searchParams.Lab)
            {
                isMatch = false;
            }

            var clientNode = node.SelectSingleNode("Client");
            if (isMatch && searchParams.ClientName != "" && clientNode?.Attributes!["Name"]?.Value != searchParams.ClientName)
            {
                isMatch = false;
            }

            var authorSearchRequired = searchParams.AuthorFullName != "" || searchParams.AuthorPosition != "";
            if (isMatch && authorSearchRequired)
            {
                isMatch = FindAuthors(searchParams, node, isMatch);
            }

            if (!isMatch)
            {
                continue;
            }

            var work = CreateWorkFromResult(node, labNode, clientNode);

            works.Add(work);
        }
        return works;
    }

    private static ScientificWork CreateWorkFromResult(XmlNode node, XmlNode? labNode, XmlNode? clientNode)
    {
        var work = new ScientificWork
        {
            WorkTitle = node.Attributes!["WorkTitle"]?.Value ?? "",
            Faculty = node.Attributes["Faculty"]?.Value ?? "",
            Department = node.Attributes["Department"]?.Value ?? "",
            Field = node.Attributes["Field"]?.Value ?? "",
            Lab = labNode?.InnerText ?? "",
            ClientName = clientNode?.Attributes!["Name"]?.Value ?? ""
        };

        var firstAuthor = node.SelectSingleNode("Authors/Author");
        if (firstAuthor is null)
        {
            return work;
        }

        work.AuthorFullName = firstAuthor.Attributes!["FullName"]?.Value ?? "";
        work.AuthorPosition = firstAuthor.Attributes["Position"]?.Value ?? "";

        return work;
    }

    private static bool FindAuthors(ScientificWork searchParams, XmlNode node, bool isMatch)
    {
        var authorMatchFound = false;
        var authorNodes = node.SelectNodes("Authors/Author");

        foreach (XmlNode authorNode in authorNodes!)
        {
            var nameMatch = searchParams.AuthorFullName == "" || authorNode.Attributes!["FullName"]?.Value == searchParams.AuthorFullName;
            var posMatch = searchParams.AuthorPosition == "" || authorNode.Attributes!["Position"]?.Value == searchParams.AuthorPosition;

            if (nameMatch && posMatch)
            {
                authorMatchFound = true;
                break;
            }
        }

        if (!authorMatchFound)
        {
            isMatch = false;
        }

        return isMatch;
    }
}
