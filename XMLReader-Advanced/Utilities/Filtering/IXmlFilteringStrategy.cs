using System.Xml;
using System.Xml.Linq;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Filtering;

public interface IXmlFilteringStrategy
{
    List<ScientificWork> Filter(ScientificWork searchParams, XmlDocument document);
}