﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Configuration
{
    [Serializable]
    public class SyncGroupNotFoundException : ConfigurationException
    {

        public SyncGroupNotFoundException(string name) : this(name, null)
        {
            
        } 

        public SyncGroupNotFoundException(string name, Exception innerException) : base($"The SyncGroup '{name}' could not be found", innerException)
        {
        }
    }
}