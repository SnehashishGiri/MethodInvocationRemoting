/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using OperatingSystemAbstraction;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: FileRemoteSender
    //
    //******************************************************************************
    /// <summary>
    /// Sends messages to a remote location via the file system.
    /// </summary>
    public class FileRemoteSender : IRemoteSender, IDisposable
    { 
        private IFile messageFile;
        private IFile lockFile;
        private IFileSystem fileSystem;
        private string lockFilePath;
        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        protected bool disposed;

        //******************************************************************************
        //
        // Method: FileRemoteSender (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.FileRemoteSender class.
        /// </summary>
        /// <param name="messageFilePath">The full path of the file used to send messages.</param>
        /// <param name="lockFilePath">The full path of the file used to indicate when the message file is locked for writing.</param>
        public FileRemoteSender(string messageFilePath, string lockFilePath)
        {
            if (messageFile == null)
            {
                messageFile = new File(messageFilePath);
            }
            if (lockFile == null)
            {
                lockFile = new File(lockFilePath);
            }
            if (fileSystem == null)
            {
                fileSystem = new FileSystem();
            }
            this.lockFilePath = lockFilePath;
            disposed = false;
        }

        //******************************************************************************
        //
        // Method: FileRemoteSender (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.FileRemoteSender class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="messageFilePath">The full path of the file used to send messages.</param>
        /// <param name="lockFilePath">The full path of the file used to indicate when the message file is locked for writing.</param>
        /// <param name="messageFile">A test (mock) message file.</param>
        /// <param name="lockFile">A test (mock) lock file.</param>
        /// <param name="fileSystem">A test (mock) file system.</param>
        public FileRemoteSender(string messageFilePath, string lockFilePath, IFile messageFile, IFile lockFile, IFileSystem fileSystem)
            : this(messageFilePath, lockFilePath)
        {
            this.messageFile = messageFile;
            this.lockFile = lockFile;
            this.fileSystem = fileSystem;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteSender.Send(System.String)"]/*'/>
        public void Send(string message)
        {
            CheckNotDisposed();

            try
            {
                // Lock file is created before data is written to the message file
                //   The FileRemoteReceiver class checks for the absence of the lock file to prevent attempting to open the message file when it is partially written and causing an exception
                lockFile.WriteAll("");
                messageFile.WriteAll(message);
                fileSystem.DeleteFile(lockFilePath);
            }
            catch (Exception e)
            {
                throw new Exception("Error sending message.", e);
            }
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the FileRemoteSender.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileRemoteSender()
        {
            Dispose(false);
        }

        //******************************************************************************
        //
        // Method: Dispose
        //
        //******************************************************************************
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                messageFile.Dispose();
                lockFile.Dispose();
                // Set large fields to null.
                lockFilePath = null;
                disposed = true;
            }
        }

        //******************************************************************************
        //
        // Method: CheckNotDisposed
        //
        //******************************************************************************
        /// <summary>
        /// Throws an exception if the disposed property is true.
        /// </summary>
        protected void CheckNotDisposed()
        {
            if (disposed == true)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion
    }
}
