using System.Text;
using XMLReader_Advanced.Models;

namespace XMLReader_Advanced.Utilities.Saving;

public sealed class HtmlSaver : IFileSaver
{
    private readonly GoogleDriveService _driveService;

    public HtmlSaver(GoogleDriveService service)
    {
        _driveService = service;
    }

    public void Save(List<ScientificWork> works, string fileName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'><title>Звіт</title></head><body>");
        sb.AppendLine("<h1>Результати пошуку</h1>");
        sb.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%;'>");
        sb.AppendLine("<tr style='background-color: #f2f2f2;'><th>Назва</th><th>Факультет</th><th>Кафедра</th><th>Автор</th></tr>");

        foreach (var work in works)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{work.WorkTitle}</td>");
            sb.Append($"<td>{work.Faculty}</td>");
            sb.Append($"<td>{work.Department}</td>");
            sb.Append($"<td>{work.AuthorFullName}</td>");
            sb.Append("</tr>");
        }

        sb.AppendLine("</table></body></html>");

        var content = sb.ToString();
        _driveService.SaveFile(content, fileName + ".html", "text/html");
    }
}
