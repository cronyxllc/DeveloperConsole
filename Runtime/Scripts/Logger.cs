using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cronyx.Console
{
	internal class Logger
	{
		internal enum LogLevel
		{
			Info = 1,
			Warn = 2,
			Error = 4
		}

		internal const string ApplicationName = "Developer Console";
		private string mLogFormat = $"<b>[{ApplicationName}]</b> {{0}}";

		public static void Out(object s) => RuntimeLog.LogOut(s);
		public static void Warn(object s) => RuntimeLog.LogWarn(s);
		public static void Error(object s) => RuntimeLog.LogError(s);

		public void Write(LogLevel level, object s)
		{
			switch (level)
			{
				case LogLevel.Error:
					Debug.LogErrorFormat(mLogFormat, s.ToString());
					break;
				case LogLevel.Warn:
					Debug.LogWarningFormat(mLogFormat, s.ToString());
					break;
				case LogLevel.Info:
					Debug.LogFormat(mLogFormat, s.ToString());
					break;
			}
		}

		public void LogOut(object s) => Write(LogLevel.Info, s);
		public void LogWarn(object s) => Write(LogLevel.Warn, s);
		public void LogError(object s) => Write(LogLevel.Error, s);

		public static readonly Logger RuntimeLog = new Logger();

		private Logger() { }
	}
}
