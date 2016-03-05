﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using Ninject.Modules;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration.Reader;

namespace SyncTool.Git.DI
{
    public class GitModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IGroupSettingsProvider>().To<JsonGroupSettingsProvider>();
            this.Bind<IRepositoryPathProvider>().To<CurrentDirectoryRepositoryPathProvider>();
            this.Bind<IGroupManager>().To<GitBasedGroupManager>();
        }
    }
}