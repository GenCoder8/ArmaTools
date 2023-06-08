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
                

            buildStr(output, "GC Arma Dev Tools. ver: " + fvi.FileVersion, outputSize);

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
   //if (function.Equals("version"))

    //output.Append(function);
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
      if (isEqualVarname(function,"doesFileExist"))
      {
         if (argCount == 1)
         {
             string s = args[0];
                        
             s = s.Replace("\"", "");

           //  s = s.Replace("\\", "");

             bool res = FileExists(s);

             output.Append(res.ToString());
         }
         else
         {
             output.Append("Invalid argument count");
         }

       }
    else if (isEqualVarname(function, "ExecuteFile"))
    {
     if (argCount == 2)
     {
      string argStr = "";
      for(int i = 1; i < args.Length; i++)
      {
       argStr += args[i] + " ";
      }

      output.Append("Executing file \"" + args[0] + "\"" + " Args: " + argStr);
      ExecuteFile(args[0], argStr);


     }
     else
     {
      output.Append("Invalid argument count");
     }
    }
    else
    {
     output.Append("Invalid function name \"" + function + "\"");
    }

    }
    catch (Exception e)
    {
        if (argCount > 0)
            output.Append("file: " + args[0] + "   ");

        output.Append("Operation Failed " + e.Message);
    }

     return 0;
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
     ProcessStartInfo startInfo = new ProcessStartInfo();
     startInfo.CreateNoWindow = true;
     startInfo.UseShellExecute = false;
     startInfo.FileName = filename;
     startInfo.WindowStyle = ProcessWindowStyle.Normal;

     startInfo.Arguments = args;

   try
   {
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

  static bool isEqualVarname(string str1, string str2)
  {
   return String.Equals(str1,str2, StringComparison.OrdinalIgnoreCase);
  }

 }

}
