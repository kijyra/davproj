namespace davproj.Models
{
    public class FileSystemItemViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string RelativePath { get; set; }
        public string FormattedSize { get; set; }
        public string FileExtension { get; set; }
        public List<FileSystemItemViewModel> Children { get; set; } = new List<FileSystemItemViewModel>();
    }
}