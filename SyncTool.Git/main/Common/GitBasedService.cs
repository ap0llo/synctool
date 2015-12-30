﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.Common;

namespace SyncTool.Git.Common
{
    public class GitBasedService : IService
    {

        public IGroup Group => GitGroup;
        
        protected GitBasedGroup GitGroup { get; }


        public GitBasedService(GitBasedGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            GitGroup = group;
        }




    }
}