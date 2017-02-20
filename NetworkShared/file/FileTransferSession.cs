using System;
using System.Collections.Generic;
using System.Text;
using com.ideadynamo.foundation.channels;
using com.tieao.mmo.deamon.server;
using com.tieao.mmo.deamon.client;

namespace com.tieao.network.file
{
    public class FileTransferSession
    {
        public FileSendSession SendSession { get { return _fileSendSession; } }
        public FileReceiveSession ReceiveSession { get { return _fileReceiveSession; } }

        FileSendSession _fileSendSession;
        FileReceiveSession _fileReceiveSession;

        public void Setup(IConnection connection) 
        {
            _fileSendSession = new FileSendSession(connection);
            _fileReceiveSession = new FileReceiveSession(connection);
        }

        public void OnClosed() 
        {
        }

        public bool HandleProtocol(com.ideadynamo.foundation.buffer.ByteArray buffer) 
        {
            if (_fileReceiveSession != null)
                if (FileServerHelper.IntepretMessage(buffer, _fileReceiveSession))
                    return true;

            if (_fileSendSession != null)
                if (FileClientHelper.IntepretMessage(buffer, _fileSendSession))
                    return true;

            return false;
        }

        public void SendDirectory(string dir, string remotedir)
        {
            if (_fileSendSession != null)
                _fileSendSession.SendDirectory(dir, remotedir);
        }

        public void SetWorkingDir(string dir)
        {
            if (_fileSendSession != null)
                _fileSendSession.SetWorkingDir(dir);
        }

        public void SendFile(string local, string remote)
        {
            if (_fileSendSession != null)
                _fileSendSession.SendFile(local, remote);
        }

        public void CreateDirectory(string dir)
        {
            if (_fileSendSession != null)
                _fileSendSession.CreateDirectory(dir);
        }

        public void GetFile(string file , string remotefile) 
        {
            if (_fileReceiveSession != null)
                _fileReceiveSession.GetFile(file , remotefile);
        }

        public void GetDirectory(string remotedir , string localdir)
        {
            if (_fileReceiveSession != null)
                _fileReceiveSession.GetDirectory(remotedir , localdir);
        }
    }
}
