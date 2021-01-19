using Newtonsoft.Json;

namespace DPXLib.Model.Common
{
    /// <summary>
    /// a value that has a unit associated to it
    /// </summary>
    public class DimensionedValue
    {
        /// <summary>
        /// the value
        /// </summary>
        [JsonProperty("value")]
        public long Value { get; set; }

        /// <summary>
        /// the unit of the value
        /// </summary>
        [JsonProperty("unit")]
        public string Unit { get; set; }

        /// <summary>
        /// convert the value to a string, with unit appended
        /// </summary>
        /// <returns>the string, in schema Value Unit</returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Unit))
                return Value.ToString();
            else
                return $"{Value} {Unit}";
        }
    }
}
