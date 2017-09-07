﻿using System;
using System.Data;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model.Tables
{
    class ChangesView : IDisposable
    {
        const string s_PreviousSnapshotId = "PreviousSnapshotId";
        const string s_IncludedFileInstances = "IncludedFileInstances";        
        const string s_PreviousIncludedFileInstances = "previousFileInstances";        
        const string s_UnfilteredChanged = "UnfilteredChanges";
        const string s_Changes = "Changes";
        const string s_Current = "current";
        const string s_Previous = "previous";

        public enum Column
        {
            FileId,
            CurrentId,
            PreviousId,
            CurrentLastWriteTimeTicks,
            PreviousLastWriteTimeTicks,
            CurrentLength,
            PreviousLength
        }


        readonly Guid m_Id;
        readonly IDbConnection m_Connection;
        readonly DatabaseLimits m_Limits;
        readonly FileSystemSnapshotDo m_Snapshot;


        public string Name => GetViewName(s_Changes);


        private ChangesView([NotNull] IDbConnection connection, [NotNull] DatabaseLimits limits, [NotNull] FileSystemSnapshotDo snapshot)
        {
            m_Id = Guid.NewGuid();
            m_Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            m_Limits = limits ?? throw new ArgumentNullException(nameof(limits));
            m_Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        }


        public void Dispose()
        {
            m_Connection.ExecuteNonQuery($@"                    
                DROP VIEW IF EXISTS {GetViewName(s_PreviousSnapshotId)};
                DROP VIEW IF EXISTS {GetViewName(s_IncludedFileInstances)};
                DROP VIEW IF EXISTS {GetViewName(s_PreviousIncludedFileInstances)};
                DROP VIEW IF EXISTS {GetViewName(s_UnfilteredChanged)};
                DROP VIEW IF EXISTS {GetViewName(s_Changes)};                
            ");
        }


        void Create()
        {
            m_Connection.ExecuteNonQuery($@"                    

                -- query database for preceding snapshot                
                CREATE VIEW {GetViewName(s_PreviousSnapshotId)} AS 
                    SELECT {FileSystemSnapshotsTable.Column.Id} FROM {FileSystemSnapshotsTable.Name}
                    WHERE {FileSystemSnapshotsTable.Column.HistoryId} = {m_Snapshot.HistoryId} AND
                            {FileSystemSnapshotsTable.Column.SequenceNumber} < {m_Snapshot.SequenceNumber}
                    ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                    LIMIT 1 ;

                -- get the file instances included in the current snapshot
                CREATE VIEW {GetViewName(s_IncludedFileInstances)} AS
                    SELECT * 
                    FROM {FileInstancesTable.Name} 
                    WHERE {FileInstancesTable.Column.Id} IN 
                    (
                        SELECT {IncludesFileInstanceTable.Column.FileInstanceId} 
                        FROM {IncludesFileInstanceTable.Name}
                        WHERE {IncludesFileInstanceTable.Column.SnapshotId} = {m_Snapshot.Id}
                    );     

                -- get the file instances included in the previous snapshot
                CREATE VIEW {GetViewName(s_PreviousIncludedFileInstances)} AS
                    SELECT * 
                    FROM {FileInstancesTable.Name} 
                    WHERE {FileInstancesTable.Column.Id} IN 
                    (
                        SELECT {IncludesFileInstanceTable.Column.FileInstanceId} 
                        FROM {IncludesFileInstanceTable.Name}
                        WHERE {IncludesFileInstanceTable.Column.SnapshotId} IN (SELECT * FROM {GetViewName(s_PreviousSnapshotId)})
                    );

                -- create view containing both the previous and the current snapshot's file instances
                CREATE VIEW {GetViewName(s_UnfilteredChanged)} AS                       
                    -- (SQLite does not support a full outer join, so we need to combine two left joins)  
                    SELECT 
                        {s_Current}.{FileInstancesTable.Column.FileId}              AS {Column.FileId},
                        {s_Current}.{FileInstancesTable.Column.Id}                  AS {Column.CurrentId},
                        {s_Current}.{FileInstancesTable.Column.LastWriteTimeTicks}  AS {Column.CurrentLastWriteTimeTicks},
                        {s_Current}.{FileInstancesTable.Column.Length}              AS {Column.CurrentLength},
                        {s_Previous}.{FileInstancesTable.Column.Id}                 AS {Column.PreviousId},
                        {s_Previous}.{FileInstancesTable.Column.LastWriteTimeTicks} AS {Column.PreviousLastWriteTimeTicks},
                        {s_Previous}.{FileInstancesTable.Column.Length}             AS {Column.PreviousLength}
                    FROM {GetViewName(s_IncludedFileInstances)} AS {s_Current} 
                    LEFT OUTER JOIN {GetViewName(s_PreviousIncludedFileInstances)} AS {s_Previous}
                    ON {s_Current}.{FileInstancesTable.Column.FileId} = {s_Previous}.{FileInstancesTable.Column.FileId}                        
                    UNION
                        SELECT 
                            {s_Previous}.{FileInstancesTable.Column.FileId}             AS {Column.FileId},
                            {s_Current}.{FileInstancesTable.Column.Id}                  AS {Column.CurrentId},
                            {s_Current}.{FileInstancesTable.Column.LastWriteTimeTicks}  AS {Column.CurrentLastWriteTimeTicks},
                            {s_Current}.{FileInstancesTable.Column.Length}              AS {Column.CurrentLength},
                            {s_Previous}.{FileInstancesTable.Column.Id}                 AS {Column.PreviousId},
                            {s_Previous}.{FileInstancesTable.Column.LastWriteTimeTicks} AS {Column.PreviousLastWriteTimeTicks},
                            {s_Previous}.{FileInstancesTable.Column.Length}             AS {Column.PreviousLength}
                        FROM {GetViewName(s_PreviousIncludedFileInstances)} AS {s_Previous}
                        LEFT OUTER JOIN {GetViewName(s_IncludedFileInstances)} AS {s_Current}                                 
                        ON {s_Current}.{FileInstancesTable.Column.FileId} = {s_Previous}.{FileInstancesTable.Column.FileId};                                                                

                -- filter view to only include changes
                -- CurrentId NULL => file was deleted
                -- PreviousId NULL => file was added
                -- CurrentId != PreviousId => file was modified                
                CREATE VIEW {GetViewName(s_Changes)} AS
                    SELECT * FROM {GetViewName(s_UnfilteredChanged)} 
                    WHERE   
                    (
                            {Column.CurrentId} IS NULL OR 
                            {Column.PreviousId} IS NULL OR 
                            {Column.CurrentId} != {Column.PreviousId}
                    );
            ");
        }
        
        string GetViewName(string name)
        {
            return $"View_{name}_{m_Id.ToString().Replace("-", "")}";
        }
        
        public static ChangesView CreateTemporary(IDbConnection connection, DatabaseLimits limits, FileSystemSnapshotDo snapshot)
        {
            var view = new ChangesView(connection, limits, snapshot);
            view.Create();
            return view;
        }

    }
}
