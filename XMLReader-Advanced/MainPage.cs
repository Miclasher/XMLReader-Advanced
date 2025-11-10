using System.Xml;
using System.Xml.Xsl;
using XMLReader_Advanced.Models;
using XMLReader_Advanced.Utilities;
using XMLReader_Advanced.Utilities.Filtering;
using XMLReader_Advanced.Utilities.Saving;

namespace XMLReader_Advanced;

public partial class MainPage : Form
{
    private readonly GoogleDriveService _driveService;
    private List<ScientificWork> _lastSearchResult;
    private const string LocalXmlPath = @"C:\Users\Miclasher\source\repos\Miclasher\XMLReader-Advanced\XMLReader-Advanced\ScientificWorks.xml";
    private const string LocalXslPath = @"C:\Users\Miclasher\source\repos\Miclasher\XMLReader-Advanced\XMLReader-Advanced\ScientificWorks.xls";
    private readonly Logger _logger = Logger.Instance;

    public MainPage()
    {
        InitializeComponent();
        _lastSearchResult = new List<ScientificWork>();

        try
        {
            _driveService = new GoogleDriveService();
            // TODO : implement gdrive auth and loading xml
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Помилка ініціалізації Google Drive: {ex.Message}");
            MessageBox.Show(@"Помилка ініціалізації Google Drive. Сервіс не буде доступний.");
            _driveService = null!;
        }
    }

    private void Form_Load(object sender, EventArgs e)
    {
        if (System.IO.File.Exists(LocalXmlPath))
        {
            FillDropdowns();
        }
        else
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Файл {LocalXmlPath} не знайдено.");
            MessageBox.Show($@"Не вдалося знайти файл {LocalXmlPath}. Переконайтеся, що він існує.");
        }
        XMLDOMRadioButton.Checked = true;
    }

    private void FillDropdowns()
    {
        var doc = new XmlDocument();
        doc.Load(LocalXmlPath);
        var root = doc.DocumentElement;

        if (root != null)
        {
            var workNodes = root.SelectNodes("Work");
            foreach (XmlNode workNode in workNodes!)
            {
                CheckPropertyKey(FacultyComboBox, workNode, "@Faculty");
                CheckPropertyKey(DepartmentComboBox, workNode, "@Department");
                CheckPropertyKey(FieldComboBox, workNode, "@Field");
                CheckPropertyKey(LabComboBox, workNode, "Lab");

                var clientNode = workNode.SelectSingleNode("Client");
                if (clientNode != null)
                {
                    CheckPropertyKey(ClientNameComboBox, clientNode, "@Name");
                }

                var authorNodes = workNode.SelectNodes("Authors/Author");
                foreach (XmlNode authorNode in authorNodes!)
                {
                    CheckPropertyKey(AuthorFullNameComboBox, authorNode, "@FullName");
                    CheckPropertyKey(AuthorPositionComboBox, authorNode, "@Position");
                }
            }
        }
    }

    private void CheckPropertyKey(ComboBox comboBox, XmlNode node, string property)
    {
        var valueNode = property.StartsWith("@") ? node.Attributes!.GetNamedItem(property[1..]) : node.SelectSingleNode(property);

        if (valueNode != null && !string.IsNullOrEmpty(valueNode.Value))
        {
            var value = valueNode.Value.Trim();
            if (!comboBox.Items.Contains(value))
            {
                comboBox.Items.Add(value);
            }
        }
    }

    private void SearchButton_Click(object sender, EventArgs e)
    {
        Search();
    }

    private void Search()
    {
        ResultRichTextBox.Text = "";
        var searchParams = new ScientificWork();
        var logParams = "";

        try
        {
            if (FacultyCheckBox.Checked && FacultyComboBox.SelectedItem != null)
            {
                searchParams.Faculty = FacultyComboBox.SelectedItem.ToString()!;
                logParams += $"Faculty: '{searchParams.Faculty}', ";
            }

            if (DepartmentCheckBox.Checked && DepartmentComboBox.SelectedItem != null)
            {
                searchParams.Department = DepartmentComboBox.SelectedItem.ToString()!;
                logParams += $"Department: '{searchParams.Department}', ";
            }

            if (FieldCheckBox.Checked && FieldComboBox.SelectedItem != null)
            {
                searchParams.Field = FieldComboBox.SelectedItem.ToString()!;
                logParams += $"Field: '{searchParams.Field}', ";
            }

            if (LabCheckBox.Checked && LabComboBox.SelectedItem != null)
            {
                searchParams.Lab = LabComboBox.SelectedItem.ToString()!;
                logParams += $"Lab: '{searchParams.Lab}', ";
            }

            if (ClientNameCheckBox.Checked && ClientNameComboBox.SelectedItem != null)
            {
                searchParams.ClientName = ClientNameComboBox.SelectedItem.ToString()!;
                logParams += $"ClientName: '{searchParams.ClientName}', ";
            }

            if (AuthorFullNameCheckBox.Checked && AuthorFullNameComboBox.SelectedItem != null)
            {
                searchParams.AuthorFullName = AuthorFullNameComboBox.SelectedItem.ToString()!;
                logParams += $"AuthorFullName: '{searchParams.AuthorFullName}', ";
            }

            if (AuthorPositionCheckBox.Checked && AuthorPositionComboBox.SelectedItem != null)
            {
                searchParams.AuthorPosition = AuthorPositionComboBox.SelectedItem.ToString()!;
                logParams += $"AuthorPosition: '{searchParams.AuthorPosition}', ";
            }
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Помилка зчитування параметрів пошуку: {ex.Message}");
            MessageBox.Show(@"Помилка при зчитуванні параметрів.");
            return;
        }

        // Логування
        if (string.IsNullOrEmpty(logParams))
        {
            logParams = "немає параметрів";
        }

        _logger.Log(Logger.LoggingLevel.Filtering, $"Пошук. Параметри: {logParams.TrimEnd(' ', ',')}");

        // Вибір стратегії
        IXmlFilteringStrategy parserStrategy;
        if (XMLDOMRadioButton.Checked)
        {
            parserStrategy = new DomxmlFilteringStrategy();
        }
        else if (LINQRadioButton.Checked)
        {
            parserStrategy = new LinqFilteringStrategy();
        }
        else if (SAXRadioButton.Checked)
        {
            parserStrategy = new SaxFilteringStrategy();
        }
        else
        {
            MessageBox.Show(@"Будь ласка, оберіть метод парсингу.");
            return;
        }

        // Search
        try
        {
            _lastSearchResult = parserStrategy.Filter(searchParams, LocalXmlPath);
            _logger.Log(Logger.LoggingLevel.Filtering, $"Пошук завершено. Знайдено: {_lastSearchResult.Count} елементів.");
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Помилка під час парсингу ({parserStrategy.GetType().Name}): {ex.Message}");
            MessageBox.Show($@"Помилка виконання пошуку: {ex.Message}");
            return;
        }

        // Show results
        if (_lastSearchResult.Count == 0)
        {
            ResultRichTextBox.Text = @"Нічого не знайдено.";
            return;
        }

        foreach (var work in _lastSearchResult)
        {
            ResultRichTextBox.Text += $"Назва роботи: {work.WorkTitle}\n";
            ResultRichTextBox.Text += $"Факультет: {work.Faculty}\n";
            ResultRichTextBox.Text += $"Кафедра: {work.Department}\n";
            ResultRichTextBox.Text += $"Галузь: {work.Field}\n";
            ResultRichTextBox.Text += $"Лабораторія: {work.Lab}\n";
            ResultRichTextBox.Text += $"Автор: {work.AuthorFullName} ({work.AuthorPosition})\n";
            ResultRichTextBox.Text += $"Замовник: {work.ClientName}\n";
            ResultRichTextBox.Text += "__________________________________\n\n";
        }
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        ResultRichTextBox.Clear();
        _lastSearchResult.Clear();

        FacultyComboBox.SelectedItem = null;
        DepartmentComboBox.SelectedItem = null;
        FieldComboBox.SelectedItem = null;
        LabComboBox.SelectedItem = null;
        ClientNameComboBox.SelectedItem = null;
        AuthorFullNameComboBox.SelectedItem = null;
        AuthorPositionComboBox.SelectedItem = null;

        FacultyCheckBox.Checked = false;
        DepartmentCheckBox.Checked = false;
        FieldCheckBox.Checked = false;
        LabCheckBox.Checked = false;
        ClientNameCheckBox.Checked = false;
        AuthorFullNameCheckBox.Checked = false;
        AuthorPositionCheckBox.Checked = false;

        _logger.Log(Logger.LoggingLevel.Filtering, "Поля очищено.");
    }

    private void HTMLButton_Click(object sender, EventArgs e)
    {
        Transform();
    }

    private void Transform()
    {
        try
        {
            var xct = new XslCompiledTransform();
            xct.Load(LocalXslPath);
            var fileHtml = "ScientificWorks.html";

            xct.Transform(LocalXmlPath, fileHtml);

            _logger.Log(Logger.LoggingLevel.Transformation, $"Трансформація у {fileHtml} успішна.");
            MessageBox.Show($@"Трансформація успішна! Файл збережено як {fileHtml}");
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Помилка трансформації: {ex.Message}");
            MessageBox.Show($@"Помилка XSLT трансформації: {ex.Message}");
        }
    }

    private void SaveToDriveXMLButton_Click(object sender, EventArgs e)
    {
        HandleSave(new XmlSaverFactory(_driveService));
    }

    private void SaveToDriveHTMLButton_Click(object sender, EventArgs e)
    {
        HandleSave(new HtmlSaverFactory(_driveService));
    }

    private void HandleSave(SaverFactory factory)
    {
        if (_lastSearchResult.Count == 0)
        {
            MessageBox.Show(@"Немає даних для збереження. Спочатку виконайте пошук.");
            return;
        }

        try
        {
            var fileName = "FilteredWorks_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var saver = factory.CreateSaver();

            saver.Save(_lastSearchResult, fileName);

            MessageBox.Show($@"Файл {fileName} успішно збережено на Google Drive.");
        }
        catch (NotImplementedException)
        {
            _logger.Log(Logger.LoggingLevel.Error, "Помилка збереження GDrive: автентифікація не реалізована.");
            MessageBox.Show(@"Помилка збереження: Сервіс Google Drive не налаштований (потрібна автентифікація).");
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LoggingLevel.Error, $"Помилка збереження на GDrive: {ex.Message}");
            MessageBox.Show($@"Помилка збереження: {ex.Message}");
        }
    }


    private void Form_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (MessageBox.Show(@"Чи дійсно ви хочете завершити роботу з програмою?",
                            @"Вихід",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.No)
        {
            e.Cancel = true;
        }
    }
}
