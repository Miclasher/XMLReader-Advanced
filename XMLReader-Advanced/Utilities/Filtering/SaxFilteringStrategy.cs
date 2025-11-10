using System.Xml;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Filtering;

public sealed class SaxFilteringStrategy : IXmlFilteringStrategy
{
    public List<ScientificWork> Filter(ScientificWork searchParams, string xmlFilePath)
    {
        var works = new List<ScientificWork>();

        var authorSearchRequired = searchParams.AuthorFullName != "" || searchParams.AuthorPosition != "";

        using var reader = new XmlTextReader(xmlFilePath);
        while (reader.Read())
        {
            if (reader is not { NodeType: XmlNodeType.Element, Name: "Work" })
            {
                continue;
            }

            var currentWork = new ScientificWork();
            var isMatch = true;
            var currentWorkAuthorFound = false;

            // Getting attributes
            currentWork.WorkTitle = reader.GetAttribute("WorkTitle")!;
            currentWork.Faculty = reader.GetAttribute("Faculty")!;
            currentWork.Department = reader.GetAttribute("Department")!;
            currentWork.Field = reader.GetAttribute("Field")!;

            if (searchParams.WorkTitle != "" && searchParams.WorkTitle != currentWork.WorkTitle)
            {
                isMatch = false;
            }

            if (isMatch && searchParams.Faculty != "" && searchParams.Faculty != currentWork.Faculty)
            {
                isMatch = false;
            }

            if (isMatch && searchParams.Department != "" && searchParams.Department != currentWork.Department)
            {
                isMatch = false;
            }

            if (isMatch && searchParams.Field != "" && searchParams.Field != currentWork.Field)
            {
                isMatch = false;
            }

            // Reading children nodes
            if (isMatch && !reader.IsEmptyElement)
            {
                using var subtreeReader = reader.ReadSubtree();
                while (subtreeReader.Read())
                {
                    if (subtreeReader.NodeType == XmlNodeType.Element)
                    {
                        switch (subtreeReader.Name)
                        {
                            case "Lab":
                                currentWork.Lab = subtreeReader.ReadElementContentAsString();
                                if (searchParams.Lab != "" && searchParams.Lab != currentWork.Lab)
                                {
                                    isMatch = false;
                                }

                                break;

                            case "Client":
                                currentWork.ClientName = subtreeReader.GetAttribute("Name")!;
                                if (searchParams.ClientName != "" && searchParams.ClientName != currentWork.ClientName)
                                {
                                    isMatch = false;
                                }

                                break;

                            case "Author":
                                var authorName = subtreeReader.GetAttribute("FullName");
                                var authorPos = subtreeReader.GetAttribute("Position");

                                // Saving first author
                                if (string.IsNullOrEmpty(currentWork.AuthorFullName))
                                {
                                    currentWork.AuthorFullName = authorName!;
                                    currentWork.AuthorPosition = authorPos!;
                                }

                                if (authorSearchRequired && !currentWorkAuthorFound)
                                {
                                    var nameMatch = searchParams.AuthorFullName == "" || searchParams.AuthorFullName == authorName;
                                    var posMatch = searchParams.AuthorPosition == "" || searchParams.AuthorPosition == authorPos;
                                    if (nameMatch && posMatch)
                                    {
                                        currentWorkAuthorFound = true;
                                    }
                                }
                                break;
                        }
                    }
                    if (!isMatch)
                    {
                        break;
                    }
                }
            }

            if (authorSearchRequired && !currentWorkAuthorFound)
            {
                isMatch = false;
            }

            if (isMatch)
            {
                works.Add(currentWork);
            }

            currentWork = null;
        }

        return works;
    }
}
