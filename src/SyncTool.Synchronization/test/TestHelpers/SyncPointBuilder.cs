﻿using System;
using System.Collections.Generic;
using SyncTool.Configuration.Model;
using SyncTool.Synchronization.State;
using System.Linq;

namespace SyncTool.TestHelpers
{
    public static class SyncPointBuilder
    {

        public static MutableSyncPoint NewSyncPoint()
        {
            return new MutableSyncPoint();
        }


        public static MutableSyncPoint WithId(this MutableSyncPoint state, int id)
        {
            state.Id = id;
            return state;
        }


        public static MutableSyncPoint WithMultiFileSystemSnapshotId(this MutableSyncPoint state, string id)
        {            
            state.MultiFileSystemSnapshotId = id;            
            return state;
        }
        
    }
}
