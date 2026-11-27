using Mitsu.Core.Domain;

namespace Mitsu.Core.Data;

/// <summary>
/// Read access to PrintData / CartonSize used by the print-control screen.
/// The SQL/Excel data layer will implement this; until then the app uses sample data.
/// </summary>
public interface IMasterDataSource
{
    /// <summary>Key code contains <paramref name="keyCodePart"/> (case-insensitive), at most <paramref name="max"/> rows.</summary>
    Task<IReadOnlyList<ProductSpec>> SearchProductsAsync(string keyCodePart, int max, CancellationToken ct = default);

    Task<ProductSpec?> GetProductAsync(string keyCode, CancellationToken ct = default);

    Task<CartonSpec?> GetCartonAsync(string caseNo, CancellationToken ct = default);
}

/// <summary>Simple in-memory implementation (sample data, tests).</summary>
public sealed class InMemoryMasterDataSource(IEnumerable<ProductSpec> products, IEnumerable<CartonSpec> cartons) : IMasterDataSource
{
    private readonly List<ProductSpec> _products = products.ToList();
    private readonly Dictionary<string, CartonSpec> _cartons =
        cartons.ToDictionary(c => c.CaseNo, StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<ProductSpec>> SearchProductsAsync(string keyCodePart, int max, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ProductSpec>>(_products
            .Where(p => p.KeyCode.Contains(keyCodePart ?? "", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.KeyCode, StringComparer.OrdinalIgnoreCase)
            .Take(max)
            .ToList());

    public Task<ProductSpec?> GetProductAsync(string keyCode, CancellationToken ct = default) =>
        Task.FromResult(_products.FirstOrDefault(p => string.Equals(p.KeyCode, keyCode, StringComparison.OrdinalIgnoreCase)));

    public Task<CartonSpec?> GetCartonAsync(string caseNo, CancellationToken ct = default) =>
        Task.FromResult(_cartons.GetValueOrDefault(caseNo));
}
