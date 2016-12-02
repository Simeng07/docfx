﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;

    public class FileAbstractLayer : IFileAbstractLayer
    {
        #region Constructors

        internal FileAbstractLayer(IFileReader reader, IFileWriter writer)
        {
            Reader = reader;
            Writer = writer;
        }

        #endregion

        #region Public Members

        public IFileReader Reader { get; }

        public IFileWriter Writer { get; }

        #endregion

        #region IFileAbstractLayer Members

        public bool CanWrite => Writer != null;

        public IEnumerable<RelativePath> GetAllInputFiles()
        {
            EnsureNotDisposed();
            return Reader.EnumerateFiles();
        }

        public IEnumerable<RelativePath> GetAllOutputFiles()
        {
            EnsureNotDisposed();
            if (!CanWrite)
            {
                throw new InvalidOperationException();
            }
            return Writer.CreateReader().EnumerateFiles();
        }

        public bool Exists(RelativePath file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            EnsureNotDisposed();
            return Reader.FindFile(file) != null;
        }

        public FileStream OpenRead(RelativePath file)
        {
            EnsureNotDisposed();
            var pp = FindPhysicalPath(file);
            return File.OpenRead(pp.PhysicalPath);
        }

        public FileStream Create(RelativePath file)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException();
            }
            EnsureNotDisposed();
            return Writer.Create(file);
        }

        public void Copy(RelativePath sourceFileName, RelativePath destFileName)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException();
            }
            EnsureNotDisposed();
            var mapping = FindPhysicalPath(sourceFileName);
            Writer.Copy(mapping, destFileName);
        }

        public ImmutableDictionary<string, string> GetProperties(RelativePath file)
        {
            EnsureNotDisposed();
            var mapping = FindPhysicalPath(file);
            return mapping.Properties;
        }

        #endregion

        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Private Methods

        private PathMapping FindPhysicalPath(RelativePath file)
        {
            var mapping = Reader.FindFile(file);
            if (mapping == null)
            {
                string fn = file;
                throw new FileNotFoundException($"File ({fn}) not found.", fn);
            }
            return mapping.Value;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("FileAbstractLayer");
            }
        }

        #endregion
    }
}
