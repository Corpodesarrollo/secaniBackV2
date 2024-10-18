using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;

namespace Core.Modelos.Common
{
    public static class GenericMapper
{
    public static TDestination Map<TSource, TDestination>(TSource source)
    {
        return source.Adapt<TDestination>();
    }

    public static IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)
    {
        return source.Adapt<IEnumerable<TDestination>>();
    }

    public static List<TDestination> MapList<TSource, TDestination>(List<TSource> source)
    {
        return source.Adapt<List<TDestination>>();
    }
}
}