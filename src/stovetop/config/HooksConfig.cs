namespace Stovetop.stovetop.config;

public class HooksConfig
{
    public List<string>? PreRun { get; set; }
    public List<string>? PostRun { get; set; }
    public List<string>? PreBuild { get; set; }
    public List<string>? PostBuild { get; set; }
}