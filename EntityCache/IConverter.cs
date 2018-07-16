using System.Collections.Generic;

namespace EntityCache
{
    public interface IConverter<T> where T : Entity
    {
        Dictionary<string, string> FromEntity(T entity);
        T ToEntity(Dictionary<string, string> data);
    }
}