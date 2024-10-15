using System;
using System.Reflection;
using System.ComponentModel;
using static VerifoneSPRemotePurchaseTerminalIntegration.Lib.Enums;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib
{
    public static class Utilities
    {
        /// <summary>
        /// Gets the description of the enum value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to get the description of.</param>
        /// <returns>The description of the enum value.</returns>
        public static string GetEnumDescription<T>(T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// Calculates the length of the command in hex.
        /// </summary>
        /// <param name="command">The command to calculate the length of.</param>
        /// <returns>The length of the command in hex.</returns>
        public static byte[] CalculateHexLength(string command)
        {
            var lengthBytes = BitConverter.GetBytes((ushort)command.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            return lengthBytes;
        }

        /// <summary>
        /// Converts a byte array to a hex string.
        /// </summary>
        /// <param name="hex">The byte array to convert.</param>
        /// <returns>The hex string.</returns>
        /// <exception cref="ArgumentException">Thrown when the hex string length is not even.</exception>
        public static byte[] ConvertHexStringToByteArray(string hex)
        {
            // Ensure the string length is even
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string length must be even.");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Converts a string to a hex string.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The hex string.</returns>
        public static string ConvertToHexString(string input)
        {
            // Create an array to store the hexadecimal values
            string[] hexArray = new string[input.Length];

            // Iterate over each character in the string
            for (int i = 0; i < input.Length; i++)
            {
                // Convert the character to its byte value and format it as a hexadecimal string
                hexArray[i] = ((int)input[i]).ToString("X2");
            }

            // Join the array into a single string separated by spaces
            return string.Join(" ", hexArray);
        }
    }
}
