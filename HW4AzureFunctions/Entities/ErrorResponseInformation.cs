using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureStorage.Entities
{
    /// <summary>
    /// Internal Error descriptions
    /// </summary>
    public static class ErrorResponsesInformation
    {
        /// <summary>
        /// Error messages
        /// </summary>
        public static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>();

        static ErrorResponsesInformation()
        {
            ErrorMessages.Add(1, "The entity already exists");
            ErrorMessages.Add(2, "The parameter is required");
            ErrorMessages.Add(3, "The entity could not be found");
            ErrorMessages.Add(4, "The parameter cannot be null");
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }
    }
}
