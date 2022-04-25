using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendData
{
    public class Utilities
    {
		public void LogException(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		public void LogTrace(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}



		public static string Left(string param, int length)
		{
			//we start at 0 since we want to get the characters starting from the
			//left and with the specified lenght and assign it to a variable
			if (param != null)
			{
				if (param.Length < length)
					length = param.Length;
				string result = param.Substring(0, length);
				//return the result of the operation
				return result;
			}
			return "";
		}
		public static string Right(string param, int length)
		{
			//start at the index based on the lenght of the sting minus
			//the specified lenght and assign it a variable
			string result = param.Substring(param.Length - length, length);
			//return the result of the operation
			return result;
		}
		public static string Mid(string param, int startIndex, int length)
		{
			//start at the specified index in the string ang get N number of
			//characters depending on the lenght and assign it to a variable
			string result = param.Substring(startIndex, length);
			//return the result of the operation
			return result;
		}
		public static string Mid(string param, int startIndex)
		{
			//start at the specified index and return all characters after it
			//and assign it to a variable
			if (startIndex > param.Length)
				startIndex = param.Length;
			string result = param.Substring(startIndex);
			//return the result of the operation
			return result;
		}

		public static bool IsNumeric(string numberString)
		{
			char[] ca = numberString.ToCharArray();
			for (int i = 0; i < ca.Length; i++)
			{
				if (!char.IsNumber(ca[i]))
					return false;
			}
			return true;
		}
	}
}
