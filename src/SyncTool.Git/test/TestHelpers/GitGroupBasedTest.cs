﻿using System.Collections.Generic;
using System.IO;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
using Directory = System.IO.Directory;
using SyncTool.Common;
using Moq;
using Autofac;
using SyncTool.Git.DI;
using SyncTool.Common.DI;

namespace SyncTool.Git.TestHelpers
{
    /// <summary>
    /// Test base class for tests that require a <see cref="Group"/>
    /// </summary>
    public abstract class GitGroupBasedTest : DirectoryBasedTest
    {
        protected readonly string m_RemotePath;
        readonly GroupStorage m_GroupStorage;
        readonly IContainer m_Container;
        readonly ILifetimeScope m_ApplicationScope;
        

        protected GitGroupBasedTest()
        {
            m_RemotePath = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(m_RemotePath);
            RepositoryInitHelper.InitializeRepository(m_RemotePath);

            var localPath = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localPath);
            m_GroupStorage = new GroupStorage(localPath);


            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<CommonModule>();
            containerBuilder.RegisterModule<GitModule>();
            containerBuilder.RegisterInstance(EqualityComparer<IFileReference>.Default).As<IEqualityComparer<IFileReference>>();
            containerBuilder.RegisterInstance(m_GroupStorage).AsSelf();            

            m_Container = containerBuilder.Build();
            m_ApplicationScope = m_Container.BeginLifetimeScope(Scope.Application);
        }


        protected Group CreateGroup()
        {
            var groupScope = m_ApplicationScope.BeginLifetimeScope(Scope.Group, builder =>
            {
                builder.RegisterInstance(new GroupSettings("Irrelevant",m_RemotePath)).AsSelf();
            });

            var instance = groupScope.Resolve<Group>();
            instance.Disposed += (s, e) => groupScope.Dispose();

            return instance;
        }


        public override void Dispose()
        {
            m_ApplicationScope.Dispose();
            m_Container.Dispose();

            base.Dispose();
        }

    }
}