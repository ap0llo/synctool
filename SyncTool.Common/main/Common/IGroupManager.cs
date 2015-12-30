﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Common
{
    public interface IGroupManager
    {
        IEnumerable<string> Groups { get; }

        IGroup GetGroup(string name);

        void AddGroup(string name);

        void RemoveGroup(string name);
    }
}