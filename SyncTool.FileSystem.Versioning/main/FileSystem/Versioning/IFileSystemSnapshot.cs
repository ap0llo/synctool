﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemSnapshot
    {
        /// <summary>
        /// The id uniquely identifying the snapshot
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The time the snapshot was created
        /// </summary>
        DateTime CreationTime { get; } 

        /// <summary>
        /// The state of the file system stored in the snapshot
        /// </summary>
        IDirectory RootDirectory { get; }
    }
}