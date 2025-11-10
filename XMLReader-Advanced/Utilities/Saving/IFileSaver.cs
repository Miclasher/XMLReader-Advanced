using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Saving;

public interface IFileSaver
{
    void Save(List<ScientificWork> works, string fileName);
}
