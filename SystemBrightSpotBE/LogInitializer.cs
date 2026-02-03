using log4net;
using System.Configuration;
public class LogInitializer
{
    public ILog InitLog()
    {
        try
        {
            string path = "./log4net.config";
            var fileInfo = new FileInfo(path);

            if (fileInfo.Exists)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
                Console.WriteLine("LOG4NET: Loaded logging configuration from: {0}", path);
            }
            else
            {
                var message = "LOG4NET: Logging configuration does not exist: " + path;
                Console.WriteLine(message);
                throw new ConfigurationErrorsException(message);
            }
        }
        catch (ConfigurationErrorsException ex)
        {
            Console.WriteLine("LOG4NET: log4net is not configured:\n{0}", ex);
        }
        return log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
    }
}