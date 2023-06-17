using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Text;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

using System.Diagnostics;

namespace ArmaTools
{

    class MyExtension
    {

     public static ExtensionCallback callback;
     public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);

#if WIN64
    [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
#else
	[DllExport("_RVExtensionRegisterCallback@4", CallingConvention = CallingConvention.Winapi)]
#endif
    public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
    {
        callback = func;
    }

#if WIN64
    [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
#else
	[DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
#endif
    public static void RvExtensionVersion(StringBuilder output, int outputSize)
    {
        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            if (assembly == null) return;
                
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fvi == null) return;
                

            buildStr(output, "GC Arma Dev Tools. Version: " + fvi.FileVersion, outputSize);

        }
        catch (Exception e)
        {
            output.Append(e.Message);
        }
    }

#if WIN64
    [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
#else
	[DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
#endif
    public static void RvExtension(StringBuilder output, int outputSize,
        [MarshalAs(UnmanagedType.LPStr)] string function)
    {
   if (isEqualVarName(function, "isLoaded"))
    output.Append("true");
   else
     output.Append(function);
    }

#if WIN64
    [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
#else
	[DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
#endif
    public static int RvExtensionArgs(StringBuilder output, int outputSize,
        [MarshalAs(UnmanagedType.LPStr)] string function,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
    {
     try
     {
      if (isEqualVarName(function,"doesFileExist"))
      {
      if (argumentCheck(argCount, 1, ref output))
      {
       string file = FixFilePath(ref args[0]);

           //  s = s.Replace("\\", "");

        bool res = FileExists(file);

        output.Append(res.ToString());
       }

    }
    else if (isEqualVarName(function, "ExecuteFile"))
    {

     if (argumentCheck(argCount, 2, ref output))
     {
      string file = FixFilePath(ref args[0]); // Must strip quotes from file path

      if (!FileExists(file))
       throw new Exception("ExecuteFile file not found");

      output.Append("Executing file \"" + args[0] + "\"" + " Args: " + args[1] + " ");
      if (ExecuteFile(file, args[1]))
       output.Append("Success");
      else
       output.Append("Failed");

     }

     /*if (argumentCheck(argCount, 3, ref output))
     {
      string argStr = "";
      for(int i = 1; i < args.Length; i++)
      {
       argStr += args[i] + " ";
      }

      output.Append("Executing file \"" + args[0] + "\"" + " Args: " + argStr + " ");
      if (ExecuteFile(args[0], argStr))
       output.Append("Success");
      else
       output.Append("Failed");

     }*/
    }
    else
    {
     output.Append("Invalid function name \"" + function + "\"");
    }

    }
    catch (Exception e)
    {
        if (argCount > 0)
            output.Append("Function: " + function + " arg: " + args[0] + "   ");

        output.Append("Operation Failed " + e.Message);
    }

     return 0;
    }

  static bool argumentCheck( int argCountGot, int argCountNeed, ref StringBuilder output)
  {
   if (argCountNeed != argCountGot)
   {
    output.Append("Invalid argument count. Got: " + argCountGot + " Need: " + argCountNeed);
    return false;
   }
   return true;
  }

  static string FixFilePath(ref string path)
  {
   return path.Replace("\"", "");
  }

    static bool FileExists(string path)
    {
    var dirInfo = new DirectoryInfo(Path.GetDirectoryName(path));
    string file = Path.GetFileName(path);
    bool exists = (dirInfo.Exists && dirInfo.EnumerateFiles().Any(f => f.Name.Equals(file)));
    return exists;
    }

    static bool ExecuteFile(string filename,string args)
    {
    try
    {

    ProcessStartInfo startInfo = new ProcessStartInfo();
     startInfo.CreateNoWindow = true;
     startInfo.UseShellExecute = false;
     startInfo.FileName = filename;
     startInfo.WindowStyle = ProcessWindowStyle.Normal;

     startInfo.Arguments = args;

    using (Process exeProcess = Process.Start(startInfo))
    {
     //exeProcess.WaitForExit();
    }
   }
   catch (Exception ex)
   {
    //MessageBox.Show(ex.ToString());
    return false;
   }

   return true;
  }

    static void buildStr(StringBuilder strb, string str, int maxSize, bool reportError = false)
    {
        strb.Append(str);

        if (strb.Length >= maxSize)
        {
            strb.Length = maxSize - 1;

            if (reportError)
                throw new Exception("String builder out of range");
                
        }
    }

  static bool isEqualVarName(string str1, string str2)
  {
   return String.Equals(str1,str2, StringComparison.OrdinalIgnoreCase);
  }

 }

}
