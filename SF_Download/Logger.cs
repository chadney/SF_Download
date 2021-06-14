
/*

SF Download – Integrating Salesforce Downloads to SQL Server

Copyright (C) 2021 Kevin Chadney


This program is free software: you can redistribute it and/or modify

it under the terms of the GNU General Public License as published by

the Free Software Foundation, either version 3 of the License, or

(at your option) any later version.


This program is distributed in the hope that it will be useful,

but WITHOUT ANY WARRANTY; without even the implied warranty of

MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the

GNU General Public License for more details.


You should have received a copy of the GNU General Public License

along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/

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
