using System;
using System.Collections.Generic;
using System.Text;
using com.ideadynamo.foundation.game;
using com.ideadynamo.foundation.buffer;
using com.ideadynamo.foundation.log;
using System.IO;
using com.tieao.mmo.deamon.server;
using com.tieao.mmo.deamon;
using com.ideadynamo.foundation.channels;

namespace com.tieao.network.file
{
    public class FileReceiveSession : IFileServerService
    {
        public string BaseDir = string.Empty;
        public static int MaxFileUploadSize = 1024 * 1024 * 1024; // max 1G file to upload

        ByteArray _buffer = new ByteArray(1024 * 16);
        //int _sentSize = 0;
        FileAssembler _fileAssembler;
        int _toUploadSize;
        string _currentDirectory = string.Empty;
        IConnection _connection;

        public FileReceiveSession(IConnection connection)
        {
            _connection = connection;
        }

        #region IFileServerService Members

        public void OnRequestFileSend(string file, int uploadToken, int size)
        {
            if (size > MaxFileUploadSize)
            {
                _connection.Send(FileServerHelper.FileSendResponse(0, -2));
            }
            else if (/*Service.ValidateUploadToken(uploadToken)*/true)
            {
                _fileAssembler = FileAssembler.CreateAssembler(BaseDir + "/"+ _currentDirectory + "/" + file);
                LogService.Info(this, "Begin receiving file: {0}", _currentDirectory + "/" + file);
                if (_fileAssembler != null)
                {
                    _connection.Send(FileServerHelper.FileSendResponse(0, size));
                    _toUploadSize = size;
                }
                else
                {
                    _connection.Send(FileServerHelper.FileSendResponse(0, -2));
                }
            }
            else
            {
                _connection.Send(FileServerHelper.FileSendResponse(0, -1));
            }
        }
       
        public void OnRequestFileBlock(string file, int blockId, int size, ref com.ideadynamo.foundation.buffer.ByteArray stream, int special)
        {
            if (_fileAssembler != null)
            {
                int thisBlockSize = stream.Buffer.Length - ByteArray.HEADERLENGTH;
                _fileAssembler.Append(blockId, stream.Buffer, ByteArray.HEADERLENGTH, thisBlockSize);
                _connection.Send(FileServerHelper.FileBlockResponse(0, size));
                _toUploadSize -= thisBlockSize;

                if (special != 0 || _toUploadSize <= 0)
                {
                    LogService.Info(this, "Receive file {0} success, total size {1}" , file, _fileAssembler.Size);
                    _fileAssembler.Dispose();
                    _fileAssembler = null;
                }
            }
        }
        public void OnFileBlockResponse(int file, int blockId)
        {
        }
        
        public void OnSendFinishResponse(int file, int totalSize, int expectSize, string md5)
        {

        }
        
        /*public void OnRequetFileDownload(string file, int uploadToken)
        {
            if (File.Exists(file))
            {
                try
                {
                    using (FileStream stream = new FileStream(file, FileMode.Open))
                    {
                        int size = stream.Read(_buffer.Buffer, ByteArray.HEADERLENGTH, 1024 * 16 - ByteArray.HEADERLENGTH);
                        _sentSize = size;

                        _buffer.Reset();
                        _buffer.AdjustWriter(ByteArray.HEADERLENGTH + size);
                        _buffer.RefreshHeader();
                        _connection.Send(FileServerHelper.DownFileBlockResponse(file, 0, size, _buffer, 0));
                    }
                }
                catch (Exception err)
                {
                    LogService.ErrorE(this, "Failed to start file downloading", err);
                    _connection.Send(FileServerHelper.FileDownloadResponse(file, -2));
                }
            }
            else
            {
                _connection.Send(FileServerHelper.FileDownloadResponse(file, -1));
            }
        }
        
        public void OnFileBlockResponse(int file, int blockId, int blockSize, int receivedSize, int expectSize)
        {
            throw new NotImplementedException();
        }

        public void OnDownloadFinishResponse(int file, int totalSize, string md5)
        {
            throw new NotImplementedException();
        }
        */
        public void OnCreateDirectory(string name)
        {
            if (Directory.Exists(BaseDir + _currentDirectory))
            {
                try
                {
                    Directory.CreateDirectory(BaseDir + _currentDirectory + "/" + name);
                    LogService.Info(this, "Create folder {0} under {1} success", name, _currentDirectory);
                   _connection.Send(FileServerHelper.CreateDirectoryResult(0, name));
                }
                catch (Exception err)
                {
                    LogService.ErrorE(this, string.Format("failed to create directory {0} under {1}", name, _currentDirectory), err);
                    _connection.Send(FileServerHelper.CreateDirectoryResult(-1, err.Message));
                }
            }
            else
            {
                LogService.Warn(this, "Directory {0} does not exist", name);
                _connection.Send(FileServerHelper.CreateDirectoryResult(-2, name));
            }

        }

        public void OnListDirectorys(string name)
        {
            StringSequence strings = new StringSequence();
            if (Directory.Exists(BaseDir + _currentDirectory))
            {
                try
                {
                    string[] dirs = Directory.GetDirectories(BaseDir + _currentDirectory + "/" + name);
                    LogService.Info(this, "List folders under {0} success", name);

                    for (int i = 0; i < dirs.Length; i++)
                    {
                        strings.GetElements().AddRange(dirs);
                    }

                    _connection.Send(FileServerHelper.ListDirectorysResult(0, strings));
                    return;
                }
                catch (Exception err)
                {
                    LogService.ErrorE(this, string.Format("failed to list directory {0} under {1}", name, _currentDirectory), err);

                    strings.Add(err.Message);
                    _connection.Send(FileServerHelper.ListDirectorysResult(-1, strings));
                }
            }
            else
            {
                LogService.Warn(this, "Directory {0} does not exist", name);
                _connection.Send(FileServerHelper.ListDirectorysResult(-2, strings));
            }
        }

        public void OnDeleteDirectory(string name)
        {
            if (Directory.Exists(BaseDir + _currentDirectory))
            {
                DirectoryInfo dir = new DirectoryInfo(BaseDir + _currentDirectory + "/" + name);
                if (dir.Exists)
                {
                    try
                    {
                        Directory.Delete(dir.FullName);
                        _connection.Send(FileServerHelper.DeleteDirectoryResult(0, dir.FullName));
                    }
                    catch (Exception err)
                    {
                        LogService.ErrorE(this, string.Format("failed to delete directory {0} under {1}", name, _currentDirectory), err);
                        _connection.Send(FileServerHelper.DeleteDirectoryResult(-1, err.Message));
                    }

                    return;
                }
            }

            LogService.Warn(this, "Directory {0} does not exist", name);
            _connection.Send(FileServerHelper.DeleteDirectoryResult(-2, name));

        }

        public void OnDeleteFile(string name)
        {
            if (Directory.Exists(BaseDir + _currentDirectory))
            {
                FileInfo file = new FileInfo(BaseDir + _currentDirectory + "/" + name);
                if (file.Exists)
                {
                    try
                    {
                        Directory.Delete(file.FullName);
                        _connection.Send(FileServerHelper.DeleteDirectoryResult(0, file.FullName));
                        LogService.Info(this, "delete file {0} success", name);
                    }
                    catch (Exception err)
                    {
                        LogService.ErrorE(this, string.Format("failed to delete file {0} under {1}", name, _currentDirectory), err);
                        _connection.Send(FileServerHelper.DeleteDirectoryResult(-1, err.Message));
                    }

                    return;
                }
            }

            LogService.Warn(this, "Directory {0} does not exist", name);
            _connection.Send(FileServerHelper.DeleteDirectoryResult(-2, name));
        }

        public void OnSetWorkingDirectory(string name)
        {
            if (Directory.Exists(BaseDir + "/" + name))
            {
                _currentDirectory = name;
                _connection.Send(FileServerHelper.SetWorkingDirectoryResult(0, name));
            }
            else
            {
                LogService.Warn(this, "Directory {0} does not exist", name);
                _connection.Send(FileServerHelper.SetWorkingDirectoryResult(-1, name));
            }
        }
        public void OnSendDirectoryFailed()
        {
            LogService.Warn(this, "Failed to send directory");
        }

        public void OnSendFileFailed(string filename, string reason)
        {
            LogService.Warn(this, "Failed to send file: {0} , {1}" , filename, reason);
        }

        public void OnStartBatch(string name)
        {
            LogService.Info(this, "Start new batch {0}", name);
        }

        public void OnBatchComplete(bool success, string name)
        {
            LogService.Info(this, "Batch {0} completed {1}", name, success ? "success" : "failed");
        }


        #endregion

        public void GetFile(string file , string localfile) 
        {
            _connection.Send(FileServerHelper.RequestFileDownload(file, localfile, false));
        }

        public void GetDirectory(string dir, string localdir) 
        {
            _connection.Send(FileServerHelper.RequestFileDownload(dir, localdir, true));            
        }

      
    }
}
