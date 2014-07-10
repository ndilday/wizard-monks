using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WizardMonks.Core
{
    public class Multiton<K, T> where T: new()
    {
        private static readonly Dictionary<K, T> instances = new Dictionary<K, T>();

        private Multiton() 
        {
        }

        public static T GetInstance(K key)
        {
            T instance;
            lock (instances)
            {
                if (!instances.TryGetValue(key, out instance))
                {
                    instance = new T();
                    instances.Add(key, instance);
                }
            }
            return instance;
        }
    }

    public interface IKeyed<K>
    {
        K GetKey();
    }

    public class ImmutableMultiton<K, T> where T: IKeyed<K>, new()
    {
        private static readonly Dictionary<K, T> instances = new Dictionary<K, T>();

        private ImmutableMultiton() 
        {
        }

        public static void Initialize(string filePath)
        {
            instances.Clear();
            StreamReader reader = new StreamReader(filePath);
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            List<T> list = (List<T>)serializer.Deserialize(reader);
            foreach (T t in list)
            {
                instances.Add(t.GetKey(), t);
            }
        }

        public static void Initialize(IEnumerable<T> enumerable)
        {
            foreach (T t in enumerable)
            {
                instances.Add(t.GetKey(), t);
            }
        }

        public static T GetInstance(K key)
        {
            T instance;
            lock (instances)
            {
                if (!instances.TryGetValue(key, out instance))
                {
                    instance = new T();
                    instances.Add(key, instance);
                }
            }
            return instance;
        }

        public static bool Contains(K key)
        {
            return instances.ContainsKey(key);
        }

        public static Dictionary<K, T>.KeyCollection GetKeys()
        {
            return instances.Keys;
        }
    }
}
