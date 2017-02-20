using System;
using System.Collections.Generic;
using System.Text;

namespace com.tieao.network.file
{
    public enum FileTransferError
    {
        TIMEOUT,
        NOTEXIST,
        FAILED,
        None,
    }

    public interface IFileTransferCallback
    {
        void OnSendStarted(int totalSize, int firstSent);
        void OnSendProgress(int thisChunk, int totalSize, int sentSize);
        void OnSendFailed(FileTransferError error);
        int TimeoutTick { get; }
    }
}
