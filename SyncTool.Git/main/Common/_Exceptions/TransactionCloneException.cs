﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Git.Common
{
    public class TransactionCloneException : GitTransactionException
    {
        public TransactionCloneException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public TransactionCloneException(string message) : base(message)
        {

        }
    }
}