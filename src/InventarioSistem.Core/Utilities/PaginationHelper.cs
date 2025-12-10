using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioSistem.Core.Utilities;

/// <summary>
/// Suporte a paginação de dados
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Classe auxiliar para paginação
/// </summary>
public class PaginationHelper
{
    /// <summary>
    /// Pagina uma coleção
    /// </summary>
    public static PagedResult<T> Paginate<T>(IEnumerable<T> source, int pageNumber, int pageSize = 50)
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1)
            pageSize = 50;

        var query = source.AsQueryable();
        var totalCount = query.Count();
        
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Pagina com filtro customizado
    /// </summary>
    public static PagedResult<T> Paginate<T>(
        IEnumerable<T> source,
        Func<T, bool> predicate,
        int pageNumber,
        int pageSize = 50)
    {
        var filtered = source.Where(predicate);
        return Paginate(filtered, pageNumber, pageSize);
    }
}
