﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Base class for implementations of <see cref="IDirectory"/>
    /// </summary>
    public abstract class AbstractDirectory : FileSystemItem, IDirectory
    {
        public abstract IEnumerable<IDirectory> Directories { get; }

        public abstract IEnumerable<IFile> Files { get; }

        public IFileSystemItem this[string name]
        {
            get
            {
                if (FileExists(name))
                {
                    return GetFile(name);
                }
                return GetDirectory(name);
            }
        }


        protected AbstractDirectory(IDirectory parent, string name) : base(parent, name)
        {
        }


        public virtual IDirectory GetDirectory(string path)
        {
            PathValidator.EnsurePathIsValid(path);
            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return GetDirectoryByName(localName);
            }
            else
            {
                return GetDirectoryByName(localName).GetDirectory(remainingPath);
            }
        }

        public virtual IFile GetFile(string path)
        {
            PathValidator.EnsurePathIsValid(path);

            string localName;
            string remainingPath;            
            ParsePath(path, out localName, out remainingPath);       
            
            if(remainingPath == "")
            {
                return GetFileByName(localName);
            }
            else
            {
                return GetDirectoryByName(localName).GetFile(remainingPath);
            }
        }

        public virtual IFile GetFile(IFileReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            // first get file by path
            var file = GetFile(reference.Path);

            if (!reference.Matches(file))
            {
                throw new FileNotFoundException("A file at the specified path was found, but does not match ther reference");
            }
            else
            {
                return file;
        }
        }


        public virtual bool FileExists(string path)
        {
            PathValidator.EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return FileExistsByName(localName);
            }
            else if(DirectoryExistsByName(localName))
            {
                return GetDirectoryByName(localName).FileExists(remainingPath);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a file with the specified name exists in this current directory.
        /// Only direct children of the directory are considered, name must not be a path into a subdirectory       
        /// </summary>
        protected abstract bool FileExistsByName(string name);


        public virtual bool DirectoryExists(string path)
        {
            PathValidator.EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return DirectoryExistsByName(localName);                
            }
            else if (DirectoryExistsByName(localName))
            {
                return GetDirectoryByName(localName).DirectoryExists(remainingPath);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a directory with the specified name exists in this directory.
        /// Only direct children of the directory are considered, name must not be a path into a subdirectory       
        /// </summary>
        protected abstract bool DirectoryExistsByName(string name);

        /// <summary>
        /// Gets the file with the specified name from the directory
        /// Only direct children of the directory are considered, name must not be a path into a subdirectory       
        /// </summary>
        protected abstract IFile GetFileByName(string name);

        /// <summary>
        /// Gets the directory with the specified name from the directory
        /// Only direct children of the directory are considered, name must not be a path into a subdirectory       
        /// </summary>
        protected abstract IDirectory GetDirectoryByName(string name);
        

                       

        void ParsePath(string path, out string localName, out string remainingPath)
        {
            if (path.Contains(Constants.DirectorySeparatorChar))
            {
                var splitIndex = path.IndexOf(Constants.DirectorySeparatorChar);
                localName = path.Substring(0, splitIndex);
                remainingPath = path.Substring(splitIndex + 1);
            }
            else
            {
                localName = path;
                remainingPath = "";
            }
        }

        
        
    }
}