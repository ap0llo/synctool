﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class ReplaceFileSyncAction : SyncAction
    {
        public override string FilePath => OldVersion.Path;

        public IFile OldVersion { get; }

        public IFile NewVersion { get; }


        public ReplaceFileSyncAction(Guid id, SyncParticipant target, IFile oldVersion, IFile newVersion) : base(id, target)
        {
            if (oldVersion == null)
            {
                throw new ArgumentNullException(nameof(oldVersion));
            }
            if (newVersion == null)
            {
                throw new ArgumentNullException(nameof(newVersion));
            }
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(oldVersion.Path, newVersion.Path))
            {
                throw new ArgumentException($"The paths of {nameof(oldVersion)} and {nameof(newVersion)} are differnet");
            }

            this.OldVersion = oldVersion;
            this.NewVersion = newVersion;

        }

        public override void Accept<T>(ISyncActionVisitor<T> visitor, T parameter)
        {
            visitor.Visit(this, parameter);
        }
    }
}