﻿using System;
using SyncTool.Common;

namespace SyncTool.Configuration
{
    [Serializable]
    public class DuplicateSyncFolderException : DuplicateItemException
    {


        public DuplicateSyncFolderException(string name) : base($"A SyncFolder named '{name}' already exists")
        {
            
        }

    }
}