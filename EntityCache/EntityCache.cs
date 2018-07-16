using System;
using System.Collections.Generic;
using RepositoryProvider;

namespace EntityCache
{
    public class EntityCache<T> where T : Entity
    {
        private readonly Dictionary<int, T> _entitiesDictionary = new Dictionary<int, T>();
        private readonly IRepositoryProvider _repositoryProvider;
        private readonly ELoadingMode _loadingMode;
        private readonly object _locker = new object();
        private readonly IConverter<T> _converter;
        public event Action<string> Notification;

        public EntityCache(IRepositoryProvider repositoryProvider, IConverter<T> converter, ELoadingMode loadingMode = ELoadingMode.Eager)
        {
            _repositoryProvider = repositoryProvider ?? throw new ArgumentNullException(nameof(repositoryProvider)
                                      , "Repository cannot be null");
            _converter = converter ?? throw new ArgumentNullException(nameof(converter), "Converter cannot be null");
            _loadingMode = loadingMode;

            switch (_loadingMode)
            {
                case ELoadingMode.Eager:
                    retrieveEntitiesFromProvider();
                    break;
                case ELoadingMode.Lazy:
                    break;
            }
        }

        private void retrieveEntitiesFromProvider()
        {
            lock (_locker)
            {
                try
                {
                    var entities = _repositoryProvider.GetAll();

                    foreach (var e in entities)
                    {
                        var newEntity = _converter.ToEntity(e);
                        _entitiesDictionary.Add(newEntity.getId(), newEntity);
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Could not retrieve all entities from the provider");
                }
            }
        }

        public T Get(int id)
        {
            lock (_locker)
            {
                if (_entitiesDictionary.ContainsKey(id)) return _entitiesDictionary[id];

                Dictionary<string, string> item = null;

                if (_loadingMode == ELoadingMode.Lazy)
                    //Checks if entity exists in the repository.
                    item = _repositoryProvider.Get(id.ToString());

                if (item == null)
                    throw new InvalidOperationException("id does not exist");

                var entity = _converter.ToEntity(item);
                _entitiesDictionary.Add(entity.getId(), entity);

                return _entitiesDictionary[id];
            }
        }

        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

            var entityId = entity.getId().ToString();

            lock (_locker)
            {

                if (_entitiesDictionary.ContainsKey(entity.getId())
                    || (_loadingMode == ELoadingMode.Lazy && _repositoryProvider.Get(entityId) != null))
                    throw new InvalidOperationException("An entity with the same id already exists");

                try
                {
                    var data = _converter.FromEntity(entity);

                    _repositoryProvider.Add(data);
                    _entitiesDictionary.Add(entity.getId(), entity);

                }
                catch (Exception)
                {
                    throw new Exception("Failed to add a new entity");
                }
            }

            Notification?.Invoke("Entity was added");
        }

        public void Update(T newEntity)
        {
            if (newEntity == null)
                throw new ArgumentNullException(nameof(newEntity), "Entity cannot be null");

            var entityId = newEntity.getId().ToString();

            lock (_locker)
            {
                if (!_entitiesDictionary.ContainsKey(newEntity.getId())
                    && (_loadingMode == ELoadingMode.Lazy && _repositoryProvider.Get(entityId) == null))
                    throw new InvalidOperationException("Entity does not exist");

                try
                {
                    _repositoryProvider.Update(entityId, _converter.FromEntity(newEntity));
                    _entitiesDictionary[newEntity.getId()] = newEntity;
                }
                catch (Exception)
                {
                    throw new Exception("Failed to update");
                }
            }

            Notification?.Invoke("Entity was updated");
        }

        public void Remove(int id)
        {
            lock (_locker)
            {

                if (!_entitiesDictionary.ContainsKey(id)
                    && (_loadingMode == ELoadingMode.Lazy && _repositoryProvider.Get(id.ToString()) == null))
                    throw new InvalidOperationException("Entity does not exist");

                try
                {
                    _repositoryProvider.Remove(id.ToString());

                    if (_entitiesDictionary.ContainsKey(id))
                        _entitiesDictionary.Remove(id);
                }
                catch (Exception)
                {
                    throw new Exception("Failed to remove");
                }
            }

            Notification?.Invoke("Entity was removed");
        }
    }
}
