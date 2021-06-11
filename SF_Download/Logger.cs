using System;


namespace SF_Download
{
    public class Logger
    {

        public SFDTarget Target { get; set; }
        private int ConsoleLogLevel;
        private int DBLogLevel;


        //constructor
        public Logger(int consoleLogLevel, int dBLogLevel)
        {
            ConsoleLogLevel = consoleLogLevel;
            DBLogLevel = dBLogLevel;

        }


        public int LogTaskStart(LogTaskType logTaskType, string objectName = null)
        {

            int logTaskId = 0;

            if (logTaskType.MinConsoleLogLevel <= ConsoleLogLevel)
            {
                Console.WriteLine("{0} : {1}", DateTime.Now.ToString(), logTaskType.DisplayName );
                
            }


            if (logTaskType.MinDBLogLevel <= DBLogLevel && Target != null)
            {
                logTaskId = Target.LogTaskStart(logTaskType.DisplayName, objectName: objectName);

            }

            return logTaskId;

        }


        public void LogTaskComplete(LogTaskType logTaskType, int logTaskId)
        {
            if (logTaskType.MinDBLogLevel <= DBLogLevel && logTaskId != 0 && Target != null)
            {
                Target.LogTaskComplete(logTaskId);

            }

        }


        public void LogTaskError(LogTaskType logTaskType, int logTaskId, Exception e)
        {

            Console.WriteLine("{0} : {1} failed ", DateTime.Now.ToString(), logTaskType.DisplayName);
            Console.WriteLine();
            Console.WriteLine(e.ToString());

            if (logTaskId != 0 && Target != null)
            {
                Target.LogTaskComplete(logTaskId, e.ToString());

            }
            else if (logTaskType.MinDBLogLevel < 100 && Target != null)
            {
                Target.LogTaskComplete(Target.LogTaskStart(logTaskType.DisplayName), e.ToString());

            }

        }

    }
}
