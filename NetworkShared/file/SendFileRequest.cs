using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using com.ideadynamo.foundation.log;
using com.ideadynamo.foundation;

namespace com.tieao.network.file
{
    public class SendFileRequest
    {
        public enum CommandType
        {
            SETWORKINGDIR = 0,
            CREATEDIR = 1,
            SENDFILE=2,
        }

        public class Command 
        {
            public CommandType type;
            public string remote;
            public string local;
        }

        List<Command> _commands = new List<Command>();
        int _currentCommand = 0;
        public string batchTarget = string.Empty;
        public VoidDelegate OnDone;

        public bool IsType(CommandType type)
        {
            if (_currentCommand < 0 || _currentCommand >= _commands.Count)
                return false;

            return _commands[_currentCommand].type == type;
        }

        public bool CompleteCommand(CommandType type) 
        {
            if (IsType(type)) 
            {
                ++_currentCommand;
                return true;
            }

            return false;
        }

        public bool Completed 
        {
            get 
            {
                return _currentCommand >= _commands.Count;
            }
        }

        public bool InitDirectoryCommandQueue(string localDir , string remoteDir, string baseDir, string currentDir) 
        {
            DirectoryInfo directory = new DirectoryInfo(localDir);
            string localRelativeDir = localDir;
            if (!directory.Exists) 
            {
                localRelativeDir = baseDir + "/" + currentDir + "/" + localDir;
                directory = new DirectoryInfo(localRelativeDir);
            }
            if (!directory.Exists) return false;

            batchTarget = localDir;
            string localFullDir = directory.FullName;
            List<string> skipFiles = null;
            if (File.Exists(localFullDir)) 
            {
                skipFiles = new List<string>();
                using (StreamReader sr = new StreamReader(new FileStream(localFullDir, FileMode.Open))) 
                {
                    while (!sr.EndOfStream) 
                    {
                        skipFiles.Add(sr.ReadLine());
                    }
                }
            }
            _commands.Clear();
            _currentCommand = 0;
            
            AddDirectoryToCommandQueue(directory, remoteDir, _commands , skipFiles , localRelativeDir );
            _commands.Add(new Command() { type = CommandType.SETWORKINGDIR, remote = string.Empty });

            return true;
        }

        private void AddDirectoryToCommandQueue(DirectoryInfo directory, string remoteDir, List<Command> commands , List<String> skipFiles, string localRelativeDir)
        {
            //首先确保根目录存在
            _commands.Add(new Command() { type = CommandType.CREATEDIR, remote = remoteDir });
            
            DirectoryInfo [] dirs = directory.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                //Skip all .svn folders
                if (dirs[i].Name.StartsWith(".")) continue;
                if ((dirs[i].Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                if ((dirs[i].Attributes & FileAttributes.System) == FileAttributes.System) continue;
                AddDirectoryToCommandQueue(dirs[i], remoteDir + "/" + dirs[i].Name, _commands, skipFiles, localRelativeDir + "/" + dirs[i].Name);
            }

            _commands.Add(new Command() { type = CommandType.SETWORKINGDIR, remote = remoteDir });
            FileInfo[] files = directory.GetFiles();
            for (int j = 0; j < files.Length; j++)
            {
                if (skipFiles != null && skipFiles.Contains(localRelativeDir + "/" + files[j].Name)) continue;
                if (files[j].Name.EndsWith(".tmp")) continue;
                if (files[j].Name.EndsWith(".vshost.exe")) continue;
                _commands.Add(new Command() { type = CommandType.SENDFILE, local = files[j].FullName, remote = files[j].Name });
            }
        }

        public bool ExecuteNext(FileSendSession client) 
        {
            if (_commands.Count == 0) 
            {
                LogService.Info(this, "Command buffer is empty!");
                return false;
            }

            if (_currentCommand >= _commands.Count)
            {
                LogService.Info(this , "Command buffer finished executing!");
                if(OnDone != null) OnDone();

                return false;
            }

            Command cmd = _commands[_currentCommand];

            switch (cmd.type)
            {
                case CommandType.SETWORKINGDIR:
                    client.SetWorkingDir(cmd.remote);
                    break;
                case CommandType.CREATEDIR:
                    client.CreateDirectory(cmd.remote);
                    break;
                case CommandType.SENDFILE:
                    client.SendFile(cmd.local, cmd.remote);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}
