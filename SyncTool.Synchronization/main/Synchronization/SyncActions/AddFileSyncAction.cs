﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class AddFileSyncAction : SyncAction
    {
        public override string FilePath => NewFile.Path;

        public IFileReference NewFile { get; }


        public AddFileSyncAction(Guid id, string target, SyncActionState state, IFileReference newFile) : base(id, target, state)
        {
            if (newFile == null)
            {
                throw new ArgumentNullException(nameof(newFile));
            }
            this.NewFile = newFile;
        }
    }
}