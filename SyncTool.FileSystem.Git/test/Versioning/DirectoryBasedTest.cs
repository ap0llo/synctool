﻿using System;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.Git
{
    public abstract class DirectoryBasedTest : IDisposable
    {

        readonly LocalItemCreator m_DirectoryCreator = new LocalItemCreator();
        protected readonly TemporaryLocalDirectory m_TempDirectory;


        protected DirectoryBasedTest()
        {
            m_TempDirectory = m_DirectoryCreator.CreateTemporaryDirectory();
        }


        public virtual void Dispose()
        {
            m_TempDirectory.Dispose();
        }
    }
}