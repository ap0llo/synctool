﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemDiff
    {
         
        /// <summary>
        /// The snapshot that serves as base of the diff        
        /// </summary>
        /// <remarks>Might be null when the diff describes all changes ever made to a file system</remarks>
        IFileSystemSnapshot FromSnapshot { get; }

        /// <summary>
        /// The latest snapshot that was included in the diff
        /// </summary>
        IFileSystemSnapshot ToSnapshot { get; }


        /// <summary>
        /// The list of changes between the two snapshots
        /// </summary>
        IEnumerable<IChange> Changes { get; } 

    }
}