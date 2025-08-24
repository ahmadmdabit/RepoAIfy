namespace FileToMarkdownConverter;

public class Options
{
    public FileFilter FileFilter { get; set; } = new FileFilter();
    public Chunking Chunking { get; set; } = new Chunking();
    public Output Output { get; set; } = new Output();
}

public class FileFilter
{
    public List<string> IncludedExtensions { get; set; } = new List<string>();
    public List<string> ExcludedDirectories { get; set; } = new List<string>();
}

public class Chunking
{
    public int MaxChunkSizeKb { get; set; } = 128;
}

public class Output
{
    public string OutputDirectory { get; set; } = "./ai-output";
}
