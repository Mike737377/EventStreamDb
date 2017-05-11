using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventStreamDb
{
    public class Loader
    {
        // public KeyValuesStore<Type, IEventStreamStore> Stores { get; } = new KeyValuesStore<Type, IEventStreamStore>();
        // public Dictionary<Type, List<ITransform<,>>

        public Dictionary<Type, List<IListen>> Listeners { get; } = new Dictionary<Type, List<IListen>>();

        public void ScanAssembly(Assembly assembly)
        {
            // GetTypesWhichImplement(typeof(IListenFor<>), assembly).Select(x=> x. )
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