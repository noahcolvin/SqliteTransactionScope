using System;
using System.Threading;
using System.Threading.Tasks;

namespace SqliteTranScopeLib
{
    public class Worker
    {
        public async Task Run()
        {
            var timer = new Timer(RunOtherThread,null,1,100);

            using (var scope = new SqliteTransactionScope(true))
            {
                for (var i = 0; i < 10; i++)
                {
                    await PersistenceManager.Save(GetNewStuff());
                    await Task.Delay(250);
                }

                LogThreadOne("Inside Transaction");
                (await PersistenceManager.GetAll<Stuff>()).ForEach(LogThreadOne);
            }

            LogThreadOne("After Transaction");
            (await PersistenceManager.GetAll<Stuff>()).ForEach(LogThreadOne);
        }

        private void RunOtherThread(object state)
        {
            for (var i = 0; i < 10; i++)
            {
                PersistenceManager.SaveSynchronous(GetNewStuff());
                Task.Delay(100).Wait();
            }

            PersistenceManager.GetAllSynchronous<Stuff>().ForEach(LogThreadTwo);
        }

        private void LogThreadOne(object entity)
        {
            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(entity);

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        private void LogThreadTwo(IEntity entity)
        {
            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(entity);

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        private Stuff GetNewStuff()
        {
            var lorem =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam nibh felis, maximus ac egestas non, congue in purus. Nullam bibendum mollis erat eget lacinia. Vivamus finibus semper metus, id convallis dui tristique id. Nam mollis magna vel ex aliquet sagittis. Pellentesque iaculis lacus tellus. Suspendisse at nisl interdum, luctus dui vestibulum, condimentum mi. Suspendisse egestas elementum leo, at viverra ante viverra a";
            var words = lorem.Split(' ');

            var r = new Random(DateTime.Now.Millisecond);
            return new Stuff { Word = words[r.Next(0, words.Length - 1)] };
        }
    }
}