using System.Data;

namespace SyncTool.Sql.Model
{
    static class FileInstancesTable
    {
        public const string Name = "FileInstances";

        public enum Column
        {
            Id,
            FileId,
            LastWriteUnixTimeTicks,
            Length
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}                     INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.FileId}                 INTEGER NOT NULL,
                    {Column.LastWriteUnixTimeTicks} BIGINT NOT NULL,
                    {Column.Length}                 BIGINT NOT NULL,
                    FOREIGN KEY ({Column.FileId}) REFERENCES {FilesTable.Name}({FilesTable.Column.Id}),
                    CONSTRAINT FileInstance_Unique UNIQUE (
                        {Column.FileId}, 
                        {Column.LastWriteUnixTimeTicks}, 
                        {Column.Length}
                    )); 
            ");
        }
    }
}
