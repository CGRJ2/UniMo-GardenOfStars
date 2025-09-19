// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("6ljb+OrX3NPwXJJcLdfb29vf2tmDEQNlOKJwkzaZ//BjcSeXyXHtkJLRBqJTkahZzHh9vv3TBGd/fVvFIodk1lgLnxJGmvwIvJLNKWGpnxKb4cjVcGOayiVJXIXWbvmLyMJTU6m96sW96mUP7gZ4zb02K/T39t33VOVS9PEHDiM0pmHBqfFa8Af/LqKi3l8PzsRL1sUeq9cw9YQK2xWmqte+b2dACoXZrojRVzh5i85QM2E6RqbZPIXQ0wrmYTYtlU2FiVV2Sb+ZY3W77J9H6UZwoc2/J60TNHE9XFjb1drqWNvQ2Fjb29oMoGWCgnXJTXd96tBrBnTsdRVz8Xcd5iL3dZpzZhlV+dDSQ8/VuYeJxJh8pSOFlexh+J0NAncGKdjZ29rb");
        private static int[] order = new int[] { 0,10,13,5,6,11,7,8,9,12,13,13,12,13,14 };
        private static int key = 218;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
