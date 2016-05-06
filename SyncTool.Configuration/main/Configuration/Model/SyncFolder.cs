﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Configuration object for a folder that's being synced within a group
    /// </summary>
    public class SyncFolder : IEquatable<SyncFolder>
    {

        FilterConfiguration m_Filter;

        /// <summary>
        /// Gets the name of the folder
        /// </summary>
        public string Name { get; }
      
        /// <summary>
        /// Gets or sets the path of the folders root directory in the filesystem
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the filesystem filter for this folder
        /// </summary>        
        public FilterConfiguration Filter
        {
            get { return m_Filter ?? FilterConfiguration.Empty; }
            set { m_Filter = value; }
        }


        public SyncFolder(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value must not be empty or whitespace", nameof(name));
            }

            Name = name;
        }

        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Name);

        public override bool Equals(object obj) => Equals(obj as SyncFolder);

        public bool Equals(SyncFolder other)
        {
            if (other == null)
            {
                return false;                
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Name, other.Name) &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(this.Path, other.Path) &&
                   EqualityComparer<FilterConfiguration>.Default.Equals(Filter, other.Filter);                   
        }
    }
}