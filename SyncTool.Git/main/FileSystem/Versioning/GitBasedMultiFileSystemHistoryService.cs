﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryService : GitBasedService, IMultiFileSystemHistoryService
    {
        internal static readonly BranchName BranchName = new BranchName("MultiFileSystemSnapshots");        
        readonly IHistoryService m_HistoryService;

        public GitBasedMultiFileSystemHistoryService(GitBasedGroup group, IHistoryService historyService) : base(group)
        {
            if (historyService == null)            
                throw new ArgumentNullException(nameof(historyService));
            
            m_HistoryService = historyService;
        }


        public IMultiFileSystemSnapshot LatestSnapshot
        {
            get
            {
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return null;
                }
                var tip = GitGroup.Repository.GetLocalBranch(BranchName).Tip;
                return Snapshots.FirstOrDefault(snapshot => snapshot.Id == tip.Sha);
            }
        }

        public IMultiFileSystemSnapshot this[string id]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(id))
                    throw new ArgumentNullException(nameof(id));

                return GetSnapshot(id);
            }
        }

        public IEnumerable<IMultiFileSystemSnapshot> Snapshots
        {
            get
            {                
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<IMultiFileSystemSnapshot>();
                }
                
                return GitGroup.Repository.GetLocalBranch(BranchName).Commits
                               .Where(GitBasedMultiFileSystemSnapshot.IsSnapshot)
                               .Select(commit => new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService));
            }
        }

        public IMultiFileSystemSnapshot CreateSnapshot()
        {
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                GitGroup.Repository.CreateBranch(BranchName, GitGroup.Repository.GetInitialCommit());
            }

            return GitBasedMultiFileSystemSnapshot.Create(GitGroup.Repository, BranchName, m_HistoryService);
        }

        public string[] GetChangedFiles(string to)
        {
            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var snapshot = GetSnapshot(to);
            foreach (var histoyName in snapshot.HistoryNames)
            {
                var history = m_HistoryService[histoyName];                
                var changedFiles = history.GetChangedFiles(snapshot.GetSnapshotId(histoyName));

                foreach (var filePath in changedFiles)
                {
                    result.Add(filePath);
                }
            }

            return result.ToArray();
        }

        public string[] GetChangedFiles(string from, string to)
        {
            if (String.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException(nameof(from));

            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var fromSnapshot = GetSnapshot(from);
            var toSnapshot = GetSnapshot(to);

            //TODO: check that fromId is an ancestor of toId

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var histoyName in toSnapshot.HistoryNames)
            {
                var history = m_HistoryService[histoyName];

                string[] changedFiles;
                // if the history already existed at the time of fromSnapshot, 
                // only get the changes between the snapshots                
                if (fromSnapshot.HistoryNames.Contains(histoyName))
                {
                    changedFiles = history.GetChangedFiles(
                        fromSnapshot.GetSnapshotId(histoyName), 
                        toSnapshot.GetSnapshotId(histoyName));
                }
                // if the current history did not exist at the time of fromSnapshot, 
                // get all changes for the current history
                else
                {
                    changedFiles = history.GetChangedFiles(toSnapshot.GetSnapshotId(histoyName));
                }

                foreach (var filePath in changedFiles)
                {
                    result.Add(filePath);
                }
            }

            return result.ToArray();
        }

        public IMultiFileSystemDiff GetChanges(string toId, string[] pathFilter = null)
        {
            if(String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            var snapshot = GetSnapshot(toId);

            // get all change lists from all histories
            var allChangeLists = snapshot.HistoryNames
                .Select(name => m_HistoryService[name].GetChanges(snapshot.GetSnapshotId(name)))
                .SelectMany(diff => diff.ChangeLists);
                            
            // group lists by path, flatten the the list and create new changelists
            var combinedChangeLists = allChangeLists                
                .GroupBy(changeList => changeList.Path, StringComparer.InvariantCultureIgnoreCase)                
                .Select(group => group.SelectMany(changeList => changeList.Changes).Distinct())
                .Select(group => new ChangeList(group));
                          
            return new MultiFileSystemDiff(snapshot, combinedChangeLists);
        }

        public IMultiFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            if (String.IsNullOrWhiteSpace(fromId))
                throw new ArgumentNullException(nameof(fromId));

            if (String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            //TODO: check that fromId is an ancestor of toId

            var fromSnapshot = GetSnapshot(fromId);
            var toSnapshot = GetSnapshot(toId);

            throw new NotImplementedException();
        }


        IMultiFileSystemSnapshot GetSnapshot(string id)
        {            
            var commit = GitGroup.Repository.Lookup<Commit>(id);
            if (commit == null || !GitBasedMultiFileSystemSnapshot.IsSnapshot(commit))
            {
                throw new SnapshotNotFoundException(id);
            }
            else
            {
                return new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService);
            }
        }
    }
}