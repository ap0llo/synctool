﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Configuration
{
    public class SyncGroupNotFoundException : ConfigurationException
    {
        public SyncGroupNotFoundException(string name) : base($"The SyncGroup '{name}' could not be found")
        {
        }
    }
}