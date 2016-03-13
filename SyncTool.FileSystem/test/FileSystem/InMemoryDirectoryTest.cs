﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace SyncTool.FileSystem
{
    public class InMemoryDirectoryTest
    {
        readonly DerivedDirectory m_Root;
        DerivedDirectory m_Dir1;
        DerivedDirectory m_Dir11;
        File m_File1;


        public InMemoryDirectoryTest()
        {
            m_Root = new DerivedDirectory(null, "root")
            {
                root => new DerivedDirectory(root, "dir1")
                {
                    dir1 =>
                    {
                        m_Dir1 = (DerivedDirectory) dir1;
                        m_Dir11 = new DerivedDirectory(dir1, "dir11");
                        return m_Dir11;
                    },
                    dir1 =>
                    {
                        m_File1 = new File(dir1, "file1");
                        return m_File1;
                    }
                }
            };
        }


        #region GetFile()

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws ArgumentNullException if path is null")]
        public void GetFile_throws_ArgumentNullException_if_path_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Root.GetFile((string) null));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws ArgumentNullException if reference is null")]
        public void GetFile_throws_ArgumentNullException_if_reference_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Root.GetFile((IFileReference)null));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws FormatException if path is empty or whitespace")]
        public void GetFile_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile(""));
            Assert.Throws<FormatException>(() => m_Root.GetFile("  "));
        } 

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws FormatException if path contains a backslash")]
        public void GetFile_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("name\\name"));            
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws FormatException if path starts with separator char")]
        public void GetFile_throws_FormatException_if_path_starts_with_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("/name"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("/name/someOtherName"));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() throws FormatException if path ends with directory separator char")]
        public void GetFile_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("name/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("name/someOtherName/"));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() returns direct child")]
        public void GetFile_returns_direct_child()
        {
            var expected = m_File1;
            var actual = m_Dir1.GetFile("file1");

            Assert.Equal(expected, actual);
        }
       

        [Fact(DisplayName= nameof(InMemoryDirectory) + ".GetFile() returns children down in the hierarchy")]
        public void GetFile_returns_children_down_in_the_hierarchy()
        {
            var expected = m_File1;
            var actual = m_Root.GetFile("dir1/file1");

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() using reference returns children down in the hierarchy")]
        public void GetFile_using_reference_returns_children_down_in_the_hierarchy()
        {
            var expected = m_File1;
            var actual = m_Root.GetFile(new FileReference(m_File1.Path, m_File1.LastWriteTime, m_File1.Length));

            Assert.Equal(expected, actual);
        }


        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() using reference throws " + nameof(FileNotFoundException) + " if the LastWriteTime from the reference does not match")]
        public void GetFile_using_reference_throws_FileNotFoundException_if_the_LastWriteTime_from_the_reference_does_not_match()
        {
            var reference = new FileReference(m_File1.Path, m_File1.LastWriteTime.AddMinutes(1), m_File1.Length);
            Assert.Throws<FileNotFoundException>(() => m_Root.GetFile(reference));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetFile() using reference throws " + nameof(FileNotFoundException) + " if the Length from the reference does not match")]
        public void GetFile_using_reference_throws_FileNotFoundException_if_the_Length_from_the_reference_does_not_match()
        {
            var reference = new FileReference(m_File1.Path, m_File1.LastWriteTime, m_File1.Length + 2);
            Assert.Throws<FileNotFoundException>(() => m_Root.GetFile(reference));
        }


        #endregion

        #region FileExists

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".FileExists() returns expected result")]
        public void FileExists_returns_expected_result()
        {
            Assert.False(m_Root.FileExists("someFileName"));
            Assert.False(m_Root.FileExists("file1"));
            Assert.False(m_Root.FileExists("someDir/someFile"));         

            Assert.True(m_Root.FileExists("dir1/file1"));
            Assert.True(m_Dir1.FileExists("file1"));   
            
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".FileExists() using reference returns expected result")]
        public void FileExists_using_reference_returns_expected_result()
        {
            Assert.False(m_Root.FileExists(new FileReference("someFileName")));
            Assert.False(m_Root.FileExists(new FileReference("someFileName", DateTime.Now)));
            Assert.False(m_Root.FileExists(new FileReference("someFileName", DateTime.Now, 23)));
            Assert.False(m_Root.FileExists(new FileReference("someFileName", null, 42)));

            Assert.False(m_Root.FileExists(new FileReference("file1")));
            Assert.False(m_Root.FileExists(new FileReference("file1", DateTime.Now)));
            Assert.False(m_Root.FileExists(new FileReference("file1", DateTime.Now, 23)));
            Assert.False(m_Root.FileExists(new FileReference("file1", null, 23)));

            Assert.False(m_Root.FileExists(new FileReference("someDir/someFile")));
            Assert.False(m_Root.FileExists(new FileReference("someDir/someFile", DateTime.Now)));
            Assert.False(m_Root.FileExists(new FileReference("someDir/someFile", DateTime.Now, 42)));
            Assert.False(m_Root.FileExists(new FileReference("someDir/someFile", null, 23)));

            Assert.True(m_Root.FileExists(new FileReference("dir1/file1")));            
            Assert.True(m_Root.FileExists(new FileReference("dir1/file1", m_File1.LastWriteTime)));            
            Assert.True(m_Root.FileExists(new FileReference("dir1/file1", m_File1.LastWriteTime, m_File1.Length)));            
            Assert.True(m_Root.FileExists(new FileReference("dir1/file1", null, m_File1.Length)));

            
            Assert.False(m_Root.FileExists(new FileReference("dir1/file1", m_File1.LastWriteTime.AddHours(5))));
            Assert.False(m_Root.FileExists(new FileReference("dir1/file1", m_File1.LastWriteTime, m_File1.Length + 23)));
            Assert.False(m_Root.FileExists(new FileReference("dir1/file1", null, m_File1.Length +42)));

        }

        #endregion

        #region GetDirectory()

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() throws ArgumentNullException if path is null")]
        public void GetDirectory_throws_ArgumentNullException_if_path_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Root.GetDirectory(null));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() throws FormatException if path is empty or whitespace")]
        public void GetDirectory_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory(""));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("  "));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() throws FormatException if path contains a backslash")]
        public void GetDirectory_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("dir\\dir1"));            
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() throws FormatException if path starts with separator char")]
        public void GetDirectory_throws_FormatException_if_path_starts_with_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/name"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/name/someOtherName"));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() throws FormatException if path ends with directory separator char")]
        public void GetDirectory_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("name/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("name/someOtherName/"));
        }

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() returns direct child")]
        public void GetDirectory_returns_direct_child()
        {
            var expected = m_Dir1;
            var actual = m_Root.GetDirectory("dir1");

            Assert.Equal(expected, actual);
        }
    
        [Fact(DisplayName = nameof(InMemoryDirectory) + ".GetDirectory() returns children down in the hierarchy")]
        public void GetDirectory_returns_children_down_in_the_hierarchy()
        {
            var expected = m_Dir11;
            var actual = m_Root.GetDirectory("dir1/dir11");

            Assert.Equal(expected, actual);
        }

        #endregion

        #region DirectoryExists

        [Fact(DisplayName = nameof(InMemoryDirectory) + ".DirectoryExists() returns the expected result")]
        public void DirectoryExists_returns_the_expected_result()
        {
            Assert.True(m_Root.DirectoryExists("dir1"));
            Assert.True(m_Dir1.DirectoryExists("dir11"));
            Assert.True(m_Root.DirectoryExists("dir1/dir11"));

            Assert.False(m_Root.DirectoryExists("someName"));
            Assert.False(m_Root.DirectoryExists("someDir/someSubDir"));
        }

        #endregion
        

        class DerivedDirectory : InMemoryDirectory, IEnumerable<IFileSystemItem>
        {
            public DerivedDirectory(IDirectory parent, string name) 
                : base(parent, name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
            {
            }


            // Make Add() method public
            public new void Add(Func<IDirectory, IDirectory> createDirectory) => base.Add(createDirectory);

            // Make Add() method public
            new public void Add(Func<IDirectory, IFile> createFile) => base.Add(createFile);

            public IEnumerator<IFileSystemItem> GetEnumerator()
            {
                return Directories.Cast<IFileSystemItem>().Union(Files.Cast<IFileSystemItem>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }       
    }
}