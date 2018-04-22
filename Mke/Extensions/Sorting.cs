namespace Mke.Extensions
{
    using System.Collections.Generic;
    
    public static class Sorting
    {
        /// <summary>Сортировка списка пар значений</summary>
        /// <param name="pairs">Список пар</param>
        /// <param name="N">Максимальное значение элемента пары</param>
        public static void SortPairs(this List<Pair> pairs, int N)
        {
            for (int i = 0; i < pairs.Count - 1; ++i)
            {
                for (int j = i + 1; j < pairs.Count; ++j)
                {
                    if (pairs[j].First * N + pairs[j].Second < pairs[i].First * N + pairs[i].Second)
                    {
                        var temp = pairs[i];
                        pairs[i] = pairs[j];
                        pairs[j] = temp;
                    }
                }
            }
        }
    }
}
