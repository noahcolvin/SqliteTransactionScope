using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SqliteTranScopeLib;

namespace SqliteTranactionScope
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Worker().Run();
        }
    }
}
