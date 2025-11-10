namespace TMS.Common.Helpers;

public class StringHelper
{
    public static string GetStringForLogger(string value)
    {
        string loggedValue = value?.Length <= 20
            ? value
            : value?.Substring(0, 20) + "...";

        return loggedValue;
    }
}