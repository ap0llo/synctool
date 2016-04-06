﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.Synchronization.State;

namespace SyncTool.TestHelpers
{
    public static class SynchronizationStateBuilder
    {

        public static MutableSyncPoint NewSynchronizationState()
        {
            return new MutableSyncPoint();
        }


        public static MutableSyncPoint WithId(this MutableSyncPoint state, int id)
        {
            state.Id = id;
            return state;
        }



        public static MutableSyncPoint WithoutFromSnapshots(this MutableSyncPoint state)
        {
            state.FromSnapshots = null;            
            return state;
        }

        public static MutableSyncPoint WithToSnapshot(this MutableSyncPoint state, string name, string id)
        {
            Dictionary<string, string> current;
            try
            {
                current = (Dictionary<string, string>) state.ToSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? new Dictionary<string, string>();

            current.Add(name, id);
            state.ToSnapshots = current;
            
            return state;
        }

        public static MutableSyncPoint WithFromSnapshot(this MutableSyncPoint state, string name, string id)
        {
            Dictionary<string, string> current;
            try
            {
                current = (Dictionary<string, string>)state.FromSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? new Dictionary<string, string>();

            current.Add(name, id);
            state.FromSnapshots = current;

            return state;
        }


    }
}