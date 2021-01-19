using System.Collections.Generic;

namespace DPXLib.Util
{
    /// <summary>
    /// class for calculating averages
    /// </summary>
    public class AverageNumber
    {
        /// <summary>
        /// internal list for holding the separate values to average
        /// </summary>
        private readonly List<double> numbers = new List<double>();

        /// <summary>
        /// the current average of all numbers added
        /// </summary>
        public double Average
        {
            get
            {
                // check we have at least one number
                if (numbers == null || numbers.Count <= 0)
                    return 0;

                // calculate average of all numbers
                double avg = 0;
                foreach (double n in numbers)
                    avg += n;

                return avg / numbers.Count;
            }
        }

        /// <summary>
        /// how many numbers we currently averaging
        /// </summary>
        public long TotalNumbers
        {
            get
            {
                return numbers.Count;
            }
        }

        /// <summary>
        /// add a number to the average
        /// </summary>
        /// <param name="number">the number to add</param>
        public void Add(double number)
        {
            numbers.Add(number);
        }
    }
}
