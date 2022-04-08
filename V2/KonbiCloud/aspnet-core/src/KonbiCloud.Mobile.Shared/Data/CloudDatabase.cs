using KonbiCloud.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Data
{
    public class CloudDatabase
    {
        readonly SQLiteAsyncConnection _database;
        public CloudDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Cloud>().Wait();
        }
        public Task<Cloud> GetCloudUrlAsync()
        {
            return _database.Table<Cloud>()
                .FirstOrDefaultAsync();
        }
        public Task<int> SaveCloudUrlAsync(Cloud cloud)
        {
            if (cloud.Id != 0)
            {
                return _database.UpdateAsync(cloud);
            }
            else
            {
                return _database.InsertAsync(cloud);
            }
        }
    }
}
