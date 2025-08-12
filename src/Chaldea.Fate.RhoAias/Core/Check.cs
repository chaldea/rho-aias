// The Chaldea licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Chaldea.Fate.RhoAias
{
    internal static class Check
    {
        public static T NotNull<T>(T? value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
