//-----------------------------------------------------------------------
//Copyright 2015-2016 Roman Tumaykin
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Impromptu.AssemblyResolver;
using Impromptu.Package;
using NuGet;

namespace Impromptu
{
    public sealed class InstantiatorFactory<T> where T : class
    {
        private readonly IPackageRetriever _packageRetriever;
        private readonly string _rootPath;

        static InstantiatorFactory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += PluginContext<T>.ResolveByFullAssemblyName;
        }


        /// <summary>
        /// Dictionary to store all of the cached instantiators. All of the possible variations of constructors will be in the Value part of this dictionary.
        /// Key is the packageId, and the second Dictionary is a concatenated Types for each constructor.
        /// </summary>
        private static Dictionary<InstantiatorKey, Dictionary<string, Instantiator<T>>> _instantiators;

        private static Dictionary<InstantiatorKey, Dictionary<string, Instantiator<T>>> Instantiators
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _instantiators,
                    () => new Dictionary<InstantiatorKey, Dictionary<string, Instantiator<T>>>());
                return _instantiators;
            }
        }

        private static ConcurrentDictionary<InstantiatorKey, object> _instantiatorLocks;
        private static ConcurrentDictionary<InstantiatorKey, object> InstantiatorLocks
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _instantiatorLocks, () => new ConcurrentDictionary<InstantiatorKey, object>());
                return _instantiatorLocks;
            }
        }

        public InstantiatorFactory(IPackageRetriever packageRetriever, string rootPath)
        {
            _packageRetriever = packageRetriever;
            _rootPath = rootPath;
        }

        public InstantiatorFactory(IPackageRetriever packageRetriever)
        {
            _packageRetriever = packageRetriever;
            _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ImpromptuPackages");
        }

        /// <summary>
        /// Creates and returns an instance of a class or interface T with no arguments.
        /// </summary>
        /// <typeparam name="T">type of the interface to instantiate</typeparam>
        /// <param name="instantiatorKey"></param>
        /// <returns></returns>
        /// <exception cref="InstantiatorException"></exception>
        public T Instantiate(InstantiatorKey instantiatorKey)
        {
            return Instantiate(instantiatorKey, null);
        }

        /// <summary>
        /// Creates and returns an instance of a class or interface T with a single constructor argument (<paramref name="data"/>).
        /// </summary>
        /// <typeparam name="T">type of the interface to instantiate</typeparam>
        /// <param name="instantiatorKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="InstantiatorException"></exception>
        public T Instantiate(InstantiatorKey instantiatorKey, object data)
        {
            return Instantiate(instantiatorKey, new[] { data });
        }

        /// <summary>
        /// Creates and returns an instance of a class or interface T with given constructor arguments (<paramref name="data"/>).
        /// </summary>
        /// <typeparam name="T">type of the interface to instantiate</typeparam>
        /// <param name="instantiatorKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="InstantiatorException"></exception>
        public T Instantiate(InstantiatorKey instantiatorKey, params object[] data)
        {
            try
            {
                var instance = CreateInstance(instantiatorKey, data);
                if (instance != null)
                {
                    return instance;
                }

                // OK. We did not find an instantiator. Let's try to create one. First of all let's lock an object
                var lockObject1 = new object();
                lock (lockObject1)
                {
                    if (InstantiatorLocks.TryAdd(instantiatorKey, lockObject1))
                    {
                        // if we ended up here, it means that we were first
                        Instantiators.AddRange(CreateInstantiatorsForPackage(instantiatorKey));
                    }
                    else
                    {
                        // some other process have already created (or creating) instantiator
                        // Theoretically, it is quite possible to have previous process fail, so we will need to be careful about assuming that if we got here,
                        // then we should have instantiators.
                        lock (InstantiatorLocks[instantiatorKey])
                        {
                            // try read from the instantiators first. Maybe it has already been successfully created
                            instance = CreateInstance(instantiatorKey, data);
                            if (instance != null)
                            {
                                return instance;
                            }
                            Instantiators.AddRange(CreateInstantiatorsForPackage(instantiatorKey));
                        }
                    }
                    instance = CreateInstance(instantiatorKey, data);
                    if (instance != null)
                    {
                        return instance;
                    }
                }
            }
            catch (Exception e)
            {
                throw new InstantiatorException("Error occurred during instantiation", e);
            }

            throw new InstantiatorException($"Unknown error. Instantiator failed to produce an instance of {instantiatorKey}", null);
        }

        /// <summary>
        /// Creates an instance of a generic type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instantiatorKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static T CreateInstance(InstantiatorKey instantiatorKey, object[] data)
        {
            if (!Instantiators.ContainsKey(instantiatorKey))
                return default(T);

            var instantiatorByType = Instantiators[instantiatorKey];

            // here it make sense to concatenate params
            var paramsHash = data == null || !data.Any() ? "" : string.Join(", ", data.Select(d => d.GetType().FullName));
            if (instantiatorByType.ContainsKey(paramsHash))
            {
                return instantiatorByType[paramsHash](data);
            }

            throw new InstantiatorException(
                $"Constructor signature {paramsHash} not found for package {instantiatorKey}", null);
        }

        /// <summary>
        /// Creates instantiators for all of the types in the package that can be instantiated
        /// </summary>
        /// <param name="instantiatorKey"></param>
        /// <returns></returns>
        /// <exception cref="InstantiatorCreationException"></exception>
        private Dictionary<InstantiatorKey, Dictionary<string, Instantiator<T>>> CreateInstantiatorsForPackage(InstantiatorKey instantiatorKey)
        {
            string packagePath;
            Directory.CreateDirectory(_rootPath);
            var returnDictionary = new Dictionary<InstantiatorKey, Dictionary<string, Instantiator<T>>>();

            try
            {
                packagePath = _packageRetriever.Retrieve(_rootPath, instantiatorKey.PackageId,
                    SemanticVersion.Parse(instantiatorKey.Version));
            }
            catch (Exception e)
            {
                throw new InstantiatorCreationException(
                    $"Package Retriever Failed to obtain the package {instantiatorKey.PackageId}.{instantiatorKey.Version}",
                    e);
            }

            if (string.IsNullOrWhiteSpace(packagePath))
                throw new InstantiatorCreationException(
                    $"Package Retriever Failed to obtain the package {instantiatorKey.PackageId}.{instantiatorKey.Version} from available sources",
                    null);

            // find the directory where the dlls are
            var libPath = Path.Combine(packagePath, "impromptu");

            var hotAssemblies = PluginContext<T>.DiscoverHotAssemblies(libPath);

            if (hotAssemblies == null)
                return returnDictionary;

            foreach (var hotType in hotAssemblies.SelectMany(impromptu => impromptu.ExportedTypes.Where(
                t =>
                    t.IsClass &&
                    typeof(T).IsAssignableFrom(t) &&
                    t.GetConstructors().Any())))
            {
                returnDictionary.Add(
                    new InstantiatorKey(instantiatorKey.PackageId, instantiatorKey.Version, hotType.FullName),
                    hotType.GetConstructors().ToDictionary(
                        ctor =>
                            !ctor.GetParameters().Any()
                                ? ""
                                : string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.FullName)),
                        CreateInstantiator));
            }

            return returnDictionary;
        }


        /// <summary>
        /// Creates a compiled instantiator based on the <paramref name="ctor"/>.
        /// </summary>
        /// <param name="ctor"></param>
        /// <returns></returns>
        public static Instantiator<T> CreateInstantiator(ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            var param =
                Expression.Parameter(typeof(object[]), "args");

            var argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;

                var paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                var paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            //            var lambda = Expression.Lambda<Instantiator<T>>(Expression.Convert(newExp, typeof(T)), param);
            var lambda = Expression.Lambda<Instantiator<T>>(newExp, param);

            //compile it
            var compiled = lambda.Compile();
            return compiled;
        }
    }
}