using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    internal class Utils
    {
		public static string GetVersion()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			return fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf('.'));
		}
	}
}
