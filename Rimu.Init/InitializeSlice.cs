using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Kernel;
namespace Rimu.Init;

public class InitializeSlice {
    private readonly IServiceCollection _serviceCollection;

    public InitializeSlice(IServiceCollection serviceCollection) {
        this._serviceCollection = serviceCollection;
    }
    
    public void Binds(Assembly[] assemblies) {
        foreach (var assembly in assemblies) {
            Bind(assembly);
        }
    }

    public void Bind(Assembly assembly) {
        FindAndAddWebRequestHandler(assembly);
    }

    private void FindAndAddWebRequestHandler(Assembly assembly) {
        var types = GetEnumerableOfType(assembly, typeof(WebRequestHandler<,>));
        foreach (var type in types) {
            this._serviceCollection.AddScoped(type);
        }
    }
    
    private static IEnumerable<Type> GetEnumerableOfType(Assembly assembly, Type searchType)
    {
        List<Type> objects = new ();
        foreach (Type type in 
                 assembly.GetTypes()
                         .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(searchType))) {
            objects.Add(type);
        }
        
        return objects;
    }
}