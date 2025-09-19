// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("A6ZF93kqvjNnu90pnbPsCECIvjN5+vT7y3n68fl5+vr7LYFEo6NU6Iicy+Scy0QuzydZ7JwXCtXW1/zWy3n62cv2/fLRfbN9DPb6+vr++/i4QlSazb5myGdRgOyeBowyFVAcfbPwJ4NysIl47Vlcn9zyJUZeXHrkZ4f4HaTx8ivHQBcMtGykqHRXaJ5sVlzL8UonVc1UNFLQVjzHA9ZUu/afTkZhK6T4j6nwdhlYqu9xEkAbusDp9FFCu+sEaH2k90/YqunjcnJSRzh02PHzYu70mKao5bldhAKktKIwIkQZg1GyF7je0UJQBrboUMyxdcRz1dAmLwIVh0DgiNB70SbeD4OD/34u7+Vq9+Q/ivYR1KUr+jSHi81A2bwsI1YnCPn4+vv6");
        private static int[] order = new int[] { 5,1,11,5,11,13,12,11,9,12,13,13,13,13,14 };
        private static int key = 251;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
