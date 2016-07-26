﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemDiff
    {
        IMultiFileSystemSnapshot FromSnapshot { get; } 

        IMultiFileSystemSnapshot ToSnapshot { get; }

        IEnumerable<IChangeList> ChangeLists { get; }
        
        //TODO: Added changes to the histories itself (added/removed/modified/unchanged)

    }
}