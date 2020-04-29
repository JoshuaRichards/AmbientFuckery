using LiteDB;
using System;

#nullable enable
namespace AmbientFuckery.Contracts
{
    public interface IAmbientFuckeryDatabase : IDisposable
    {
        void Insert<T>(T record) where T : class;
        ILiteQueryable<T> Query<T>() where T : class;
        void Update<T>(T record) where T : class;
    }
}
