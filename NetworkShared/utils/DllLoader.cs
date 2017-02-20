using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace com.tieao.network.utils
{
    public class DllLoader
    {
        static DllLoader _instance = new DllLoader();
        DllLoader() { }
        public static DllLoader Instance { get { return _instance; } }
        public Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

        public Assembly LoadAssembly(string dllpath) 
        {
            string path = GetAssemblyPath(dllpath);

            Assembly ass = Assembly.LoadFile(path);
            _assemblies[path] = ass;
            return ass;
        }

        public T CreateInstance<T>(Assembly ass, string typename) where T : class
        {
            return ass.CreateInstance(typename) as T;
        }

        public T CreateInstance<T>(string assembly, string typename) where T : class
        {
            Assembly ass = LoadAssembly(assembly);
            return ass.CreateInstance(typename) as T;
        }
        static string AssemblyDir;
        
        public static string GetAssemblyPath(string relativePath)
        {
            FileInfo ffi = new FileInfo(relativePath);
            if (ffi.Exists) return ffi.FullName;

            if (AssemblyDir == null || AssemblyDir == "")
            {
                string fdir = Assembly.GetExecutingAssembly().Location;
                FileInfo fi = new FileInfo(fdir);
                AssemblyDir = fi.DirectoryName;
            }
            //if (BaseDir == string.Empty) return relativePath;

            return string.Format("{0}/{1}", AssemblyDir, relativePath);
        }
    }
}
