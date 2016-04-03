﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace SyncTool.Synchronization.SyncActions
{
    public abstract class SyncAction
    {        
        public abstract string FilePath { get; }

        public SyncActionState State { get; }

        public string Target { get; }

        public Guid Id { get; }

        protected SyncAction(Guid id, string target, SyncActionState state)
        {
            this.Target = target;
            this.Id = id;
            this.State = state;
        }
        
    }
}