namespace MadDogs.TaskbarGroups.Common.Model
{
    public class ProgramShortcut
    {
        public string Arguments = "";
        public string WorkingDirectory = Paths.ExeString;

        public string FilePath { get; set; }
        public bool IsWindowsApp { get; set; }

        public string Name { get; set; } = "";
    }
}