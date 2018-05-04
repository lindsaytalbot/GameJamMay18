#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("R5H7WB5PPUxEgKGTdbk6zM82GBK8kpPSR+PBjbGj8+IePQnTQvjWXFjqaUpYZW5hQu4g7p9laWlpbWhrexsdYIhCC7sbUjabx2ArGbcV9wayQJmTlOAGARbJRMczq7BGLIyuPEzXutDOIa8wzD4jU6lof6JCXXydvGXz0+hKsW9CkrXqRz6aSN1VRrK9I7IlKtwcHfcd6JyJsUunpVd2G+ppZ2hY6mliauppaWigkDtValuV6L2A4QLxbbOJkzjyS+VxBL4r0l13/Ywsj4dVt2oBD9CElYx0uES4gdxLsvMKFsvY+CTlsOqbE7Gp2Lp99J1dojqgOhtXuy2L8Z8HLuQvp6WdhJeOl0jKWt+V9+ZZzq36EsZpQbez5I1zvJvTyWpraWhp");
        private static int[] order = new int[] { 6,5,6,6,9,9,7,12,9,10,10,11,13,13,14 };
        private static int key = 104;

        public static byte[] Data() {
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
