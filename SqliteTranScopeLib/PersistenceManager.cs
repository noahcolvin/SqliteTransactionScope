using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace SqliteTranScopeLib
{
    public class PersistenceManager
    {
        private static PersistenceManager _current;
        private SQLiteConnection _connection;

        public static PersistenceManager Current => _current ?? (_current = new PersistenceManager());
        public static SQLiteConnection Connection => SqliteTransactionScope.CurrentConnection ?? Current._connection ?? (Current._connection = InitializeConnection());

        private static SQLiteConnection InitializeConnection()
        {
            var databasePath = Path.Combine(Path.GetTempPath(), "sql.db3");
            const SQLiteOpenFlags sqLiteOpenFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;
            var connection = new SQLiteConnection(databasePath, sqLiteOpenFlags);
            BuildSchema(connection);
            return connection;
        }

        private static void BuildSchema(SQLiteConnection connection)
        {
            connection.CreateTable<Stuff>();
        }

        public static async Task<List<T>> GetAll<T>() where T : IEntity, new()
        {
            return await Task.Run(() => Connection.Table<T>().ToList());
        }

        public static List<T> GetAllSynchronous<T>() where T : IEntity, new()
        {
            return Connection.Table<T>().ToList();
        }

        public static async Task Save<T>(T entity) where T : IEntity, new()
        {
            await Task.Run(() => InsertOrUpdate(entity));
        }

        public static void SaveSynchronous<T>(T entity) where T : IEntity, new()
        {
            InsertOrUpdate(entity);
        }

        private static void InsertOrUpdate<T>(T entity) where T : IEntity, new()
        {
            if (entity.Id == Guid.Empty || Connection.Find<T>(entity.Id) == null)
            {
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
                Connection.Insert(entity);
            }
            else
            {
                Connection.Update(entity);
            }
        }

        public static async Task Delete<T>(T entity, bool recursive = false) where T : IEntity, new()
        {
            await Task.Run(() => Connection.Delete(entity, recursive));
        }

        public static void DeleteSynchronous<T>(T entity, bool recursive = false) where T : IEntity, new()
        {
            Connection.Delete(entity, recursive);
        }

        public static async Task DeleteAll<T>(List<T> entities) where T : IEntity, new()
        {
            await Task.Run(() => Connection.DeleteAll(entities));
        }
    }
}