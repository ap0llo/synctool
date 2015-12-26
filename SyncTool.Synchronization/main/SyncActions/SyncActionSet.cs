﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    internal class SyncActionSet : ISyncActionSet, ISyncActionVisitor<MutableDirectory>
    {
        readonly IEqualityComparer<IFile> m_FileComparer; 
        readonly LinkedList<ResolvedSyncAction> m_Actions = new LinkedList<ResolvedSyncAction>();
        readonly LinkedList<ConflictSyncAction> m_Conflicts = new LinkedList<ConflictSyncAction>();


        public IEnumerable<ResolvedSyncAction> Actions => m_Actions;

        public IEnumerable<ConflictSyncAction> Conflicts => m_Conflicts; 


        public SyncActionSet(IEqualityComparer<IFile> fileComparer)
        {
            if (fileComparer == null)
            {
                throw new ArgumentNullException(nameof(fileComparer));
            }
            m_FileComparer = fileComparer;
        }


        public void Add(ResolvedSyncAction action)
        {
            m_Actions.AddLast(action);
        }

        public void Add(ConflictSyncAction conflict)
        {
            m_Conflicts.AddLast(conflict);
        } 

        public IEnumerator<SyncAction> GetEnumerator() => m_Actions.Union(m_Conflicts.Cast<SyncAction>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IDirectory ApplyTo(IDirectory directory)
        {
            if (m_Conflicts.Any())
            {
                throw new InvalidOperationException("Cannot apply SyncActionSet to directory because it contains conflicts");
            }
            
            var newDirectory = directory.ToMutableDirectory();
            foreach (var syncAction in m_Actions)
            {                
                syncAction.Accept(this, newDirectory);
            }
                        
            return newDirectory;
        }
        


        public void Visit(MultipleVersionConflictSyncAction action, MutableDirectory rootDirectory)
        {
            throw new InvalidOperationException();
        }

        public void Visit(ModificationDeletionConflictSyncAction action, MutableDirectory rootDirectory)
        {
            throw new InvalidOperationException();
        }

        public void Visit(ReplaceFileSyncAction action, MutableDirectory rootDirectory)
        {
            // replace can be mapped to remove + add

            var removeAction = new RemoveFileSyncAction(action.Target, action.OldVersion);
            var addAction = new AddFileSyncAction(action.Target, action.NewVersion);
            
            removeAction.Accept(this, rootDirectory);
            addAction.Accept(this, rootDirectory);

        }

        public void Visit(AddFileSyncAction action, MutableDirectory rootDirectory)
        {            
            if (rootDirectory.FileExists(action.FilePath))
            {
                throw new NotApplicableException($"Cannot apply {nameof(AddFileSyncAction)}. A file already exists at '{action.FilePath}'");
            }

            // if parent path is empty, the file will be added to the root directory
            if (String.IsNullOrEmpty(action.NewFile.Parent.Path))
            {
                rootDirectory.Add(d => action.NewFile.WithParent(d));
            }
            else
            {
                rootDirectory.EnsureDirectoryExists(action.NewFile.Parent.Path);                
                rootDirectory.GetDirectory(action.NewFile.Parent.Path).Add(d => action.NewFile.WithParent(d));                            
            }           
        }

        public void Visit(RemoveFileSyncAction action, MutableDirectory rootDirectory)
        {
            if (!rootDirectory.FileExists(action.FilePath))
            {
                throw new NotApplicableException($"Cannot apply {nameof(RemoveFileSyncAction)}. File '{action.FilePath}' was not found");
            }

            var existingFile = rootDirectory.GetFile(action.FilePath);
            if (!m_FileComparer.Equals(existingFile, action.RemovedFile))
            {
                throw new NotApplicableException($"Cannot apply {nameof(RemoveFileSyncAction)}. Exisitng file is differnt from the file to be removed");
            }

            var directory = String.IsNullOrEmpty(existingFile.Parent.Path) 
                ? rootDirectory 
                : rootDirectory.GetDirectory(existingFile.Parent.Path);
            
            directory.RemoveFileByName(action.RemovedFile.Name);            
        }
        
        
    }
}