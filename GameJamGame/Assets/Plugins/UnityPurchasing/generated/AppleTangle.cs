#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("EzURAwZQAQYWCER1dWlgJVdqanEznEkofbLoiZ7Z9nKe93PXcjVKxBqU3htCVe4A6Ft8gSjuM6dSSVDpbGNsZmRxbGprJURwcW1qd2xxfDRycitkdXVpYCtmamgqZHV1aWBmZFyiAAx5EkVTFBtx1rKOJj5CptBqV2BpbGRrZmAlamslcW1sdiVmYHeFES7VbEKRcwz78W6IK0Wj8kJIekzdc5o2EWCkcpHMKAcGBAUEpocEzBx38FgL0HpanvcgBr9QikhYCPQBAxYHUFY0FjUUAwZQAQ8WD0R1dbQ1XelfATeJbbaKGNtgdvpiW2C5a2ElZmprYWxxbGprdiVqYyVwdmCQm38JoUKOXtETMjbOwQpIyxFs1HVpYCVXampxJUZENRsSCDUzNTE3Iefu1LJ12gpA5CLP9Gh96OKwEhI1hwG+NYcGpqUGBwQHBwQHNQgDDCklZmB3cWxjbGZkcWAldWppbGZ8CAMML4NNg/IIBAQAAAUGhwQEBVl/NYcEczULAwZQGAoEBPoBAQYHBLIeuJZHIRcvwgoYs0iZW2bNToUSdWlgJUZgd3FsY2xmZHFsamslRHCHBAUDDC+DTYPyZmEABDWE9zUvAwM1CgMGUBgWBAT6AQA1BgQE+jUYJUZENYcEJzUIAwwvg02D8ggEBAR3ZGZxbGZgJXZxZHFgaGBrcXYrNSo1hMYDDS4DBAAAAgcHNYSzH4S23DN6xIJQ3KKcvDdH/t3QdJt7pFc2M181ZzQONQwDBlABAxYHUFY0Fg0uAwQAAAIHBBMbbXFxdXY/KipyZ2lgJXZxZGthZHdhJXFgd2h2JWRxbWp3bHF8NBM1EQMGUAEGFghEdS+DTYPyCAQEAAAFNWc0DjUMAwZQJWpjJXFtYCVxbWBrJWR1dWlsZmSt2XsnMM8g0NwK027RpyEmFPKkqQqYOPYuTC0fzfvLsLwL3FsZ0844NRQDBlABDxYPRHV1aWAlTGtmKzQDBlAYCwETAREu1WxCkXMM+/FuiGlgJUxrZis0IzUhAwZQAQ4WGER1inaEZcMeXgwql7f9QU31ZT2bEPCOHIzb/E5p8AKuJzUH7R07/VUM1mKKDbEl8s6pKSVqdbM6BDWJskbKDVs1hwQUAwZQGCUBhwQNNYcEATVxbGNsZmRxYCVnfCVka3wldWR3cbvxdp7r12EKznxKMd2nO/x9+m7NfCVkdnZwaGB2JWRmZmB1cWRrZmCupnSXQlZQxKoqRLb9/uZ1yOOmSWEwJhBOEFwYtpHy85mbylW/xF1VxWY2cvI/AilT7t8KJAvfv3YcSrAC6Xg8ho5WJdY9wbS6n0oPbvou+TA3NDE1NjNfEgg2MDU3NTw3NDE1IzUhAwZQAQ4WGER1dWlgJUZgd3EABQaHBAoFNYcEDweHBAQF4ZSsDCtFo/JCSHoNWzUaAwZQGCYBHTUTekStnfzUz2OZIW4U1aa+4R4vxhqwP6jxCgsFlw60JBMrcdA5CN5nEzgjYiWPNm/yCIfK2+6mKvxWb15hQHsaSW5Vk0SMwXFnDhWGRII2j4QagIaAHpw4QjL3rJ5FiynRtJUX3SVka2ElZmB3cWxjbGZkcWxqayV1Va+P0N/h+dUMAjK1cHAk");
        private static int[] order = new int[] { 12,48,15,11,36,52,38,32,27,19,15,26,43,18,38,53,43,44,47,28,59,36,34,29,47,46,37,58,58,35,42,52,48,41,54,54,58,42,42,55,43,58,58,55,50,55,47,59,57,51,57,53,58,58,57,57,56,59,58,59,60 };
        private static int key = 5;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif