using Discord;
using Serilog;


namespace BishHouse2.Util
{
    public static class LogHelper
    {
        public static Task OnLogAsync(ILogger logger, LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Verbose:
                    logger.Information(msg.ToString());
                    break;

                case LogSeverity.Info:
                    logger.Information(msg.ToString());
                    break;

                case LogSeverity.Warning:
                    logger.Warning(msg.ToString());
                    break;

                case LogSeverity.Error:
                    logger.Error(msg.ToString());
                    break;

                case LogSeverity.Critical:
                    logger.Fatal(msg.ToString());
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
