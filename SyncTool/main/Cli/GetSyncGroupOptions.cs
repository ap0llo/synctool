﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using CommandLine;

namespace SyncTool.Cli
{
    [Verb("Get-Group")]
    public class GetSyncGroupOptions
    {

        [Option(Required = false)]
        public string Name { get; set; }
        
    }
}