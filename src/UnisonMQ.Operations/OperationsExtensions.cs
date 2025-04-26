using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

public static class OperationsExtensions
{
    public static IServiceCollection AddOperations(this IServiceCollection services)
    {
        services
            .RegisterOperations()
            .AddSingleton<IOperationProcessor>(serviceProvider =>
        {
            var operationTypes = serviceProvider.GetRequiredService<OperationTypes>();
            var operations = operationTypes.Select(
                t => (Operation)serviceProvider.GetRequiredService(t))
                .ToArray();

            return new Processor(operations);
        });
        
        return services;
    }
    
    private static IServiceCollection RegisterOperations(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var operationType = typeof(Operation);

        var discoveredOperations = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(operationType));
        OperationTypes operationTypes = new OperationTypes();

        foreach (var operation in discoveredOperations)
        {
            services.AddSingleton(operation);
            operationTypes.Add(operation);
        }

        services.AddSingleton(operationTypes);
        
        return services;
    }
}