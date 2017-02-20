using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using com.ideadynamo.foundation.log;

namespace com.tieao.network.file
{
    public sealed class FileAssembler : IDisposable
    {
        FileStream _stream;
        string _file;
        int _index;
        long _totalSize;

        public long Size { get { return _totalSize; } }

        public static FileAssembler CreateAssembler(string file)
        {
            if (file == string.Empty || file == null) return null;

            BackupFile(file);

            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    file = GetNoDupFile(file, ".pend");
                }
            }
            if (file == string.Empty || file == null) return null;

            return new FileAssembler(file);
        }

        private static void BackupFile(string file)
        {
            try
            {
                string dup = GetNoDupFile(file, ".bak");
                if (File.Exists(file))
                {
                    File.Move(file, dup);
                }
            }
            catch (Exception err)
            {
                LogService.ErrorE(file, "Failed to backup file" + file, err);
            }
        }

        FileAssembler(string file)
        {
            _file = file;
            _stream = new FileStream(file, FileMode.CreateNew);
            _index = 0;
        }

        private static string GetNoDupFile(string file, string postfix)
        {
            int i = 0;
            while (true)
            {
                string name = string.Format("{0}_{1}_{2}.{3}.{4}.{5}.{6}{7}", file, i, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, postfix);
                if (!File.Exists(name))
                {
                    return name;
                }
                ++i;
            }
        }

        public void Append(int index, string content)
        {
            if (index != _index)
            {
                Trace.WriteLine(String.Format("Writer index mismatch, {0} remote and {1} local", index, _index));
            }

            _index++;
            byte[] bs = Encoding.UTF8.GetBytes(content);
            _totalSize += bs.Length;
            _stream.Write(bs, 0, bs.Length);
        }

        public void Append(int index, byte[] content, int startIdx, int length)
        {
            if (index != _index)
            {
                Trace.WriteLine(String.Format("Writer index mismatch, {0} remote and {1} local", index, _index));
            }

            _index++;
            _totalSize += length;
            _stream.Write(content, startIdx, length);
        }

        public void Cancel() 
        {
            LogService.Info(this, "Remove file {0} since canceled" , _file);
            Dispose();
            try
            {
                File.Delete(_file);
            }
            catch (Exception err) 
            {
                LogService.ErrorE(this, "Remove file failed" , err);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;

                _index = 0;
                _totalSize = 0;
            }
        }

        #endregion
    }
}
