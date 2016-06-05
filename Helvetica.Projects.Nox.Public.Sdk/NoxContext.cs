using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Public.Sdk
{
    public class MissingServiceException : ApplicationException
    {
        public MissingServiceException(string message) 
            : base(message) { }
    }

    public class ServiceContainer
    {
        private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

        public T GetService<T>(bool required = false) 
            where T: class
        {
            object obj;
            if (!_services.TryGetValue(typeof (T), out obj))
            {
                if(required)
                    throw new MissingServiceException("Service not found.");
                return null;
            }
                
            return (T) obj;
        }

        public T GetOrAddService<T>(T obj = null) 
            where T : class
        {
            return (T)_services.GetOrAdd(typeof (T), obj);
        }
    }

    public class NoxContext : INoxContext
    {
        public ServiceContainer ServiceContainer { get; }

        public PossiblePhrase Phrase { get; set; }
        public ReadOnlyCollection<KeyValuePair<string, string>> VariableResults { get; set; }
        public ReadOnlyCollection<KeyValuePair<string, string>> DictationResults { get; set; }
        public Dictionary<string, ReadOnlyCollection<KeyValuePair<string, string>>> ServiceResults { get;set; }

        public NoxContext(ServiceContainer services)
        {
            ServiceContainer = services;
        }
    }
}