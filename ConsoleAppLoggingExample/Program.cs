#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)

namespace ConsoleApp
{
    using Library;
    using System;

    /// <summary>
    /// Example of how to simply configure and use log4net
    /// </summary>
    public class Program
	{
		/// <summary>s
		/// Application entry point
		/// </summary>
		/// <param name="args">command line arguments</param>
		public static void Main(string[] args)
		{
			// Log an info level message
            LogUtility.Info(() => "Start");
            LogUtility.Warn(() => "Testing special characters: áéíóúüñ ÁÉÍÓÚÜÑ");

            Init();

			try
			{
                new InstanceClass().Start();
				Bar();
			}
			catch(Exception ex)
			{
				// Log an error with an exception
				LogUtility.PublishException(ex, "Exception thrown from method Bar");
			}

            TestLoggingErrors();

            TestPushingLogMessages();

            Pause();
		}

        private static void Init()
        {
            // Log a debug message. Test if debug is enabled before
            // attempting to log the message. This is not required but
            // can make running without logging faster.
            LogUtility.Debug(() => "This is a debug message");
        }

        private static void Pause()
        {
            // Log an info level message
            LogUtility.Info(() => "Application [ConsoleApp] End");

            //Console.Write("Press Enter to exit...");
            //Console.ReadLine();
        }

        private static void TestPushingLogMessages()
        {
            // Push a message on to the Nested Diagnostic Context stack
            using (log4net.NDC.Push("NDC_Message"))
            {
                LogUtility.Warn(() => "This should have an NDC message");

                // Set a Mapped Diagnostic Context value  
                log4net.MDC.Set("auth", "auth-none");
                LogUtility.Warn(() => "This should have an MDC message for the key 'auth'");

            } // The NDC message is popped off the stack at the end of the using {} block

            LogUtility.Warn(() => "See the NDC has been popped of! The MDC 'auth' key is still with us.");
        }

        private static void TestLoggingErrors()
        {
            LogUtility.Error("Hey this is an error!");
        }

		// Helper methods to demonstrate location information and nested exceptions

		private static void Bar()
		{
			Goo();
		}

		private static void Foo()
		{
			throw new Exception("This is an Exception");
		}

		private static void Goo()
		{
			try
			{
				Foo();
			}
			catch(Exception ex)
			{
				throw new ArithmeticException("Failed in Goo. Calling Foo. Inner Exception provided", ex);
			}
		}
	}
}
