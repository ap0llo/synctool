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


        public virtual IDirectory GetDirectory(string path) => GetFileSystemItem(path, GetDirectoryByName, (dir, relativePath) => dir.GetDirectory(relativePath));

        public virtual IFile GetFile(string path) => GetFileSystemItem(path, GetFileByName, (dir, relativePath) => dir.GetFile(relativePath));

        public IFile GetFile(IFileReference reference)
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
            return FileSystemItemExists<IFile>(path, FileExistsByName, (dir, relativePath) => dir.FileExists(relativePath));
        }

        public bool FileExists(IFileReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (FileExists(reference.Path))
            {
                var file = GetFile(reference.Path);
                return reference.Matches(file);
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
            return FileSystemItemExists<IDirectory>(path, DirectoryExistsByName, (dir, relativePath) => dir.DirectoryExists(relativePath));
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
        

                       

        void ParseRelativePath(string path, out string localName, out string remainingPath)
        {        
            if (path.Contains(Constants.DirectorySeparatorChar))
            {
                
                // remove the first name from the path (that's the name of this directory's child directory -> 'localName')
                var splitIndex = path.IndexOf(Constants.DirectorySeparatorChar);
                localName = path.Substring(0, splitIndex);

                // remainingPath is the path relative to the child director
                remainingPath = path.Substring(splitIndex + 1);                
            }
            else
            {
                localName = path;
                remainingPath = "";
            }
        }



        private T GetFileSystemItem<T>(string path, Func<string, T> getFileSystemItemByNameFunction, Func<IDirectory, string, T> getFileSystemItemFunction)
        {
            PathValidator.EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParseRelativePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return getFileSystemItemByNameFunction.Invoke(localName);
            }
            else
            {
                return getFileSystemItemFunction.Invoke(GetDirectoryByName(localName), remainingPath);
            }
        }

        public virtual bool FileSystemItemExists<T>(string path, Func<string, bool> existsByNameFunction, Func<IDirectory, string, bool> existsFunction)
        {
            PathValidator.EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParseRelativePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return existsByNameFunction.Invoke(localName);
            }
            else if (DirectoryExistsByName(localName))
            {
                return existsFunction(GetDirectoryByName(localName), remainingPath);
            }
            else
            {
                return false;
            }
        }

    }
}