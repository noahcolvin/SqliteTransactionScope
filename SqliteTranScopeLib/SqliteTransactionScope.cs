using System;
using System.IO;
using System.Threading;
using SQLite;

namespace SqliteTranScopeLib
{
    public class SqliteTransactionScope : IDisposable
    {
        private bool _isCompleted;
        private bool _isDisposed;
        private readonly bool _asyncFlow;
        private static readonly ThreadLocal<SQLiteConnectionChain> _tlVersions = new ThreadLocal<SQLiteConnectionChain>();
        private static readonly AsyncLocal<SQLiteConnectionChain> _alVersions = new AsyncLocal<SQLiteConnectionChain>();
        public static SQLiteConnection CurrentConnection => _alVersions.Value?.Current ?? _tlVersions.Value?.Current;
        private SQLiteConnectionChain CurrentConnectionChain => _asyncFlow ? _alVersions.Value : _tlVersions.Value;

        public SqliteTransactionScope(bool asyncFlow = false)
        {
            _asyncFlow = asyncFlow;

            var databasePath = Path.Combine(Path.GetTempPath(), "sql.db3");
            const SQLiteOpenFlags sqLiteOpenFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;

            var cur = CurrentConnectionChain;
            
            if (asyncFlow)
            {
                _alVersions.Value = new SQLiteConnectionChain(new SQLiteConnection(databasePath, sqLiteOpenFlags), cur);
            }
            else
            {
                _tlVersions.Value = new SQLiteConnectionChain(new SQLiteConnection(databasePath, sqLiteOpenFlags), cur);
            }

            CurrentConnection.CreateTable<Stuff>();
            CurrentConnection.BeginTransaction();
        }

        public void Dispose()
        {
            if (!_isCompleted)
                CurrentConnection.Rollback();

            if (_isCompleted || _isDisposed)
                return;
            var cur = CurrentConnectionChain;
            if (cur != null)
            {
                DeleteConnection(cur.Current);

                if (_asyncFlow)
                {
                    _alVersions.Value = cur.Previous;
                }
                else
                {
                    _tlVersions.Value = cur.Previous;
                }
            }
            _isDisposed = true;
        }

        public void Commit()
        {
            if (_isCompleted || _isDisposed)
                return;
            var cur = CurrentConnectionChain;
            if (cur != null)
            {
                PushConnection(cur.Current);

                if (_asyncFlow)
                {
                    _alVersions.Value = cur.Previous;
                }
                else
                {
                    _tlVersions.Value = cur.Previous;
                }
            }
            _isCompleted = true;

            CurrentConnection.Commit();
        }

        private void DeleteConnection(SQLiteConnection connection)
        {
            Console.WriteLine($"SQLiteConnection {connection} deleted");
        }

        private void PushConnection(SQLiteConnection connection)
        {
            Console.WriteLine($"SQLiteConnection {connection} pushed");
        }

        private class SQLiteConnectionChain
        {
            public SQLiteConnectionChain(SQLiteConnection current, SQLiteConnectionChain previous)
            {
                Current = current;
                Previous = previous;
            }

            public SQLiteConnection Current { get; }
            public SQLiteConnectionChain Previous { get; }
        }
    }
}