﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Synchronization.State
{
    public interface ISynchronizationState
    {
        /// <summary>
        /// Gets the id of the synchronization point
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the ids of the oldest snapshots that were included in the sync
        /// </summary>
        IReadOnlyDictionary<string, string> FromSnapshots { get; }

        /// <summary>
        /// Gets the ids for of the latest snapshots that were included in the sync
        /// </summary>
        IReadOnlyDictionary<string, string> ToSnapshots { get; }

    }
}