// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("PVSFjargbzNEYju90pNhJLrZi9BINLXlJC6hPC/0QT3aH27gMf9MQHg77Ei5e0KzJpKXVBc57o2Vl7EvcQsiP5qJcCDPo7ZvPIQTYSIoubm+D7geG+3kyd5MiytDG7Aa7RXESACyMRIAPTY5GrZ4tsc9MTExNTAzyG2OPLLhdfiscBbiVngnw4tDdfhp++mP0kiaedxzFRqJm819I5sHenOJn1EGda0DrJpLJ1XNR/nem9e2mYzzvxM6OKklP1NtYy5ylk/Jb39DVwAvVwCP5QTskidX3MEeHRw3HaedlwA6geyeBp//mRud9wzIHZ9wsjE/MACyMToysjExMOZKj2honyOsTDPWbzo54AyL3Md/p29jv5yjVQaLEnfn6J3swzIzMTAx");
        private static int[] order = new int[] { 9,8,13,6,7,9,9,10,13,9,11,11,13,13,14 };
        private static int key = 48;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
