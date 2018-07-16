using System.Collections.Generic;

namespace RepositoryProvider
{
    public interface IRepositoryProvider
    {
        void Add(Dictionary<string, string> data);
        void Update(string id, Dictionary<string, string> data);
        void Remove(string id);
        Dictionary<string, string> Get(string id);
        IEnumerable<Dictionary<string, string>> GetAll();
    }
}