﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

using SyncTool.Sql.Model.Tables;
using SyncTool.Utilities;

namespace SyncTool.Sql.Model
{
    public class SnapshotRepository
    {
        private class ChangeRecord
        {
            public int FileId { get; set; }

            public int? CurrentId { get; set; }

            public int? PreviousId { get; set; }

            public long? CurrentLastWriteTimeTicks { get; set; }

            public long? PreviousLastWriteTimeTicks { get; set; }

            public long? CurrentLength { get; set; }
            
            public long? PreviousLength { get; set; }

            public void Deconstruct(out FileInstanceDo previous, out FileInstanceDo current, out int fileId)
            {
                previous = PreviousId.HasValue
                    ? new FileInstanceDo() { Id = PreviousId.Value, LastWriteTimeTicks = PreviousLastWriteTimeTicks.Value, Length = PreviousLength.Value }
                    : null;

                current = CurrentId.HasValue
                    ? new FileInstanceDo() { Id = CurrentId.Value, LastWriteTimeTicks = CurrentLastWriteTimeTicks.Value, Length = CurrentLength.Value }
                    : null;

                fileId = FileId;
            }
        }
                                
        readonly Database m_Database;


        public SnapshotRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public FileSystemSnapshotDo GetSnapshotOrDefault(int historyId, int id)
        {
            return m_Database.QuerySingleOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} = @id AND
                        {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;
            ",
            new { historyId = historyId, id = id });
        }

        public FileSystemSnapshotDo GetLatestSnapshotOrDefault(int historyId)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = historyId });
        }

        public IEnumerable<FileSystemSnapshotDo> GetSnapshots(int historyId)
        {
            return m_Database.Query<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;",
                new { historyId = historyId });
        }

        public void AddSnapshot(FileSystemSnapshotDo snapshot)
        {
            //TODO: make sure no other snapshots were added for the history ?? Is this a Problem?            

            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var tmpId = Guid.NewGuid().ToString();
                var inserted = connection.QuerySingle<FileSystemSnapshotDo>($@"
                        INSERT INTO {FileSystemSnapshotsTable.Name} 
                        (                          
                            {FileSystemSnapshotsTable.Column.HistoryId} ,
                            {FileSystemSnapshotsTable.Column.SequenceNumber},
                            {FileSystemSnapshotsTable.Column.CreationTimeTicks} ,
                            {FileSystemSnapshotsTable.Column.RootDirectoryInstanceId},
                            {FileSystemSnapshotsTable.Column.TmpId}
                        )
                        VALUES 
                        (   
                            @historyId, 
                            (SELECT count(*) FROM {FileSystemSnapshotsTable.Name}), 
                            @ticks, 
                            @directoryId, 
                            @tmpId
                        );

                        SELECT *
                        FROM {FileSystemSnapshotsTable.Name}
                        WHERE {FileSystemSnapshotsTable.Column.TmpId} = @tmpId;

                        UPDATE {FileSystemSnapshotsTable.Name}
                        SET {FileSystemSnapshotsTable.Column.TmpId} = NULL 
                        WHERE {FileSystemSnapshotsTable.Column.TmpId} = @tmpId;
                ",
                new
                {
                    historyId = snapshot.HistoryId,
                    ticks = snapshot.CreationTimeTicks,
                    directoryId = snapshot.RootDirectoryInstanceId,
                    tmpId = tmpId
                });



                foreach(var segment in snapshot.IncludedFiles.ToArray().GetSegments(m_Database.Limits.MaxParameterCount - 1))
                {
                    var query = $@"
                        
                        INSERT INTO {IncludesFileInstanceTable.Name} 
                        (
                            {IncludesFileInstanceTable.Column.SnapshotId}, 
                            {IncludesFileInstanceTable.Column.FileInstanceId}
                        )
                        VALUES 
                        {
                            segment
                                .Select((_, index) => $"(@snapshotId, @file{index})")
                                .JoinToString(",")
                        };
                    ";

                    var parameters = segment
                        .Select((file, index) => ($"file{index}", (object)file.Id))
                        .Concat(("snapshotId", inserted.Id))
                        .ToArray();

                    connection.ExecuteNonQuery(query, parameters);
                }

                transaction.Commit();

                snapshot.Id = inserted.Id;
                snapshot.SequenceNumber = inserted.SequenceNumber;
            }
        }

        public void LoadIncludedFiles(FileSystemSnapshotDo snapshot)
        {
            var files = m_Database.Query<FileInstanceDo>($@"
                SELECT * 
                FROM {FileInstancesTable.Name}
                WHERE {FileInstancesTable.Column.Id} IN 
                (
                    SELECT {IncludesFileInstanceTable.Column.FileInstanceId}
                    FROM {IncludesFileInstanceTable.Name}
                    WHERE {IncludesFileInstanceTable.Column.SnapshotId} = @snapshotId
                );
            ",
            new { snapshotId = snapshot.Id });

            snapshot.IncludedFiles = files.ToList();            
        }

        public FileSystemSnapshotDo GetPrecedingSnapshot(FileSystemSnapshotDo snapshot)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId AND
                        {FileSystemSnapshotsTable.Column.SequenceNumber} < @sequenceNumber
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = snapshot.HistoryId, sequenceNumber = snapshot.SequenceNumber });
        }

        public IEnumerable<string> GetChangedFiles(FileSystemSnapshotDo snapshot)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var changesView = ChangesView.CreateTemporary(connection, m_Database.Limits, snapshot);
                               
                return connection.Query<string>($@"
                        SELECT {FilesTable.Column.Path} 
                        FROM {FilesTable.Name}
                        WHERE {FilesTable.Column.Id} IN 
                        (
                            SELECT {ChangesView.Column.FileId} 
                            FROM {changesView}                                                 
                        )")
                    .ToArray();
            }            
        }

        public IEnumerable<(FileInstanceDo previous, FileInstanceDo current)> GetChanges(FileSystemSnapshotDo snapshot, string[] pathFilter)
        {            
            using (var connection = m_Database.OpenConnection())
            {
                var changesView = ChangesView.CreateTemporary(connection, m_Database.Limits, snapshot);
                var filesView = FilteredFilesView.CreateTemporary(connection, m_Database.Limits, pathFilter);

                var fileDos = connection.Query<FileDo>($@"                    
                        SELECT * FROM {filesView}
                        WHERE {FilesTable.Column.Id} IN (SELECT DISTINCT {ChangesView.Column.FileId} FROM {changesView});")
                    .ToDictionary(record => record.Id);

                var changeQuery = $@"
                    SELECT * FROM {changesView}
                    WHERE {ChangesView.Column.FileId} IN (SELECT {FilteredFilesView.Column.Id} FROM {filesView})
                        
                ;";

                var changes = new LinkedList<(FileInstanceDo, FileInstanceDo)>();
                foreach (var (previous, current, fileId) in connection.Query<ChangeRecord>(changeQuery))
                {
                    var fileDo = fileDos[fileId];                    

                    if(previous != null)
                        previous.File = fileDo;

                    if(current != null)
                        current.File = fileDo;

                    changes.AddLast((previous, current));
                }
                
                return changes;
            }
        }

        /// <summary>
        /// Gets all the snapshots between the specified snapshots
        /// </summary>
        /// <param name="fromSnapshot">The start of the range (exclusive)</param>
        /// <param name="toSnapshot">The end of the range of snapshots to return (inclusive)</param>
        public IEnumerable<FileSystemSnapshotDo> GetSnapshotRange(FileSystemSnapshotDo fromSnapshot, FileSystemSnapshotDo toSnapshot)
        {
            if (fromSnapshot.HistoryId != toSnapshot.HistoryId)
                throw new ArgumentException("Cannot get range between snapshots of different histories");            

            return m_Database.Query<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} > @fromId AND
                      {FileSystemSnapshotsTable.Column.Id} <= @toId AND
                      {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} ASC;
                ",
                new { fromId = fromSnapshot.Id, toId = toSnapshot.Id, historyId = fromSnapshot.HistoryId });
        }

        /// <summary>
        /// Gets all the snapshots up to the specified snapshot
        /// </summary>        
        /// <param name="toSnapshot">The end of the range of snapshots to return (inclusive)</param>
        public IEnumerable<FileSystemSnapshotDo> GetSnapshotRange(FileSystemSnapshotDo toSnapshot)
        {            
            return m_Database.Query<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} <= @toId AND
                      {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} ASC;
                ",
                new { toId = toSnapshot.Id, historyId = toSnapshot.HistoryId });
        }
    }
}
