﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Synchronization.State;

namespace SyncTool.Git.FileSystem
{
    public class MutableSynchronizationState : ISynchronizationState
    {
        public int Id { get; set; }

        public IReadOnlyDictionary<string, string> FromSnapshots { get; set; }

        public IReadOnlyDictionary<string, string> ToSnapshots { get; set; }
    }
}