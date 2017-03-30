﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public sealed class ValueNode<T> : Node<T>
    {
        public T Value { get; }


        public ValueNode(T value, IEqualityComparer<T> valueComparer, int index) : base(valueComparer, index)
        {            
            Value = value;                        
        }        
    }
}