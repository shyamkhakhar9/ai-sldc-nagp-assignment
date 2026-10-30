using System.Collections;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using InvoiceApproval.Api.Endpoints;
using InvoiceApproval.Api.Persistence;
using InvoiceApproval.Api.Services;

namespace InvoiceApproval.ArchitectureTests;

/// <summary>
/// One structural rule: invoice mutation on the request path goes through
/// <see cref="IInvoiceStore"/>. The HTTP/approval types must not own a collection,
/// a SQL client, or a PayEx/HttpClient. The assembly must not grow a PayEx type
/// until the outbox decision says so.
///
/// Command:
/// dotnet test tests/InvoiceApproval.ArchitectureTests/InvoiceApproval.ArchitectureTests.csproj
/// </summary>
public sealed class PersistenceBoundaryTests
{
    [Fact]
    public void Request_path_writes_invoices_only_through_IInvoiceStore()
    {
        var api = typeof(InvoiceService).Assembly;

        var storeImplementations = api.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IInvoiceStore).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();
        Assert.Equal(new[] { nameof(InMemoryInvoiceStore) }, storeImplementations);

        var ctor = typeof(InvoiceService).GetConstructors().Single();
        var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType).ToList();
        Assert.Contains(typeof(IInvoiceStore), parameterTypes);
        Assert.DoesNotContain(typeof(InMemoryInvoiceStore), parameterTypes);
        Assert.DoesNotContain(typeof(HttpClient), parameterTypes);

        var payexTypes = api.GetTypes()
            .Where(t => t.Name.Contains("PayEx", StringComparison.OrdinalIgnoreCase)
                        || t.Name.Contains("PaymentProvider", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.FullName)
            .ToList();
        Assert.True(payexTypes.Count == 0,
            "PayEx is not in the as-built approval path. Found: " + string.Join(", ", payexTypes));

        var forbidden = new[]
        {
            typeof(ConcurrentDictionary<,>),
            typeof(Dictionary<,>),
            typeof(HttpClient),
            typeof(IDictionary),
            typeof(IList)
        };

        var offenders = new List<string>();
        foreach (var type in new[] { typeof(InvoiceService), typeof(InvoiceEndpoints) })
        {
            var members = type
                .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            foreach (var member in members)
            {
                var memberType = member switch
                {
                    FieldInfo field => field.FieldType,
                    PropertyInfo property => property.PropertyType,
                    _ => null
                };
                if (memberType is null)
                {
                    continue;
                }

                var closed = memberType.IsGenericType ? memberType.GetGenericTypeDefinition() : memberType;
                var isForbidden = forbidden.Any(f => f == closed || f.IsAssignableFrom(memberType))
                                  || memberType.Namespace is "Microsoft.Data.SqlClient" or "System.Data.SqlClient";
                if (isForbidden)
                {
                    offenders.Add($"{type.Name}.{member.Name} : {memberType.Name}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "InvoiceService and InvoiceEndpoints must go through IInvoiceStore. Offenders: "
            + string.Join(", ", offenders));
    }
}
