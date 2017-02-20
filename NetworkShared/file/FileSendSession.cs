using System;
using System.Collections.Generic;
using System.Text;
using com.ideadynamo.foundation.game;
using System.IO;
using com.ideadynamo.foundation.buffer;
using com.ideadynamo.foundation.log;
using com.tieao.mmo.deamon.client;
using com.ideadynamo.foundation.channels;

namespace com.tieao.network.file
{
    public class FileSendSession : IFileClientService , IDisposable
    {
        public Stream Stream { get; set; }
        public IFileTransferCallback Callback { get; set; }
        public String FileName { get; set; }
        public string BaseDir { get { return _baseDir; } set { _baseDir = value; } }
        public string CurrentDir { get { return _currentDir; } set { _currentDir = value; } }

        public int UploadTag;

        ByteArray _buffer = new ByteArray(1024 * 16);
        int _sentSize = 0;
        bool _isDone;
        IConnection _connection;
        string _baseDir = string.Empty;
        string _currentDir = string.Empty;
        
        SendFileRequest _request = new SendFileRequest();

        public FileSendSession(IConnection connection) 
        {
            _connection = connection;
        }

        #region IFileClientService Members

        public void OnFileSendResponse(int file, int uploadToken)
        {
            _isDone = false;
            //Client.ResetTimeout();

            if (Stream == null)
            {
                try
                {
                    Stream = new FileStream(FileName, FileMode.Open);
                }
                catch (IOException err) 
                {
                    _connection.Send(FileClientHelper.SendFileFailed(FileName, err.Message));
                    if (_request.IsType(SendFileRequest.CommandType.SENDFILE)) 
                    {
                        _connection.Send(FileClientHelper.SendDirectoryFailed());
                    }
                    return;
                }
            }
            {
                int size = Stream.Read(_buffer.Buffer, ByteArray.HEADERLENGTH, 1024 * 16 - ByteArray.HEADERLENGTH);
                _sentSize = size;

                _buffer.Reset();
                _buffer.AdjustWriter(ByteArray.HEADERLENGTH + size);
                _buffer.RefreshHeader();
                _connection.Send(FileClientHelper.RequestFileBlock(FileName, 0, size, _buffer, 0));
                if (Callback != null)
                {
                    Callback.OnSendStarted((int)Stream.Length, size);
                }
            }
        }

        public void OnFileBlockResponse(int file, int blockId)
        {
            if (Stream == null)
            {
                try
                {
                    Stream = new FileStream(FileName, FileMode.Open);
                }
                catch (IOException err)
                {
                    _connection.Send(FileClientHelper.SendFileFailed(FileName, err.Message));
                    if (_request.IsType(SendFileRequest.CommandType.SENDFILE))
                    {
                        _connection.Send(FileClientHelper.SendDirectoryFailed());
                    }
                    return;
                }
            }

            //Client.ResetTimeout();
            Stream.Seek(_sentSize, SeekOrigin.Begin);
            int size = Stream.Read(_buffer.Buffer, ByteArray.HEADERLENGTH, 1024 * 16 - ByteArray.HEADERLENGTH);
            _sentSize += size;
            if (size != 0)
            {
                _buffer.Reset();
                _buffer.AdjustWriter(ByteArray.HEADERLENGTH + size);
                _buffer.RefreshHeader();
                _connection.Send(FileClientHelper.RequestFileBlock(FileName, ++blockId, size, _buffer, _sentSize >= Stream.Length ? -1 : 0));

                if (Callback != null)
                {
                    Callback.OnSendProgress(size, (int)Stream.Length, _sentSize);
                }
            }
            else
            {
                if (Callback != null && !_isDone)
                {
                    Callback.OnSendFailed(FileTransferError.None);
                    _isDone = true;
                }

                //Client.FinishSending();
                Stream.Dispose();
                Stream = null;

                if (_request.CompleteCommand(SendFileRequest.CommandType.SENDFILE))
                {
                    _request.ExecuteNext(this);
                }

            }
        }

        public void OnSendFinishResponse(int file, int totalSize, int expectSize)
        {
            //ResetTimeout();
            _isDone = true;
            LogService.Info(this, "File send success");
        }

        public void OnClientRequestError_(int errorCode)
        {

        }

        public void OnFileDownloadResponse(string file, int id)
        {
            LogService.Info(this, string.Format("OnFileDownloadResponse('{0}' , {1})", file, id));
            if (Callback != null)
            {
                Callback.OnSendStarted(0, id);
            }
        }

        public void OnDownFileBlockResponse(string file, int blockId, int size, ref ByteArray stream, int special)
        {

        }


        public void OnSendFinishResponse(int file, int totalSize, int expectSize, string md5)
        {

        }

        public void OnCreateDirectoryResult(int result, string desc)
        {
            LogService.Info(this, "Create Directory {0} / {1}", result, desc);

            if (_request.CompleteCommand(SendFileRequest.CommandType.CREATEDIR))
            {
                _request.ExecuteNext(this);
            }
        }

        public void OnListDirectorysResult(int result, ref com.tieao.mmo.deamon.StringSequence directories)
        {
            LogService.Info(this, "List Directory result {0} ", result);
            for (int i = 0; i < directories.GetElements().Count; i++)
            {
                LogService.Info(this, "    Directory {0}: {1} ", i, directories.GetElements()[i]);
            }
        }

        public void OnDeleteDirectoryResult(int result, string desc)
        {
            throw new NotImplementedException();
        }

        public void OnDeleteFileResult(int result, string desc)
        {
            throw new NotImplementedException();
        }

        public void OnSetWorkingDirectoryResult(int result, string desc)
        {
            LogService.Info(this, "Set working directory to {0} result {1}", result, desc);

            if (_request.CompleteCommand(SendFileRequest.CommandType.SETWORKINGDIR))
            {
                _request.ExecuteNext(this);
            }
        }
        public void OnRequestFileDownload(string file,string localfile, bool isdir)
        {
            if (isdir)
            {
                SendDirectory(file, localfile);
            }
            else
            {
                SendFile(file, localfile);
            }
        }
        #endregion

        public void SetWorkingDir(string dir)
        {
            _connection.Send(FileClientHelper.SetWorkingDirectory(dir));
        }

        public void CreateDirectory(string dir)
        {
            _connection.Send(FileClientHelper.CreateDirectory(dir));
        }

        public void SendDirectory(string dir , string remotedir)
        {
            if (!_request.InitDirectoryCommandQueue(dir, remotedir, BaseDir, CurrentDir)) 
            {
                _connection.Send(FileClientHelper.SendDirectoryFailed());
                return;
            }
            _connection.Send(FileClientHelper.StartBatch(remotedir));
            _request.ExecuteNext(this);
        }

        public void SendFile(string file, string remoteFile)
        {
            FileInfo f = new FileInfo(file);
            if (f.Exists)
            {
                FileName = f.FullName;
                //Callback = _callback;
                _connection.Send(FileClientHelper.RequestFileSend(remoteFile, UploadTag, (int)f.Length));
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Stream != null) 
            {
                Stream.Dispose();
                Stream = null;
            }
        }

        #endregion

    }
}
