using System.Data.Entity;
using Konbini.RfidFridge.TagManagement.Entities;

namespace Konbini.RfidFridge.TagManagement.Interface
{
    public interface IKDbContext
    {
        IDbSet<Settings> Settings { get; set; }
        int SaveChanges();
    }
}