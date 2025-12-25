namespace Pract15.Services
{
    public static class AuthService
    {
        private const string ManagerPin = "1234";

        public static bool IsManagerMode { get; private set; }

        public static bool LoginAsManager(string pin)
        {
            IsManagerMode = (pin == ManagerPin);
            return IsManagerMode;
        }

        public static void LoginAsVisitor()
        {
            IsManagerMode = false;
        }

        public static void Logout()
        {
            IsManagerMode = false;
        }
    }
}