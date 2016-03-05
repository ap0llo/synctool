﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Git.Common
{
    public interface IGitTransaction
    {
        TransactionState State { get; } 

        string RemotePath { get; }

        string LocalPath { get; }


        /// <summary>
        /// Begins a new transaction by cloning the repository and creating local branches for all remote branches.
        /// The repository will be cloned as a bare repository
        /// </summary>
        /// <exception cref="GitTransactionException">The local directory exists and is not empty</exception>
        /// <exception cref="InvalidTransactionStateException">The transaction is in a state other than 'Created'</exception>
        void Begin();

        /// <summary>
        /// Completes the transaction by pushing all commits created in the local repository to the remote repository
        /// </summary>
        /// <exception cref="InvalidTransactionStateException">The transaction is in a state other than 'Active'</exception>
        /// <exception cref="TransactionFailedException">The transaction could not be completed</exception>
        void Commit();
    }
}