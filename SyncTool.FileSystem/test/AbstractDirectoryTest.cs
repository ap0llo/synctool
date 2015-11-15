﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace SyncTool.FileSystem
{
    public class AbstractDirectoryTest
    {
        readonly DerivedDirectory m_Dir1;
        readonly DerivedDirectory m_Dir11;
        readonly File m_File1 = new File("file1");
        readonly DerivedDirectory m_Root;


        public AbstractDirectoryTest()
        {
            m_Dir11 = new DerivedDirectory("dir11");
            m_Dir1 = new DerivedDirectory("dir1", new[] {m_Dir11}, new[] {m_File1});
            m_Root = new DerivedDirectory("root", new[] {m_Dir1});
        }


        #region GetFile()

        [Fact(DisplayName = "AbstractDirectory.GetFile() throws ArgumentNullException if path is null")]
        public void GetFile_throws_ArgumentNullException_if_path_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Root.GetFile(null));
        }

        [Fact(DisplayName = "AbstractDirectory.GetFile() throws FormatException if path is empty or whitespace")]
        public void GetFile_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile(""));
            Assert.Throws<FormatException>(() => m_Root.GetFile("  "));
        }

        [Fact(DisplayName = "AbstractDirectory.GetFile() throws FormatException if path contains a backslash")]
        public void GetFile_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("name\\name"));            
        }

        [Fact(DisplayName = "AbstractDirectory.GetFile() throws FormatException if path starts with separator char")]
        public void GetFile_throws_FormatException_if_path_starts_with_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("/name"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("/name/someOtherName"));
        }

        [Fact(DisplayName = "AbstractDirectory.GetFile() throws FormatException if path ends with directory separator char")]
        public void GetFile_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetFile("/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("name/"));
            Assert.Throws<FormatException>(() => m_Root.GetFile("name/someOtherName/"));
        }

        [Fact(DisplayName = "AbstractDirectory.GetFile() returns direct child")]
        public void GetFile_returns_direct_child()
        {
            var expected = m_File1;
            var actual = m_Dir1.GetFile("file1");

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName= "AbstractDirectory.GetFile() returns children down in the hierarchy")]
        public void GetFile_returns_children_down_in_the_hierarchy()
        {
            var expected = m_File1;
            var actual = m_Root.GetFile("dir1/file1");

            Assert.Equal(expected, actual);
        }


        #endregion

        #region FileExists

        [Fact(DisplayName = "AbstractDirectory.FileExists() returns expected result")]
        public void FileExists_returns_expected_result()
        {
            Assert.False(m_Root.FileExists("someFileName"));
            Assert.False(m_Root.FileExists("file1"));

            Assert.True(m_Root.FileExists("dir1/file1"));
            Assert.True(m_Dir1.FileExists("file1"));            
        }

        #endregion

        #region GetDirectory()

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() throws ArgumentNullException if path is null")]
        public void GetDirectory_throws_ArgumentNullException_if_path_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Root.GetDirectory(null));
        }

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() throws FormatException if path is empty or whitespace")]
        public void GetDirectory_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory(""));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("  "));
        }

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() throws FormatException if path contains a backslash")]
        public void GetDirectory_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("dir\\dir1"));            
        }

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() throws FormatException if path starts with separator char")]
        public void GetDirectory_throws_FormatException_if_path_starts_with_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/name"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/name/someOtherName"));
        }

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() throws FormatException if path ends with directory separator char")]
        public void GetDirectory_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("name/"));
            Assert.Throws<FormatException>(() => m_Root.GetDirectory("name/someOtherName/"));
        }

        [Fact(DisplayName = "AbstractDirectory.GetDirectory() returns direct child")]
        public void GetDirectory_returns_direct_child()
        {
            var expected = m_Dir1;
            var actual = m_Root.GetDirectory("dir1");

            Assert.Equal(expected, actual);
        }
    
        [Fact(DisplayName = "AbstractDirectory.GetDirectory() returns children down in the hierarchy")]
        public void GetDirectory_returns_children_down_in_the_hierarchy()
        {
            var expected = m_Dir11;
            var actual = m_Root.GetDirectory("dir1/dir11");

            Assert.Equal(expected, actual);
        }

        #endregion

        #region DirectoryExists

        [Fact(DisplayName = "AbstractDirectory.DirectoryExists() returns the expected result")]
        public void DirectoryExists_returns_the_expected_result()
        {
            Assert.True(m_Root.DirectoryExists("dir1"));
            Assert.True(m_Dir1.DirectoryExists("dir11"));
            Assert.True(m_Root.DirectoryExists("dir1/dir11"));

            Assert.False(m_Root.DirectoryExists("someName"));
        }

        #endregion
        

        class DerivedDirectory : AbstractDirectory
        {
            public DerivedDirectory(string name) : base(name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
            {
            }

            public DerivedDirectory(string name, IEnumerable<IDirectory> directories) : base(name, directories, Enumerable.Empty<IFile>())
            {
            }

            public DerivedDirectory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : base(name, directories, files)
            {
            }
        }       
    }
}