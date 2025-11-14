namespace TMS.AuthService
{
    /// <summary>
    /// Исключение, указывающее на ошибку конфигурации приложения.
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ConfigurationException(string message) : base(message) { }
    }
}
