﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Utilities;

namespace SyncTool.Git.Common
{
    public class CachingGitTransaction : AbstractGitTransaction
    {
        public CachingGitTransaction(string remotePath, string localPath) : base(remotePath, localPath)
        {
        }


        public override void Begin()
        {
            base.Begin();
            
            //TODO
//            // if we cannot reuse the local directory, delete the directory and execute the base case            
//            if (!CanReuseLocalRepository())
//            {
//                DirectoryHelper.DeleteRecursively(LocalPath);
//                return;
//            }
            
            
            
        }

        protected override void OnTransactionCompleted()
        {
            //nop
        }

        internal bool CanReuseLocalRepository()
        {
            // if the directory does not exist, there is nothing to reuse
            if (!Directory.Exists(LocalPath))
            {
                return false;
            }

            // if the directory is empty, there is nothing to reuse
            if (!Directory.EnumerateFileSystemEntries(LocalPath).Any())
            {
                return false;                
            }

            // check if the directory is a git repository
            if (!Repository.IsValid(LocalPath))
            {
                return false;
            }

            using (var repository = new Repository(LocalPath))
            {
                // check if the repository is a bare repository
                if (!repository.Info.IsBare)
                {
                    return false;
                }

                // make sure the repository has a remote named "origin" that points to RemotePath
                if (repository.Network.Remotes[s_Origin]?.Url != RemotePath)
                {
                    return false;
                }

                var localBranches = repository.GetLocalBranches().ToList();

                // make sure there are no branches that are not tracking a remote
                if (localBranches.Any(b => b.IsTracking == false))
                {
                    return false;
                }
                
                //fetch all branches
                repository.Network.Fetch(repository.Network.Remotes[s_Origin]);

                // make sure there are no unpushed changes  
                if (localBranches.Any(b => b.TrackingDetails.AheadBy > 0))
                {
                    return false;
                }

            }

            return true;

        }
    }
}