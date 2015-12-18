﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Get-Snapshot")]
    public class GetSnapshotOptions
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('f', "folder", Required = true)]
        public string Folder { get; set; }

    }


    public class GetSnapshotCommand : CommandBase, ICommand<GetSnapshotOptions>
    {
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;
        readonly IGroupManager<IHistoryGroup> m_HistoryGroupManager;

        public GetSnapshotCommand(IOutputWriter outputWriter, IGroupManager<IHistoryGroup> historyGroupManager, IGroupManager<IConfigurationGroup> configurationGroupManager) : base(outputWriter)
        {
            if (historyGroupManager == null)
            {
                throw new ArgumentNullException(nameof(historyGroupManager));
            }
            if (configurationGroupManager == null)
            {
                throw new ArgumentNullException(nameof(configurationGroupManager));
            }
            m_HistoryGroupManager = historyGroupManager;
            m_ConfigurationGroupManager = configurationGroupManager;
        }



        public int Run(GetSnapshotOptions opts)
        {
            using (var group = m_ConfigurationGroupManager.GetGroup(opts.Group))
            using (var historyRepository = m_HistoryGroupManager.GetGroup(opts.Group))
            {
                PrintSyncFolder(group.GetItem(opts.Folder), " ");

                var history = historyRepository.GetItem(opts.Folder);
                PrintHistory(history, " \t");

            }
            return 0;


        }


        void PrintSyncFolder(SyncFolder folder, string prefix = "")
        {
            OutputWriter.WriteLine($"{prefix}{folder.Name} --> {folder.Path}");
        }

        void PrintHistory(IFileSystemHistory history, string prefix)
        {
            if (history != null && history.Snapshots.Any())
            {
                foreach (var snapshot in history.Snapshots)
                {
                    OutputWriter.WriteLine($"{prefix}\t{snapshot.CreationTime}\t{snapshot.Id}");
                }
            }
            else
            {
                OutputWriter.WriteLine($"{prefix}No snapshots found");
            }

        }
        
    }
}