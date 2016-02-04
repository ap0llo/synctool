﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Common
{
    /// <summary>
    /// Base interface for all services
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Gets the group the service is associated to
        /// </summary>
        IGroup Group { get; }         
    }
}