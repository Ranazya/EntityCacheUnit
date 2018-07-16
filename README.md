# EntityCacheUnit
A C# library which provides its users with an entity cache module. <br />
The main purpose of the cache is to optimize access to entities e.g. read an updated entity from the cache in the memory, and not directly from the repository.
* The cache stores any type of entity.
* The cache supports the following operations performed by the user:
  1. Get a cached entity.
  2. Add a new entity - including repository.
  3. Update a cached entity - including repository.
  4. Remove a cached entity - including repository.
* The cache can be initialized by the user as follows:
  1. The user should specify a repository provider (see below), to which the cache should “attach” itself.
  2. The user should also specify the requested loading mode - eager / lazy.
* The cache uses a repository provider which is responsible for accessing a specific type of repository (access layer), as follows:
	1. Each type of repository (e.g. document format) should have its own provider.
	2. The provider should support the following operations performed by the cache: add/ update/ remove entity.
	3. The provider might fail in executing any of these operations; and if so, then the cache remains in a consistent state.
	4. The cache is not be aware of the document format used by the provider to save the entities’ data into the repository (since any format may be used), and vice versa; the provider should not be aware of the data type (i.e. entity) used by the cache to save the entities’ data as objects in memory. This means that each one of these 2 components cannot use the data format which is used by the other component.
* The cache notifies any external subscribers (e.g. users) whenever entities are added to/ updated in/ removed from it.
* The cache operates consistently in a multithreaded environment.
