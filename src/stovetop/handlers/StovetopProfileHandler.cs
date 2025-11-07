namespace Stovetop.stovetop.handlers;

public class StovetopProfileHandler
{
    // Right SO the general idea of this class is
    // 1. it gets run within the run and build commands
    // 2. it checks whether theres a profile flag set in the argument array
    // 3. if there is one, itll find the index of the given profile
    // 4. itll override the loaded configs values with the values inside the profile if the profile exists and is verified

    // this will load the profile from the given name, and error if it doesnt exist
    public static void LoadProfile(string profileName)
    {
        if (ProfileExists(profileName)) { }
    }

    // this will be used to merge the base config with the profile config, only overriding the values that match both the base config and profile config
    public static StovetopConfig MergeProfile(
        StovetopConfig baseConfig,
        StovetopConfig profileConfig
    )
    {
        StovetopConfig result = new StovetopConfig();

        return result; // return the merged profile
    }

    // this will check whether the given profile exists
    public static bool ProfileExists(string profileName)
    {
        bool verdict = false;

        if (File.Exists(GetProfilePath(profileName)))
            verdict = true;

        return verdict;
    }

    // this will return the full path to the profile file
    public static string GetProfilePath(string profileName)
    {
        return "";
    }

    // list profile method
}
