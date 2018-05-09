namespace Mke.Helpers
{
    using System.Collections.Generic;

    /// <summary>Конвертер в двоичные числа</summary>
    public static class ToBinaryConverter
    {
        /// <summary>Перевести целое десятичное число в двоичный массив</summary>
        /// <param name="number">Целое десятичное число</param>
        /// <param name="buffer">Размер массива. При нуле или меньшем значении размер устанавливается динамически</param>
        /// <returns>Массив двоичных разрядов</returns>
        public static byte[] ConvertToBinary(int number, int buffer = 0)
        {
            if (buffer > 0)
            {
                var temp = new byte[buffer];

                for (var i = 0; i < buffer; i++)
                {
                    temp[i] = (byte)(number % 2);
                    number /= 2;
                }

                return temp;
            }
            else
            {
                var temp = new List<byte>();

                while (number > 0)
                {
                    temp.Add((byte)(number % 2));
                    number /= 2;
                }

                return temp.ToArray();
            }
        }
    }
}