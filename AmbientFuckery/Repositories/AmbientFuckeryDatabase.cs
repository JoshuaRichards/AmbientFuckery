using AmbientFuckery.Contracts;
using LiteDB;

namespace AmbientFuckery.Repositories
{
    public class AmbientFuckeryDatabase : IAmbientFuckeryDatabase
    {
        private readonly ILiteDatabase db;

        public AmbientFuckeryDatabase()
        {
            db = new LiteDatabase("AmbientFuckery");
        }

        private ILiteCollection<T> GetCollection<T>() where T : class
        {
            return db.GetCollection<T>(typeof(T).Name);
        }

        public ILiteQueryable<T> Query<T>() where T : class
        {
            return GetCollection<T>().Query();
        }

        public void Insert<T>(T record) where T : class
        {
            GetCollection<T>().Insert(record);
        }

        public void Update<T>(T record) where T : class
        {
            GetCollection<T>().Update(record);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;

            db.Dispose();

            disposed = true;
        }
    }
}
