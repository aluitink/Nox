using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Helvetica.Projects.Nox.Core
{
    public class PluginLoader<T>
    {
        [ImportMany]
        public IEnumerable<T> Plugins { get; set; }

        public PluginLoader(string path)
        {
            if (!Directory.Exists(path))
                return;

            DirectoryCatalog directoryCatalog = new DirectoryCatalog(path);

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog(directoryCatalog);

            // Create the CompositionContainer with all parts in the catalog (links Exports and Imports)
            var container = new CompositionContainer(catalog);

            //Fill the imports of this object
            container.ComposeParts(this);
        }
    }
}