using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventStreamDb
{

        public class ProcessHooks
        {
            public TypeMap Listeners { get; }

            public ProcessHooks(TypeMap listeners)
            {
                Listeners = listeners;
            }
        }

    public class TypeMap
    {
        private readonly Dictionary<Type, Type[]> types;

        public TypeMap()
        {
            types = new Dictionary<Type, Type[]>();

        }
        public TypeMap(Dictionary<Type, Type[]> values)
        {
            types = values;
        }

        public TypeMap Combine(TypeMap additionalValues)
        {
            var combinedTypes = types.Union(additionalValues.types)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, v => v.SelectMany(x => x.Value).ToArray());

            return new TypeMap(combinedTypes);
        }

        public IEnumerable<Type> ForType<T>()
        {
            var type = typeof(T);

            if (types.ContainsKey(type))
            {
                return types[type].ToArray();
            }

            return new Type[] {};

        }
    }

    public class Loader
    {
        // public KeyValuesStore<Type, IEventStreamStore> Stores { get; } = new KeyValuesStore<Type, IEventStreamStore>();
        // public Dictionary<Type, List<ITransform<,>>

        public TypeMap Listeners { get; set; } = new TypeMap();

        public ProcessHooks GetProcessHooks()
        {
            return new ProcessHooks(Listeners);
        }

        public Loader ScanAssemblies(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Listeners = Listeners.Combine(GetTypesWhichImplementGeneric(typeof(IListenFor<>), assembly));
            }

            return this;

            // GetTypesWhichImplement(typeof(IStore<>), assembly)
            //     .Select(x=>
            //     {
            //         typeof()
            //     });
        }

        private Type[] GetTypesWhichImplement(Type typeToImplement, Assembly assemblyToScan)
        {
            return assemblyToScan.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeToImplement)).ToArray();
        }

        private TypeMap GetTypesWhichImplementGeneric(Type typeToImplement, Assembly assemblyToScan)
        {
            var types = assemblyToScan.GetTypes()
                .Select(x => new
                {
                    Type = x,
                    Interfaces = x.GetInterfaces().Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeToImplement).ToArray()
                })
                .Where(x => x.Interfaces.Any())
                .SelectMany(x => x.Interfaces.Select(v => new { Type = x.Type, TargettingType = v.GenericTypeArguments.Single() }))
                .GroupBy(x => x.TargettingType, r => r.Type)
                .ToDictionary(x => x.Key, r => r.ToArray());

            return new TypeMap(types);
        }
    }

    public class KeyValuesStore<T, D>
    {
        private readonly Dictionary<T, List<D>> _storedData = new Dictionary<T, List<D>>();

        public void Push(T key, D value)
        {
            if (!_storedData.ContainsKey(key))
            {
                _storedData.Add(key, new List<D>());
            }

            var list = _storedData[key];
            list.Add(value);
        }

        public D[] GetValues(T key)
        {
            if (!_storedData.ContainsKey(key))
            {
                return new D[] { };
            }

            return _storedData[key].ToArray();
        }
    }
}