namespace davproj.Models
{
    public class FileSystemItemViewModel
    {
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public string RelativePath { get; set; } = String.Empty;
        public string FormattedSize { get; set; } = String.Empty;
        public string FileExtension { get; set; } = String.Empty;
        public List<FileSystemItemViewModel> Children { get; set; } = new List<FileSystemItemViewModel>();
    }
}