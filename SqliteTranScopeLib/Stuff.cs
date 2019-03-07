using System;
using SQLite;

namespace SqliteTranScopeLib
{
    public class Stuff : IEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Word { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Word}";
        }
    }

    public interface IEntity
    {
        Guid Id { get; set; }
    }
}