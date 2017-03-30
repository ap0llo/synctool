﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem.Versioning.MetaFileSystem
{
    public class FileSystemToMetaFileSystemConverter
    {


        public IDirectory CreateMetaDirectory(IDirectory directory) => CreateMetaDirectory(null, directory);

        public IDirectory CreateMetaDirectory(IDirectory parentDirectory, IDirectory directory)
        {
            var newDirectory = new Directory(parentDirectory, directory.Name);

            foreach (var dir in directory.Directories)
            {
                newDirectory.Add(d => CreateMetaDirectory(d, dir));
            }

            foreach (var file in directory.Files)
            {
                newDirectory.Add(d => FilePropertiesFile.ForFile(d, file));
            }
            
            newDirectory.Add(d => DirectoryPropertiesFile.ForDirectory(d, directory));
            
            return newDirectory;
        }
        

    }
}