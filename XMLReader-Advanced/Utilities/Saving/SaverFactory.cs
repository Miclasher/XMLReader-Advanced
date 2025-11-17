namespace XMLReader_Advanced.Utilities.Saving;

public abstract class SaverFactory
{
    protected readonly GoogleDriveService DriveService;

    protected SaverFactory(GoogleDriveService service)
    {
        DriveService = service;
    }

    public abstract IFileSaver CreateSaver();
}

public class XmlSaverFactory : SaverFactory
{
    public XmlSaverFactory(GoogleDriveService service) : base(service) { }

    public override IFileSaver CreateSaver()
    {
        return new XmlSaver(DriveService);
    }
}

public class HtmlSaverFactory : SaverFactory
{
    public HtmlSaverFactory(GoogleDriveService service) : base(service) { }

    public override IFileSaver CreateSaver()
    {
        return new HtmlSaver(DriveService);
    }
}
